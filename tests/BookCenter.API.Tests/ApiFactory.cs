using BookCenter.Application.Abstractions.Security;
using BookCenter.Application.DTOs.Auth;
using BookCenter.Persistence.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace BookCenter.API.Tests
{
    /// <summary>
    /// Boots the real API in memory with a fresh database (and seeded admin) per factory instance.
    /// <para>
    /// By default the database is EF's InMemory provider: fast, but it doesn't enforce SQL Server's
    /// constraints, unique indexes or rowversion checks. Set <c>BOOKCENTER_TEST_SQLSERVER</c> to a
    /// SQL Server connection string (no database name) to run the same tests against real SQL Server:
    /// each factory then creates its own database from the migrations and drops it afterwards.
    /// </para>
    /// </summary>
    public class ApiFactory : WebApplicationFactory<Program>
    {
        public const string AdminEmail = "admin@test.local";
        public const string AdminPassword = "Admin12345";

        private static readonly string? SqlServer = Environment.GetEnvironmentVariable("BOOKCENTER_TEST_SQLSERVER");

        private readonly string _databaseName = $"BookCenterTests-{Guid.NewGuid()}";

        public static bool UsesSqlServer => !string.IsNullOrWhiteSpace(SqlServer);

        public ApiFactory()
        {
            // The API seeds roles and the admin at startup, so the schema must exist before it boots.
            if (UsesSqlServer)
            {
                using var db = CreateSqlServerContext();
                db.Database.Migrate();
            }
        }

        private string SqlServerConnectionString =>
            new SqlConnectionStringBuilder(SqlServer) { InitialCatalog = _databaseName }.ConnectionString;

        private AppDbContext CreateSqlServerContext() =>
            new(new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(SqlServerConnectionString).Options, new NoCurrentUser());

        private sealed class NoCurrentUser : ICurrentUser
        {
            public string? UserId => null;
            public string? Email => null;
            public bool IsAdmin => false;
            public int? ReaderId => null;
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            if (UsesSqlServer)
            {
                SqlConnection.ClearAllPools(); // otherwise pooled connections keep the database in use
                await using var db = CreateSqlServerContext();
                await db.Database.EnsureDeletedAsync();
            }
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("Jwt:Key", "test-only-signing-key-0123456789-0123456789");
            builder.UseSetting("Seed:AdminEmail", AdminEmail);
            builder.UseSetting("Seed:AdminPassword", AdminPassword);

            builder.ConfigureServices(services =>
            {
                // Drop the app's UseSqlServer(...) registration. Since EF Core 9 it lives in
                // IDbContextOptionsConfiguration, not only in the built DbContextOptions.
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
                if (UsesSqlServer)
                    services.AddDbContext<AppDbContext>(options => options.UseSqlServer(SqlServerConnectionString));
                else
                    services.AddDbContext<AppDbContext>(options => options
                        .UseInMemoryDatabase(_databaseName)
                        // InMemory has no transactions; registration's transaction becomes a no-op here.
                        .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
            });
        }

        public async Task<HttpClient> CreateClientAsAsync(string email, string password)
        {
            var client = CreateClient();
            var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
            response.EnsureSuccessStatusCode();
            var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
            return client;
        }

        public Task<HttpClient> CreateAdminClientAsync() => CreateClientAsAsync(AdminEmail, AdminPassword);

        public async Task SeedAsync(Func<AppDbContext, Task> seed)
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await seed(db);
            await db.SaveChangesAsync();
        }
    }
}
