using Weather.Clients.Models;

namespace Weather.Clients;

/// <summary>
/// Client interface for the precipitation sensor service.
/// </summary>
public interface IPrecipitationSensorClient
{
    /// <summary>
    /// Gets the current precipitation data from the sensor.
    /// </summary>
    /// <returns>Raw precipitation data from the sensor.</returns>
    Task<SensorPrecipitationResponse> GetPrecipitationAsync();
}
