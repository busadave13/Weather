namespace Weather.Middleware;

/// <summary>
/// Configuration options for the load shedding middleware.
/// </summary>
public class LoadSheddingOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether load shedding is enabled.
    /// Default is false.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the requests per second threshold above which load shedding activates.
    /// Default is 100 RPS.
    /// </summary>
    public int RpsThreshold { get; set; } = 100;

    /// <summary>
    /// Gets or sets the percentage of requests to reject when load shedding is active (0-100).
    /// Default is 25%.
    /// </summary>
    public int FailurePercentage { get; set; } = 25;

    /// <summary>
    /// Gets or sets the HTTP status code to return when rejecting requests.
    /// Default is 503 (Service Unavailable).
    /// </summary>
    public int FailureStatusCode { get; set; } = 503;

    /// <summary>
    /// Gets or sets the sliding window size in seconds for RPS calculation.
    /// Default is 1 second.
    /// </summary>
    public int WindowSizeSeconds { get; set; } = 1;
}
