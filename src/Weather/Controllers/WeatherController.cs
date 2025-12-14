using Microsoft.AspNetCore.Mvc;
using Weather.BusinessLogic;
using Weather.Models;

namespace Weather.Controllers;

/// <summary>
/// Controller for combined weather data.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherBusinessLogic _businessLogic;

    /// <summary>
    /// Initializes a new instance of <see cref="WeatherController"/>.
    /// </summary>
    /// <param name="businessLogic">The weather business logic.</param>
    public WeatherController(IWeatherBusinessLogic businessLogic)
    {
        _businessLogic = businessLogic;
    }

    /// <summary>
    /// Gets current combined weather data from all sensors.
    /// </summary>
    /// <returns>Combined weather data including temperature, wind, and precipitation.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(WeatherData), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WeatherData>> GetCurrentWeather()
    {
        var weather = await _businessLogic.GetCurrentWeatherAsync();
        return Ok(weather);
    }
}
