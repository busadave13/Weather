namespace Weather.HealthChecks;

/// <summary>
/// Interface for tracking request counts across the application.
/// </summary>
public interface IRequestCounter
{
    /// <summary>
    /// Increments the request count and returns the new value.
    /// </summary>
    /// <returns>The incremented count.</returns>
    long IncrementAndGet();

    /// <summary>
    /// Gets the current request count.
    /// </summary>
    long CurrentCount { get; }
}
