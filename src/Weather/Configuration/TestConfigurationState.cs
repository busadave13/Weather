namespace Weather.Configuration;

/// <summary>
/// Thread-safe singleton implementation of test configuration state.
/// Allows runtime modification of health check behavior for testing purposes.
/// </summary>
public class TestConfigurationState : ITestConfigurationState
{
    private volatile bool _forceStartupFail;
    private volatile bool _forceReadyFail;
    private volatile bool _forceLiveFail;
    private volatile int _startupDelayMs;
    private volatile int _readyDelayMs;
    private volatile int _liveDelayMs;

    /// <inheritdoc />
    public bool ForceStartupFail
    {
        get => _forceStartupFail;
        set => _forceStartupFail = value;
    }

    /// <inheritdoc />
    public bool ForceReadyFail
    {
        get => _forceReadyFail;
        set => _forceReadyFail = value;
    }

    /// <inheritdoc />
    public bool ForceLiveFail
    {
        get => _forceLiveFail;
        set => _forceLiveFail = value;
    }

    /// <inheritdoc />
    public int StartupDelayMs
    {
        get => _startupDelayMs;
        set => _startupDelayMs = Math.Max(0, value);
    }

    /// <inheritdoc />
    public int ReadyDelayMs
    {
        get => _readyDelayMs;
        set => _readyDelayMs = Math.Max(0, value);
    }

    /// <inheritdoc />
    public int LiveDelayMs
    {
        get => _liveDelayMs;
        set => _liveDelayMs = Math.Max(0, value);
    }
}
