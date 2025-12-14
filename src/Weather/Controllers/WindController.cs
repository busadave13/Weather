using Microsoft.AspNetCore.Mvc;
using Weather.BusinessLogic;
using Weather.Models;

namespace Weather.Controllers;

/// <summary>
/// Controller for wind data.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WindController : ControllerBase
{
    private readonly IWindBusinessLogic _businessLogic;

    /// <summary>
    /// Initializes a new instance of <see cref="WindController"/>.
    /// </summary>
    /// <param name="businessLogic">The wind business logic.</param>
    public WindController(IWindBusinessLogic businessLogic)
    {
        _businessLogic = businessLogic;
    }

    /// <summary>
    /// Gets current wind data.
    /// </summary>
    /// <returns>Current wind data.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(WindData), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WindData>> GetCurrentWind()
    {
        var wind = await _businessLogic.GetCurrentWindAsync();
        return Ok(wind);
    }
}
