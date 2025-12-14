using System.Net.Http.Json;
using Weather.Clients.Models;

namespace Weather.Clients;

/// <summary>
/// HTTP client for the wind sensor service.
/// </summary>
public class WindSensorClient : IWindSensorClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of <see cref="WindSensorClient"/>.
    /// </summary>
    /// <param name="httpClient">The HTTP client configured for the wind sensor service.</param>
    public WindSensorClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<SensorWindResponse> GetWindAsync()
    {
        return await _httpClient.GetFromJsonAsync<SensorWindResponse>("/api/wind")
            ?? throw new InvalidOperationException("Failed to deserialize wind response");
    }
}
