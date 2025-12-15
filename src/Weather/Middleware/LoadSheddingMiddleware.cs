using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace Weather.Middleware;

/// <summary>
/// Middleware that implements load shedding by rejecting a percentage of requests
/// when the requests per second exceed a configured threshold.
/// </summary>
public class LoadSheddingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoadSheddingMiddleware> _logger;
    private readonly LoadSheddingOptions _options;
    private readonly ConcurrentQueue<DateTime> _requestTimestamps;
    private readonly Random _random;
    private readonly object _cleanupLock = new();

    /// <summary>
    /// Initializes a new instance of <see cref="LoadSheddingMiddleware"/>.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">The load shedding configuration options.</param>
    public LoadSheddingMiddleware(
        RequestDelegate next,
        ILogger<LoadSheddingMiddleware> logger,
        IOptions<LoadSheddingOptions> options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _requestTimestamps = new ConcurrentQueue<DateTime>();
        _random = new Random();
    }

    /// <summary>
    /// Processes the HTTP request, potentially shedding load based on current RPS.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        // Skip if disabled or not a weather endpoint
        if (!_options.Enabled || !IsWeatherEndpoint(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var now = DateTime.UtcNow;
        var windowStart = now.AddSeconds(-_options.WindowDurationSeconds);

        // Track this request
        _requestTimestamps.Enqueue(now);

        // Cleanup old timestamps
        CleanupOldTimestamps(windowStart);

        // Calculate current RPS
        var currentRps = CalculateCurrentRps();

        _logger.LogDebug(
            "Load shedding check: CurrentRPS={CurrentRps}, Threshold={Threshold}, Path={Path}",
            currentRps,
            _options.RpsThreshold,
            context.Request.Path);

        // Check if we're over the threshold
        if (currentRps > _options.RpsThreshold)
        {
            // Randomly decide whether to shed this request
            var randomValue = _random.Next(100);
            if (randomValue < _options.FailurePercentage)
            {
                _logger.LogWarning(
                    "Load shedding triggered: CurrentRPS={CurrentRps} exceeds Threshold={Threshold}. " +
                    "Rejecting request with status {StatusCode}. Path={Path}",
                    currentRps,
                    _options.RpsThreshold,
                    _options.FailureStatusCode,
                    context.Request.Path);

                context.Response.StatusCode = _options.FailureStatusCode;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Service temporarily unavailable due to high load"
                });
                return;
            }

            _logger.LogDebug(
                "Load shedding: Request allowed despite high RPS. CurrentRPS={CurrentRps}, RandomValue={RandomValue}, FailurePercentage={FailurePercentage}",
                currentRps,
                randomValue,
                _options.FailurePercentage);
        }

        await _next(context);
    }

    private bool IsWeatherEndpoint(PathString path)
    {
        return path.StartsWithSegments("/api/weather", StringComparison.OrdinalIgnoreCase);
    }

    private void CleanupOldTimestamps(DateTime windowStart)
    {
        // Use a lock to prevent multiple threads from cleaning up simultaneously
        lock (_cleanupLock)
        {
            while (_requestTimestamps.TryPeek(out var oldest) && oldest < windowStart)
            {
                _requestTimestamps.TryDequeue(out _);
            }
        }
    }

    private int CalculateCurrentRps()
    {
        var now = DateTime.UtcNow;
        var windowStart = now.AddSeconds(-_options.WindowDurationSeconds);
        var count = _requestTimestamps.Count(ts => ts >= windowStart);
        
        // Calculate RPS based on window duration
        return (int)Math.Ceiling((double)count / _options.WindowDurationSeconds);
    }
}
