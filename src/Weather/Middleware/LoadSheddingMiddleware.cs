using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace Weather.Middleware;

/// <summary>
/// Middleware that implements load shedding based on requests per second (RPS).
/// When RPS exceeds the configured threshold, a percentage of requests are rejected.
/// </summary>
public class LoadSheddingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly LoadSheddingOptions _options;
    private readonly ILogger<LoadSheddingMiddleware> _logger;
    private readonly ConcurrentQueue<DateTime> _requestTimestamps;
    private readonly Random _random;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadSheddingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="options">The load shedding configuration options.</param>
    /// <param name="logger">The logger.</param>
    public LoadSheddingMiddleware(
        RequestDelegate next,
        IOptions<LoadSheddingOptions> options,
        ILogger<LoadSheddingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _requestTimestamps = new ConcurrentQueue<DateTime>();
        _random = new Random();
    }

    /// <summary>
    /// Processes the request, potentially rejecting it based on load shedding rules.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        // If load shedding is disabled, pass through
        if (!_options.Enabled)
        {
            await _next(context);
            return;
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
            if (ShouldRejectRequest())
            {
                _logger.LogWarning(
                    "Load shedding: Rejecting request. RPS: {CurrentRps}, Threshold: {Threshold}, FailurePercentage: {FailurePercentage}%",
                    currentRps,
                    _options.RpsThreshold,
                    _options.FailurePercentage);

                context.Response.StatusCode = _options.FailureStatusCode;
                context.Response.Headers.Append("X-Load-Shedding", "true");
                context.Response.Headers.Append("Retry-After", "1");
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Service is under heavy load",
                    message = "Please retry your request in a moment",
                    retryAfter = 1
                });
                return;
            }
        }

        await _next(context);
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
    private bool ShouldRejectRequest()
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
