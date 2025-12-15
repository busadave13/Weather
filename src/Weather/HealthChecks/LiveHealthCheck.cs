using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Weather.HealthChecks;

/// <summary>
/// Liveness health check for Kubernetes liveness probe.
/// Indicates whether the application is running and should be kept alive.
/// Fails when request count exceeds (RequestCountThreshold + LiveGracePeriodRequests),
/// allowing in-flight requests to complete before the pod is terminated.
/// </summary>
public class LiveHealthCheck : IHealthCheck
{
    private readonly IRequestCounter _counter;
    private readonly IOptions<HealthCheckOptions> _options;
    private readonly ILogger<LiveHealthCheck> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LiveHealthCheck"/> class.
    /// </summary>
    /// <param name="counter">The request counter service.</param>
    /// <param name="options">The health check configuration options.</param>
    /// <param name="logger">The logger.</param>
    public LiveHealthCheck(
        IRequestCounter counter,
        IOptions<HealthCheckOptions> options,
        ILogger<LiveHealthCheck> logger)
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
        var gracePeriod = _options.Value.LiveGracePeriodRequests;
        var currentCount = _counter.CurrentCount;
        var liveThreshold = threshold + gracePeriod;

        _logger.LogDebug(
            "Live health check: CurrentCount={CurrentCount}, Threshold={Threshold}, GracePeriod={GracePeriod}, LiveThreshold={LiveThreshold}",
            currentCount, threshold, gracePeriod, liveThreshold);

        // If threshold is disabled (0 or negative), always healthy
        if (threshold <= 0)
        {
            return Task.FromResult(HealthCheckResult.Healthy(
                $"Request count: {currentCount} (threshold disabled)"));
        }

        // Check if live threshold exceeded (ready threshold + grace period)
        if (currentCount >= liveThreshold)
        {
            _logger.LogWarning(
                "Live health check failed: Request count {CurrentCount} exceeded live threshold {LiveThreshold}",
                currentCount, liveThreshold);

            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"Request count {currentCount} exceeded live threshold {liveThreshold}"));
        }

        return Task.FromResult(HealthCheckResult.Healthy(
            $"Request count: {currentCount}/{liveThreshold}"));
    }
}
