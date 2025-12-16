namespace Weather.Configuration;

/// <summary>
/// Interface for runtime test configuration state.
/// Allows dynamic control of health check behavior for testing purposes.
/// </summary>
public interface ITestConfigurationState
{
    /// <summary>
    /// When true, forces the startup health check to return Unhealthy.
    /// </summary>
    bool ForceStartupFail { get; set; }

    /// <summary>
    /// When true, forces the ready health check to return Unhealthy.
    /// </summary>
    bool ForceReadyFail { get; set; }

    /// <summary>
    /// When true, forces the live health check to return Unhealthy.
    /// </summary>
    bool ForceLiveFail { get; set; }

    /// <summary>
    /// Delay in milliseconds to add to the startup health check.
    /// Useful for simulating slow startup or timeout scenarios.
    /// </summary>
    int StartupDelayMs { get; set; }

    /// <summary>
    /// Delay in milliseconds to add to the ready health check.
    /// Useful for simulating slow readiness or timeout scenarios.
    /// </summary>
    int ReadyDelayMs { get; set; }

    /// <summary>
    /// Delay in milliseconds to add to the live health check.
    /// Useful for simulating slow liveness or timeout scenarios.
    /// </summary>
    int LiveDelayMs { get; set; }
}
