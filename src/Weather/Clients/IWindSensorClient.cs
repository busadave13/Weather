using Weather.Clients.Models;

namespace Weather.Clients;

/// <summary>
/// Client interface for the wind sensor service.
/// </summary>
public interface IWindSensorClient
{
    /// <summary>
    /// Gets the current wind data from the sensor.
    /// </summary>
    /// <returns>Raw wind data from the sensor.</returns>
    Task<SensorWindResponse> GetWindAsync();
}
