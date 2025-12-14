namespace Weather.Models;

/// <summary>
/// Precipitation data returned by the Weather API.
/// </summary>
public record PrecipitationData
{
    /// <summary>
    /// Precipitation amount.
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// Precipitation unit (e.g., "in", "mm").
    /// </summary>
    public string Unit { get; init; } = string.Empty;

    /// <summary>
    /// Type of precipitation (e.g., "none", "rain", "snow").
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Relative humidity percentage (0-100).
    /// </summary>
    public int Humidity { get; init; }
}
