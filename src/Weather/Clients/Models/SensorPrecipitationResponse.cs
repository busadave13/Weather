using System.Text.Json.Serialization;

namespace Weather.Clients.Models;

/// <summary>
/// Raw precipitation response from the sensor service.
/// </summary>
public record SensorPrecipitationResponse
{
    /// <summary>
    /// Precipitation amount from sensor.
    /// </summary>
    [JsonPropertyName("precipAmount")]
    public decimal PrecipAmount { get; init; }

    /// <summary>
    /// Precipitation unit from sensor.
    /// </summary>
    [JsonPropertyName("precipUnit")]
    public string PrecipUnit { get; init; } = string.Empty;

    /// <summary>
    /// Type of precipitation from sensor.
    /// </summary>
    [JsonPropertyName("precipType")]
    public string PrecipType { get; init; } = string.Empty;

    /// <summary>
    /// Relative humidity percentage from sensor.
    /// </summary>
    [JsonPropertyName("relativeHumidity")]
    public int RelativeHumidity { get; init; }

    /// <summary>
    /// Time of the sensor reading.
    /// </summary>
    [JsonPropertyName("readingTime")]
    public DateTime ReadingTime { get; init; }
}
