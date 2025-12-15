using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Weather.HealthChecks;

/// <summary>
/// Readiness health check for Kubernetes readiness probe.
/// Indicates whether the application is ready to accept traffic.
/// Fails when request count threshold is exceeded, causing Kubernetes
/// to stop routing new traffic to the pod.
/// </summary>
public class ReadyHealthCheck : IHealthCheck
{
    private readonly IRequestCounter _counter;
    private readonly IOptions<HealthCheckOptions> _options;
    private readonly ILogger<ReadyHealthCheck> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadyHealthCheck"/> class.
    /// </summary>
    /// <param name="counter">The request counter service.</param>
    /// <param name="options">The health check configuration options.</param>
    /// <param name="logger">The logger.</param>
    public ReadyHealthCheck(
        IRequestCounter counter,
        IOptions<HealthCheckOptions> options,
        ILogger<ReadyHealthCheck> logger)
    {
        _counter = counter ?? throw new ArgumentNullException(nameof(counter));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var threshold = _options.Value.RequestCountThreshold;
        var currentCount = _counter.CurrentCount;

        _logger.LogDebug(
            "Ready health check: CurrentCount={CurrentCount}, Threshold={Threshold}",
            currentCount, threshold);

        // If threshold is disabled (0 or negative), always healthy
        if (threshold <= 0)
        {
            return Task.FromResult(HealthCheckResult.Healthy(
                $"Request count: {currentCount} (threshold disabled)"));
        }

        // Check if threshold exceeded
        if (currentCount >= threshold)
        {
            _logger.LogWarning(
                "Ready health check failed: Request count {CurrentCount} exceeded threshold {Threshold}",
                currentCount, threshold);

            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"Request count {currentCount} exceeded threshold {threshold}"));
        }

        return Task.FromResult(HealthCheckResult.Healthy(
            $"Request count: {currentCount}/{threshold}"));
    }
}
