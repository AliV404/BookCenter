using BookCenter.Persistence.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BookCenter.API.Infrastructure
{
    /// <summary>Reports Unhealthy when the API can't reach its database.</summary>
    public sealed class DatabaseHealthCheck : IHealthCheck
    {
        private readonly AppDbContext _db;

        public DatabaseHealthCheck(AppDbContext db)
        {
            _db = db;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) =>
            await _db.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("Cannot connect to the database.");
    }
}
