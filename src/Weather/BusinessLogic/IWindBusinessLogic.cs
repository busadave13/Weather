using Weather.Models;

namespace Weather.BusinessLogic;

/// <summary>
/// Business logic interface for wind operations.
/// </summary>
public interface IWindBusinessLogic
{
    /// <summary>
    /// Gets the current wind data.
    /// </summary>
    /// <returns>Current wind data.</returns>
    Task<WindData> GetCurrentWindAsync();
}
