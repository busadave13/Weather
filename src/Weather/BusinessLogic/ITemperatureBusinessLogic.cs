using Weather.Models;

namespace Weather.BusinessLogic;

/// <summary>
/// Business logic interface for temperature operations.
/// </summary>
public interface ITemperatureBusinessLogic
{
    /// <summary>
    /// Gets the current temperature data.
    /// </summary>
    /// <returns>Current temperature data.</returns>
    Task<TemperatureData> GetCurrentTemperatureAsync();
}
