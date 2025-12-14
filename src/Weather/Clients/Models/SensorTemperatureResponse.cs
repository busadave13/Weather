using System.Text.Json.Serialization;

namespace Weather.Clients.Models;

/// <summary>
/// Raw temperature response from the sensor service.
/// </summary>
public record SensorTemperatureResponse
{
    /// <summary>
    /// Temperature value from sensor.
    /// </summary>
    [JsonPropertyName("temp")]
    public decimal Temp { get; init; }

    /// <summary>
    /// Temperature unit from sensor.
    /// </summary>
    [JsonPropertyName("tempUnit")]
    public string TempUnit { get; init; } = string.Empty;

    /// <summary>
    /// Apparent (feels-like) temperature from sensor.
    /// </summary>
    [JsonPropertyName("apparentTemp")]
    public decimal ApparentTemp { get; init; }

    /// <summary>
    /// Time of the sensor reading.
    /// </summary>
    [JsonPropertyName("readingTime")]
    public DateTime ReadingTime { get; init; }
}
