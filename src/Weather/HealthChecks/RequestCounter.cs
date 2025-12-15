namespace Weather.HealthChecks;

/// <summary>
/// Thread-safe implementation of request counter using atomic operations.
/// </summary>
public class RequestCounter : IRequestCounter
{
    private long _count;

    /// <inheritdoc />
    public long IncrementAndGet() => Interlocked.Increment(ref _count);

    /// <inheritdoc />
    public long CurrentCount => Interlocked.Read(ref _count);
}
