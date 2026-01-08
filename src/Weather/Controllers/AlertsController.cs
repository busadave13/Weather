using Microsoft.AspNetCore.Mvc;

namespace Weather.Controllers;

/// <summary>
/// Controller for weather alerts.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AlertsController : ControllerBase
{
    private readonly ILogger<AlertsController> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="AlertsController"/>.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public AlertsController(ILogger<AlertsController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets current weather alerts.
    /// </summary>
    /// <returns>No content when no alerts are active.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult GetAlerts()
    {
        return NoContent();
    }
}
