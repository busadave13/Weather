using Microsoft.AspNetCore.Mvc;
using Weather.Middleware;
using Weather.Services;
using Microsoft.Extensions.Options;

namespace Weather.Controllers;

/// <summary>
/// Controller for weather alerts with load shedding protection.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AlertsController : ControllerBase
{
    private readonly ILoadSheddingService _loadSheddingService;
    private readonly LoadSheddingOptions _options;
    private readonly ILogger<AlertsController> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="AlertsController"/>.
    /// </summary>
    /// <param name="loadSheddingService">The load shedding service.</param>
    /// <param name="options">The load shedding options.</param>
    /// <param name="logger">The logger.</param>
    public AlertsController(
        ILoadSheddingService loadSheddingService,
        IOptions<LoadSheddingOptions> options,
        ILogger<AlertsController> logger)
    {
        _loadSheddingService = loadSheddingService ?? throw new ArgumentNullException(nameof(loadSheddingService));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets current weather alerts. Protected by load shedding.
    /// </summary>
    /// <returns>No content on success, or 503 if load shedding is active.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public IActionResult GetAlerts()
    {
        if (_loadSheddingService.ShouldRejectRequest())
        {
            Response.Headers.Append("X-Load-Shedding", "true");
            Response.Headers.Append("Retry-After", "1");

            return StatusCode(_options.FailureStatusCode, new
            {
                error = "Service is under heavy load",
                message = "Please retry your request in a moment",
                retryAfter = 1
            });
        }

        return NoContent();
    }
}
