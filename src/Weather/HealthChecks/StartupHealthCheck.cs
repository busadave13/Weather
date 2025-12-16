using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Weather.Configuration;

namespace Weather.HealthChecks;

/// <summary>
/// Startup health check for Kubernetes startup probe.
/// Indicates whether the application has finished initializing.
/// </summary>
public class StartupHealthCheck : IHealthCheck
{
    private readonly ITestConfigurationState _testState;
    private readonly ILogger<StartupHealthCheck> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartupHealthCheck"/> class.
    /// </summary>
    /// <param name="testState">The test configuration state service.</param>
    /// <param name="logger">The logger.</param>
    public StartupHealthCheck(
        ITestConfigurationState testState,
        ILogger<StartupHealthCheck> logger)
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
        if (_testState.ForceStartupFail)
        {
            _logger.LogWarning("Startup health check forced to fail via /api/config");
            return HealthCheckResult.Unhealthy("Forced failure via /api/config");
        }

        // Apply delay if configured
        var delay = _testState.StartupDelayMs;
        if (delay > 0)
        {
            _logger.LogDebug("Startup health check applying {DelayMs}ms delay", delay);
            await Task.Delay(delay, cancellationToken);
        }

        // Startup check: Application has started successfully
        return HealthCheckResult.Healthy("Application has started");
    }
}
