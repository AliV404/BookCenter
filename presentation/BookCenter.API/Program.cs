using BookCenter.API.Infrastructure;
using BookCenter.Application.ServiceRegistration;
using BookCenter.Persistence.ServiceRegistration;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "Frontend";

builder.Services
    .AddControllers(options => options.Filters.Add<ValidationFilter>())
    // Lets clients send/receive enums as strings, e.g. ?status=overdue
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddCors(options => options.AddPolicy(FrontendCorsPolicy, policy => policy
    .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
    .AllowAnyHeader()
    .AllowAnyMethod()));

builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddApplicationServices();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // In development the React dev server proxies to plain http; redirecting would break that.
    app.UseHttpsRedirection();
}

app.UseCors(FrontendCorsPolicy);

app.MapControllers();

app.Run();

// Makes Program visible to WebApplicationFactory in integration tests.
public partial class Program { }
