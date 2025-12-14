using Microsoft.AspNetCore.Mvc;
using Weather.BusinessLogic;
using Weather.Models;

namespace Weather.Controllers;

/// <summary>
/// Controller for weather data operations.
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

    /// <summary>
    /// Gets current temperature data.
    /// </summary>
    /// <returns>Current temperature data.</returns>
    [HttpGet("temperature")]
    [ProducesResponseType(typeof(TemperatureData), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TemperatureData>> GetTemperature()
    {
        var temperature = await _businessLogic.GetTemperatureAsync();
        return Ok(temperature);
    }

    /// <summary>
    /// Gets current wind data.
    /// </summary>
    /// <returns>Current wind data.</returns>
    [HttpGet("wind")]
    [ProducesResponseType(typeof(WindData), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WindData>> GetWind()
    {
        var wind = await _businessLogic.GetWindAsync();
        return Ok(wind);
    }

    /// <summary>
    /// Gets current precipitation data.
    /// </summary>
    /// <returns>Current precipitation data.</returns>
    [HttpGet("precipitation")]
    [ProducesResponseType(typeof(PrecipitationData), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PrecipitationData>> GetPrecipitation()
    {
        var precipitation = await _businessLogic.GetPrecipitationAsync();
        return Ok(precipitation);
    }
}
