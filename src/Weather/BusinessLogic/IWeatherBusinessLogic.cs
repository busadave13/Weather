using Weather.Models;

namespace Weather.BusinessLogic;

/// <summary>
/// Business logic interface for combined weather operations.
/// </summary>
public interface IWeatherBusinessLogic
{
    /// <summary>
    /// Gets the current combined weather data from all sensors.
    /// </summary>
    /// <returns>Current combined weather data.</returns>
    Task<WeatherData> GetCurrentWeatherAsync();
}
