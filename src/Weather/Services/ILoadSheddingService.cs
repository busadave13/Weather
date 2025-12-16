namespace Weather.Services;

/// <summary>
/// Service for tracking request rates and determining if load shedding should be applied.
/// </summary>
public interface ILoadSheddingService
{
    /// <summary>
    /// Tracks a request and determines if it should be rejected based on load shedding rules.
    /// </summary>
    /// <returns>True if the request should be rejected, false otherwise.</returns>
    bool ShouldRejectRequest();

    /// <summary>
    /// Gets the current requests per second.
    /// </summary>
    /// <returns>The current RPS.</returns>
    int GetCurrentRps();
}
