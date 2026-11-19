using BookCenter.Persistence.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BookCenter.API.Tests
{
    /// <summary>
    /// Boots the real API in memory and swaps SQL Server for EF's InMemory provider,
    /// with a fresh database per factory instance.
    /// </summary>
    public class ApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = $"BookCenterTests-{Guid.NewGuid()}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_databaseName));
            });
        }

        public async Task SeedAsync(Func<AppDbContext, Task> seed)
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await seed(db);
            await db.SaveChangesAsync();
        }
    }
}
