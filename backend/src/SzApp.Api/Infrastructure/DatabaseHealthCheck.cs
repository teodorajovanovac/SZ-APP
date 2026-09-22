using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SzApp.Data;

namespace SzApp.Api.Infrastructure;

public sealed class DatabaseHealthCheck(SzAppDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await dbContext.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("SQL Server nije dostupan.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("SQL Server provera nije uspela.", exception);
        }
    }
}
