using Weather.Clients;
using Weather.Clients.Models;
using Weather.Models;

namespace Weather.BusinessLogic;

/// <summary>
/// Business logic implementation for precipitation operations.
/// </summary>
public class PrecipitationBusinessLogic : IPrecipitationBusinessLogic
{
    private readonly IPrecipitationSensorClient _sensorClient;

    /// <summary>
    /// Initializes a new instance of <see cref="PrecipitationBusinessLogic"/>.
    /// </summary>
    /// <param name="sensorClient">The precipitation sensor client.</param>
    public PrecipitationBusinessLogic(IPrecipitationSensorClient sensorClient)
    {
        _sensorClient = sensorClient;
    }

    /// <inheritdoc />
    public async Task<PrecipitationData> GetCurrentPrecipitationAsync()
    {
        var sensorData = await _sensorClient.GetPrecipitationAsync();
        return MapToPrecipitationData(sensorData);
    }

    private static PrecipitationData MapToPrecipitationData(SensorPrecipitationResponse source)
    {
        return new PrecipitationData
        {
            Amount = source.PrecipAmount,
            Unit = source.PrecipUnit,
            Type = source.PrecipType,
            Humidity = source.RelativeHumidity
        };
    }
}
