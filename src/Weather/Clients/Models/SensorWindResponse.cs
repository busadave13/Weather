using System.Text.Json.Serialization;

namespace Weather.Clients.Models;

/// <summary>
/// Raw wind response from the sensor service.
/// </summary>
public record SensorWindResponse
{
    /// <summary>
    /// Wind speed from sensor.
    /// </summary>
    [JsonPropertyName("windSpeed")]
    public decimal WindSpeed { get; init; }

    /// <summary>
    /// Speed unit from sensor.
    /// </summary>
    [JsonPropertyName("speedUnit")]
    public string SpeedUnit { get; init; } = string.Empty;

    /// <summary>
    /// Wind direction in degrees (0-360).
    /// </summary>
    [JsonPropertyName("windDirection")]
    public int WindDirection { get; init; }

    /// <summary>
    /// Gust speed from sensor.
    /// </summary>
    [JsonPropertyName("gustSpeed")]
    public decimal GustSpeed { get; init; }

    /// <summary>
    /// Time of the sensor reading.
    /// </summary>
    [JsonPropertyName("readingTime")]
    public DateTime ReadingTime { get; init; }
}
