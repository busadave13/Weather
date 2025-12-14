using Weather.Clients.Models;

namespace Weather.Clients;

/// <summary>
/// Client interface for the temperature sensor service.
/// </summary>
public interface ITemperatureSensorClient
{
    /// <summary>
    /// Gets the current temperature data from the sensor.
    /// </summary>
    /// <returns>Raw temperature data from the sensor.</returns>
    Task<SensorTemperatureResponse> GetTemperatureAsync();
}
