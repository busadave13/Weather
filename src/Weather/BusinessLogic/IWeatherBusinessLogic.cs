using Weather.Models;

namespace Weather.BusinessLogic;

/// <summary>
/// Business logic interface for weather operations.
/// </summary>
public interface IWeatherBusinessLogic
{
    /// <summary>
    /// Gets the current combined weather data from all sensors.
    /// </summary>
    /// <returns>Current combined weather data.</returns>
    Task<WeatherData> GetCurrentWeatherAsync();

    /// <summary>
    /// Gets the current temperature data.
    /// </summary>
    /// <returns>Current temperature data.</returns>
    Task<TemperatureData> GetTemperatureAsync();

    /// <summary>
    /// Gets the current wind data.
    /// </summary>
    /// <returns>Current wind data.</returns>
    Task<WindData> GetWindAsync();

    /// <summary>
    /// Gets the current precipitation data.
    /// </summary>
    /// <returns>Current precipitation data.</returns>
    Task<PrecipitationData> GetPrecipitationAsync();
}
