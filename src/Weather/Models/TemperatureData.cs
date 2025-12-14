namespace Weather.Models;

/// <summary>
/// Temperature data returned by the Weather API.
/// </summary>
public record TemperatureData
{
    /// <summary>
    /// Temperature value.
    /// </summary>
    public decimal Value { get; init; }

    /// <summary>
    /// Temperature unit (e.g., "F" for Fahrenheit, "C" for Celsius).
    /// </summary>
    public string Unit { get; init; } = string.Empty;

    /// <summary>
    /// Feels-like temperature value.
    /// </summary>
    public decimal FeelsLike { get; init; }
}
