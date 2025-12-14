using Weather.Clients;
using Weather.Clients.Models;
using Weather.Models;

namespace Weather.BusinessLogic;

/// <summary>
/// Business logic implementation for weather operations.
/// </summary>
public class WeatherBusinessLogic : IWeatherBusinessLogic
{
    private readonly ITemperatureSensorClient _temperatureClient;
    private readonly IWindSensorClient _windClient;
    private readonly IPrecipitationSensorClient _precipitationClient;

    /// <summary>
    /// Initializes a new instance of <see cref="WeatherBusinessLogic"/>.
    /// </summary>
    /// <param name="temperatureClient">The temperature sensor client.</param>
    /// <param name="windClient">The wind sensor client.</param>
    /// <param name="precipitationClient">The precipitation sensor client.</param>
    public WeatherBusinessLogic(
        ITemperatureSensorClient temperatureClient,
        IWindSensorClient windClient,
        IPrecipitationSensorClient precipitationClient)
    {
        _temperatureClient = temperatureClient;
        _windClient = windClient;
        _precipitationClient = precipitationClient;
    }

    /// <inheritdoc />
    public async Task<WeatherData> GetCurrentWeatherAsync()
    {
        // Parallel calls to all sensors
        var tempTask = _temperatureClient.GetTemperatureAsync();
        var windTask = _windClient.GetWindAsync();
        var precipTask = _precipitationClient.GetPrecipitationAsync();

        await Task.WhenAll(tempTask, windTask, precipTask);

        // Map all sensor responses to Weather models
        return new WeatherData
        {
            Temperature = MapToTemperatureData(await tempTask),
            Wind = MapToWindData(await windTask),
            Precipitation = MapToPrecipitationData(await precipTask),
            Timestamp = DateTime.UtcNow
        };
    }

    /// <inheritdoc />
    public async Task<TemperatureData> GetTemperatureAsync()
    {
        var response = await _temperatureClient.GetTemperatureAsync();
        return MapToTemperatureData(response);
    }

    /// <inheritdoc />
    public async Task<WindData> GetWindAsync()
    {
        var response = await _windClient.GetWindAsync();
        return MapToWindData(response);
    }

    /// <inheritdoc />
    public async Task<PrecipitationData> GetPrecipitationAsync()
    {
        var response = await _precipitationClient.GetPrecipitationAsync();
        return MapToPrecipitationData(response);
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

    private static WindData MapToWindData(SensorWindResponse source)
    {
        string[] cardinals = ["N", "NE", "E", "SE", "S", "SW", "W", "NW"];
        int index = (int)Math.Round(source.WindDirection / 45.0) % 8;

        return new WindData
        {
            Speed = source.WindSpeed,
            Unit = source.SpeedUnit,
            Direction = cardinals[index],
            Gusts = source.GustSpeed
        };
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
