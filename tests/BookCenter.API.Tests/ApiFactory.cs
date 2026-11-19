using BookCenter.Application.DTOs.Auth;
using BookCenter.Persistence.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace BookCenter.API.Tests
{
    /// <summary>
    /// Boots the real API in memory and swaps SQL Server for EF's InMemory provider,
    /// with a fresh database (and seeded admin) per factory instance.
    /// </summary>
    public class ApiFactory : WebApplicationFactory<Program>
    {
        public const string AdminEmail = "admin@test.local";
        public const string AdminPassword = "Admin12345";

        private readonly string _databaseName = $"BookCenterTests-{Guid.NewGuid()}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("Jwt:Key", "test-only-signing-key-0123456789-0123456789");
            builder.UseSetting("Seed:AdminEmail", AdminEmail);
            builder.UseSetting("Seed:AdminPassword", AdminPassword);

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<AppDbContext>>();
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
