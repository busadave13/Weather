using Weather.Clients;
using Weather.Clients.Models;
using Weather.Models;

namespace Weather.BusinessLogic;

/// <summary>
/// Business logic implementation for temperature operations.
/// </summary>
public class TemperatureBusinessLogic : ITemperatureBusinessLogic
{
    private readonly ITemperatureSensorClient _sensorClient;

    /// <summary>
    /// Initializes a new instance of <see cref="TemperatureBusinessLogic"/>.
    /// </summary>
    /// <param name="sensorClient">The temperature sensor client.</param>
    public TemperatureBusinessLogic(ITemperatureSensorClient sensorClient)
    {
        _sensorClient = sensorClient;
    }

    /// <inheritdoc />
    public async Task<TemperatureData> GetCurrentTemperatureAsync()
    {
        var sensorData = await _sensorClient.GetTemperatureAsync();
        return MapToTemperatureData(sensorData);
    }

    private static TemperatureData MapToTemperatureData(SensorTemperatureResponse source)
    {
        return new TemperatureData
        {
            Value = source.Temp,
            Unit = source.TempUnit,
            FeelsLike = source.ApparentTemp
        };
    }
}
