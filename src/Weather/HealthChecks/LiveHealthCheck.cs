using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Weather.Configuration;

namespace Weather.HealthChecks;

/// <summary>
/// Liveness health check for Kubernetes liveness probe.
/// Indicates whether the application is running and should be kept alive.
/// Can be configured via /api/config to force failure or add delays for testing.
/// </summary>
public class LiveHealthCheck : IHealthCheck
{
    private readonly ITestConfigurationState _testState;
    private readonly ILogger<LiveHealthCheck> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LiveHealthCheck"/> class.
    /// </summary>
    /// <param name="testState">The test configuration state service.</param>
    /// <param name="logger">The logger.</param>
    public LiveHealthCheck(
        ITestConfigurationState testState,
        ILogger<LiveHealthCheck> logger)
    {
        _testState = testState ?? throw new ArgumentNullException(nameof(testState));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // Check for forced failure first
        if (_testState.ForceLiveFail)
        {
            _logger.LogWarning("Live health check forced to fail via /api/config");
            return HealthCheckResult.Unhealthy("Forced failure via /api/config");
        }

        // Apply delay if configured
        var delay = _testState.LiveDelayMs;
        if (delay > 0)
        {
            _logger.LogDebug("Live health check applying {DelayMs}ms delay", delay);
            await Task.Delay(delay, cancellationToken);
        }

        return HealthCheckResult.Healthy("Application is alive");
    }
}
