using BookCenter.API.Infrastructure;
using BookCenter.Application.Abstractions.Security;
using BookCenter.Application.Abstractions.Services;
using BookCenter.Application.Common;
using BookCenter.Application.ServiceRegistration;
using BookCenter.Infrastructure.Security;
using BookCenter.Infrastructure.ServiceRegistration;
using BookCenter.Persistence.Data;
using BookCenter.Persistence.Identity;
using BookCenter.Persistence.ServiceRegistration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Security.Claims;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "Frontend";

builder.Services
    .AddControllers(options => options.Filters.Add<ValidationFilter>())
    // Lets clients send/receive enums as strings, e.g. ?status=overdue
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Adds the "Authorize" button to Swagger UI: paste the token from /api/auth/login.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste the token from POST /api/auth/login.",
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = [],
    });
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("database");

builder.Services.AddCors(options => options.AddPolicy(FrontendCorsPolicy, policy => policy
    .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
    .AllowAnyHeader()
    .AllowAnyMethod()));

// ---------- Authentication & authorization
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((bearer, jwtOptions) =>
    {
        var jwt = jwtOptions.Value;
        // Keep claim names exactly as issued ("sub", "role", ...) instead of mapping them to long URIs.
        bearer.MapInboundClaims = false;
        bearer.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = TokenService.CreateSigningKey(jwt.Key),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            NameClaimType = AppClaims.Name,
            RoleClaimType = AppClaims.Role,
            ClockSkew = TimeSpan.FromSeconds(30),
        };
        bearer.Events = new JwtBearerEvents
        {
            // A JWT is valid until it expires, even if an admin locks the account or changes its
            // roles. Comparing the token's security stamp with the database closes that gap.
            OnTokenValidated = async context =>
            {
                var principal = context.Principal!;
                var auth = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();
                var valid = await auth.IsSessionValidAsync(
                    principal.FindFirstValue(AppClaims.UserId) ?? "",
                    principal.FindFirstValue(AppClaims.SecurityStamp));
                if (!valid)
                    context.Fail("This session has ended. Please sign in again.");
            },
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Secure by default: every endpoint needs a signed-in user unless it opts out with [AllowAnonymous].
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddPersistenceServices(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages(); // gives empty 401/403 responses a ProblemDetails body

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else if (app.Configuration.GetValue("HttpsRedirection", true))
{
    // Not in development: the React dev server proxies to plain http there.
    // Also off in Docker (HttpsRedirection=false), where the API only sits behind nginx.
    app.UseHttpsRedirection();
}

app.UseCors(FrontendCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
// For Docker/monitoring: 200 "Healthy" when the API is up and can reach the database.
app.MapHealthChecks("/api/health").AllowAnonymous();

// Containers set Database__MigrateOnStartup=true; locally run `dotnet ef database update` instead.
if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
    await app.Services.MigrateDatabaseAsync();

// Roles + first admin account (from Seed:AdminEmail / Seed:AdminPassword). Requires migrations to be applied.
await app.Services.SeedIdentityAsync(app.Configuration);

app.Run();

// Makes Program visible to WebApplicationFactory in integration tests.
public partial class Program { }
