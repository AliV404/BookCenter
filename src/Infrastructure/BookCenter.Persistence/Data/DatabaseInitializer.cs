using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BookCenter.Persistence.Data
{
    public static class DatabaseInitializer
    {
        /// <summary>
        /// Creates or upgrades the database to the latest migration. Used in containers
        /// (Database:MigrateOnStartup=true); locally you run `dotnet ef database update` instead.
        /// Retries for a while because a freshly started SQL Server container can refuse logins
        /// for some seconds even after it starts listening.
        /// </summary>
        public static async Task MigrateDatabaseAsync(this IServiceProvider services, int attempts = 10)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DatabaseInitializer));

            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    await db.Database.MigrateAsync();
                    logger.LogInformation("Database is up to date.");
                    return;
                }
                catch (Exception ex) when (attempt < attempts && ex is Microsoft.Data.SqlClient.SqlException or TimeoutException)
                {
                    logger.LogWarning("Database not reachable yet (attempt {Attempt}/{Attempts}): {Message}", attempt, attempts, ex.Message);
                    await Task.Delay(TimeSpan.FromSeconds(3));
                }
            }
        }
    }
}
