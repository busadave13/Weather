using Microsoft.Extensions.Logging;

namespace Weather.HealthChecks;

/// <summary>
/// Middleware that counts incoming requests for health check monitoring.
/// </summary>
public class RequestCounterMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRequestCounter _counter;
    private readonly ILogger<RequestCounterMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestCounterMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="counter">The request counter service.</param>
    /// <param name="logger">The logger.</param>
    public RequestCounterMiddleware(
        RequestDelegate next,
        IRequestCounter counter,
        ILogger<RequestCounterMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _counter = counter ?? throw new ArgumentNullException(nameof(counter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Invokes the middleware to count the request.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        // Only count non-health-check requests
        if (!context.Request.Path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase))
        {
            var count = _counter.IncrementAndGet();
            _logger.LogDebug("Request counted: {Path}, Total count: {Count}", context.Request.Path, count);
        }

        await _next(context);
    }
}
