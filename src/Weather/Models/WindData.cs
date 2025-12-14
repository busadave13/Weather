namespace Weather.Models;

/// <summary>
/// Wind data returned by the Weather API.
/// </summary>
public record WindData
{
    /// <summary>
    /// Wind speed value.
    /// </summary>
    public decimal Speed { get; init; }

    /// <summary>
    /// Speed unit (e.g., "mph", "km/h").
    /// </summary>
    public string Unit { get; init; } = string.Empty;

    /// <summary>
    /// Wind direction as cardinal direction (e.g., "N", "NE", "NW").
    /// </summary>
    public string Direction { get; init; } = string.Empty;

    /// <summary>
    /// Wind gust speed.
    /// </summary>
    public decimal Gusts { get; init; }
}
