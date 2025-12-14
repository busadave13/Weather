using Microsoft.AspNetCore.Mvc;
using Weather.BusinessLogic;
using Weather.Models;

namespace Weather.Controllers;

/// <summary>
/// Controller for precipitation data.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PrecipitationController : ControllerBase
{
    private readonly IPrecipitationBusinessLogic _businessLogic;

    /// <summary>
    /// Initializes a new instance of <see cref="PrecipitationController"/>.
    /// </summary>
    /// <param name="businessLogic">The precipitation business logic.</param>
    public PrecipitationController(IPrecipitationBusinessLogic businessLogic)
    {
        _businessLogic = businessLogic;
    }

    /// <summary>
    /// Gets current precipitation data.
    /// </summary>
    /// <returns>Current precipitation data.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PrecipitationData), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PrecipitationData>> GetCurrentPrecipitation()
    {
        var precipitation = await _businessLogic.GetCurrentPrecipitationAsync();
        return Ok(precipitation);
    }
}
