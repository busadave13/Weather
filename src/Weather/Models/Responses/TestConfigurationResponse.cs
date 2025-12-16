namespace Weather.Models.Responses;

/// <summary>
/// Response DTO for test configuration from GET/POST /api/config.
/// Returns the current state of all test configuration settings.
/// </summary>
public class TestConfigurationResponse
{
    /// <summary>
    /// When true, the startup health check is forced to return Unhealthy.
    /// </summary>
    public bool ForceStartupFail { get; set; }

    /// <summary>
    /// When true, the ready health check is forced to return Unhealthy.
    /// </summary>
    public bool ForceReadyFail { get; set; }

    /// <summary>
    /// When true, the live health check is forced to return Unhealthy.
    /// </summary>
    public bool ForceLiveFail { get; set; }

    /// <summary>
    /// Current delay in milliseconds applied to the startup health check.
    /// </summary>
    public int StartupDelayMs { get; set; }

    /// <summary>
    /// Current delay in milliseconds applied to the ready health check.
    /// </summary>
    public int ReadyDelayMs { get; set; }

    /// <summary>
    /// Current delay in milliseconds applied to the live health check.
    /// </summary>
    public int LiveDelayMs { get; set; }
}
