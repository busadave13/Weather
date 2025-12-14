using Microsoft.AspNetCore.Mvc;
using Weather.BusinessLogic;
using Weather.Models;

namespace Weather.Controllers;

/// <summary>
/// Controller for temperature data.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TemperatureController : ControllerBase
{
    private readonly ITemperatureBusinessLogic _businessLogic;

    /// <summary>
    /// Initializes a new instance of <see cref="TemperatureController"/>.
    /// </summary>
    /// <param name="businessLogic">The temperature business logic.</param>
    public TemperatureController(ITemperatureBusinessLogic businessLogic)
    {
        _businessLogic = businessLogic;
    }

    /// <summary>
    /// Gets current temperature data.
    /// </summary>
    /// <returns>Current temperature data.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(TemperatureData), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TemperatureData>> GetCurrentTemperature()
    {
        var temperature = await _businessLogic.GetCurrentTemperatureAsync();
        return Ok(temperature);
    }
}
