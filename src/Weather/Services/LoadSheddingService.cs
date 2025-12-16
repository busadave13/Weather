using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Weather.Middleware;

namespace Weather.Services;

/// <summary>
/// Service that implements load shedding based on requests per second (RPS).
/// When RPS exceeds the configured threshold, a percentage of requests are rejected.
/// </summary>
public class LoadSheddingService : ILoadSheddingService
{
    private readonly LoadSheddingOptions _options;
    private readonly ILogger<LoadSheddingService> _logger;
    private readonly ConcurrentQueue<DateTime> _requestTimestamps;
    private readonly Random _random;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadSheddingService"/> class.
    /// </summary>
    /// <param name="options">The load shedding configuration options.</param>
    /// <param name="logger">The logger.</param>
    public LoadSheddingService(
        IOptions<LoadSheddingOptions> options,
        ILogger<LoadSheddingService> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _requestTimestamps = new ConcurrentQueue<DateTime>();
        _random = new Random();
    }

    /// <inheritdoc />
    public bool ShouldRejectRequest()
    {
        // If load shedding is disabled, never reject
        if (!_options.Enabled)
        {
            return false;
        }

        // Track this request and clean up old timestamps
        var now = DateTime.UtcNow;
        _requestTimestamps.Enqueue(now);
        CleanupOldTimestamps(now);

        // Calculate current RPS
        var currentRps = CalculateRps();

        // Check if we should shed load
        if (currentRps > _options.RpsThreshold)
        {
            // Determine if this specific request should be rejected
            if (ShouldRejectBasedOnPercentage())
            {
                _logger.LogWarning(
                    "Load shedding: Rejecting request. RPS: {CurrentRps}, Threshold: {Threshold}, FailurePercentage: {FailurePercentage}%",
                    currentRps,
                    _options.RpsThreshold,
                    _options.FailurePercentage);

                return true;
            }
        }

        return false;
    }

    /// <inheritdoc />
    public int GetCurrentRps()
    {
        CleanupOldTimestamps(DateTime.UtcNow);
        return CalculateRps();
    }

    /// <summary>
    /// Removes timestamps older than the sliding window from the queue.
    /// </summary>
    private void CleanupOldTimestamps(DateTime now)
    {
        var windowStart = now.AddSeconds(-_options.WindowSizeSeconds);

        while (_requestTimestamps.TryPeek(out var oldest) && oldest < windowStart)
        {
            _requestTimestamps.TryDequeue(out _);
        }
    }

    /// <summary>
    /// Calculates the current requests per second based on the sliding window.
    /// </summary>
    private int CalculateRps()
    {
        return _requestTimestamps.Count / Math.Max(1, _options.WindowSizeSeconds);
    }

    /// <summary>
    /// Determines whether the current request should be rejected based on failure percentage.
    /// </summary>
    private bool ShouldRejectBasedOnPercentage()
    {
        if (_options.FailurePercentage <= 0)
        {
            return false;
        }

        if (_options.FailurePercentage >= 100)
        {
            return true;
        }

        return _random.Next(100) < _options.FailurePercentage;
    }
}
