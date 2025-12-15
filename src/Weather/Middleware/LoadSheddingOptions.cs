namespace Weather.Middleware;

/// <summary>
/// Configuration options for load shedding middleware.
/// </summary>
public class LoadSheddingOptions
{
    /// <summary>
    /// Gets or sets whether load shedding is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the requests per second threshold that triggers load shedding.
    /// When the current RPS exceeds this value, a percentage of requests will be rejected.
    /// </summary>
    public int RpsThreshold { get; set; } = 100;

    /// <summary>
    /// Gets or sets the percentage of requests to fail when over the RPS threshold (0-100).
    /// </summary>
    public int FailurePercentage { get; set; } = 25;

    /// <summary>
    /// Gets or sets the HTTP status code to return for shed requests.
    /// </summary>
    public int FailureStatusCode { get; set; } = 503;

    /// <summary>
    /// Gets or sets the sliding window duration in seconds for calculating RPS.
    /// </summary>
    public int WindowDurationSeconds { get; set; } = 1;
}
