using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Weather.HealthChecks;

/// <summary>
/// Startup health check for Kubernetes startup probe.
/// Indicates whether the application has finished initializing.
/// </summary>
public class StartupHealthCheck : IHealthCheck
{
    /// <inheritdoc />
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // Startup check: Application has started successfully
        return Task.FromResult(HealthCheckResult.Healthy("Application has started"));
    }
}
