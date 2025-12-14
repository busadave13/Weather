using Weather.Models;

namespace Weather.BusinessLogic;

/// <summary>
/// Business logic interface for precipitation operations.
/// </summary>
public interface IPrecipitationBusinessLogic
{
    /// <summary>
    /// Gets the current precipitation data.
    /// </summary>
    /// <returns>Current precipitation data.</returns>
    Task<PrecipitationData> GetCurrentPrecipitationAsync();
}
