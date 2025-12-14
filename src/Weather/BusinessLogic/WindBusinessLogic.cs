using Weather.Clients;
using Weather.Clients.Models;
using Weather.Models;

namespace Weather.BusinessLogic;

/// <summary>
/// Business logic implementation for wind operations.
/// </summary>
public class WindBusinessLogic : IWindBusinessLogic
{
    private readonly IWindSensorClient _sensorClient;

    /// <summary>
    /// Initializes a new instance of <see cref="WindBusinessLogic"/>.
    /// </summary>
    /// <param name="sensorClient">The wind sensor client.</param>
    public WindBusinessLogic(IWindSensorClient sensorClient)
    {
        _sensorClient = sensorClient;
    }

    /// <inheritdoc />
    public async Task<WindData> GetCurrentWindAsync()
    {
        var sensorData = await _sensorClient.GetWindAsync();
        return MapToWindData(sensorData);
    }

    private static WindData MapToWindData(SensorWindResponse source)
    {
        return new WindData
        {
            Speed = source.WindSpeed,
            Unit = source.SpeedUnit,
            Direction = ConvertDegreesToCardinal(source.WindDirection),
            Gusts = source.GustSpeed
        };
    }

    private static string ConvertDegreesToCardinal(int degrees)
    {
        string[] cardinals = ["N", "NE", "E", "SE", "S", "SW", "W", "NW"];
        int index = (int)Math.Round(degrees / 45.0) % 8;
        return cardinals[index];
    }
}
