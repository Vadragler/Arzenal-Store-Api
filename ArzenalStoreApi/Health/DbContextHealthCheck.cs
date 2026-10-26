using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ArzenalStoreApi.Health;

/// <summary>
/// Vérifie qu'un contexte EF Core peut établir une connexion avec sa base.
/// </summary>
public sealed class DbContextHealthCheck<TDbContext>(IServiceScopeFactory scopeFactory) : IHealthCheck
    where TDbContext : DbContext
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

            return await dbContext.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("La base de données est inaccessible.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("La base de données est inaccessible.", exception);
        }
    }
}
