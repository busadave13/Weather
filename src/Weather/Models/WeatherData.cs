namespace Weather.Models;

/// <summary>
/// Combined weather data returned by the Weather API.
/// </summary>
public record WeatherData
{
    /// <summary>
    /// Temperature data.
    /// </summary>
    public required TemperatureData Temperature { get; init; }

    /// <summary>
    /// Wind data.
    /// </summary>
    public required WindData Wind { get; init; }

    /// <summary>
    /// Precipitation data.
    /// </summary>
    public required PrecipitationData Precipitation { get; init; }

    /// <summary>
    /// Timestamp of the weather reading.
    /// </summary>
    public DateTime Timestamp { get; init; }
}
