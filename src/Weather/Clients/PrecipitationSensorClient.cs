using System.Net.Http.Json;
using Weather.Clients.Models;

namespace Weather.Clients;

/// <summary>
/// HTTP client for the precipitation sensor service.
/// </summary>
public class PrecipitationSensorClient : IPrecipitationSensorClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of <see cref="PrecipitationSensorClient"/>.
    /// </summary>
    /// <param name="httpClient">The HTTP client configured for the precipitation sensor service.</param>
    public PrecipitationSensorClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<SensorPrecipitationResponse> GetPrecipitationAsync()
    {
        return await _httpClient.GetFromJsonAsync<SensorPrecipitationResponse>("/api/precipitation")
            ?? throw new InvalidOperationException("Failed to deserialize precipitation response");
    }
}
