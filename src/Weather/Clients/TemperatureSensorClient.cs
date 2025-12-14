using System.Net.Http.Json;
using Weather.Clients.Models;

namespace Weather.Clients;

/// <summary>
/// HTTP client for the temperature sensor service.
/// </summary>
public class TemperatureSensorClient : ITemperatureSensorClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of <see cref="TemperatureSensorClient"/>.
    /// </summary>
    /// <param name="httpClient">The HTTP client configured for the temperature sensor service.</param>
    public TemperatureSensorClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<SensorTemperatureResponse> GetTemperatureAsync()
    {
        return await _httpClient.GetFromJsonAsync<SensorTemperatureResponse>("/api/temperature")
            ?? throw new InvalidOperationException("Failed to deserialize temperature response");
    }
}
