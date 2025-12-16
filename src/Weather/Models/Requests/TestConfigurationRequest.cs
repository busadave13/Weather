namespace Weather.Models.Requests;

/// <summary>
/// Request DTO for updating test configuration via POST /api/config.
/// All properties are nullable - only specified properties will be updated.
/// </summary>
public class TestConfigurationRequest
{
    /// <summary>
    /// When true, forces the startup health check to return Unhealthy.
    /// </summary>
    public bool? ForceStartupFail { get; set; }

    /// <summary>
    /// When true, forces the ready health check to return Unhealthy.
    /// </summary>
    public bool? ForceReadyFail { get; set; }

    /// <summary>
    /// When true, forces the live health check to return Unhealthy.
    /// </summary>
    public bool? ForceLiveFail { get; set; }

    /// <summary>
    /// Delay in milliseconds to add to the startup health check.
    /// Set to 0 to disable delay.
    /// </summary>
    public int? StartupDelayMs { get; set; }

    /// <summary>
    /// Delay in milliseconds to add to the ready health check.
    /// Set to 0 to disable delay.
    /// </summary>
    public int? ReadyDelayMs { get; set; }

    /// <summary>
    /// Delay in milliseconds to add to the live health check.
    /// Set to 0 to disable delay.
    /// </summary>
    public int? LiveDelayMs { get; set; }
}
