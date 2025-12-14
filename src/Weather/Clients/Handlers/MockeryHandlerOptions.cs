namespace Weather.Clients.Handlers;

/// <summary>
/// Configuration options for the MockeryHandler.
/// </summary>
public class MockeryHandlerOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the MockeryHandler is enabled.
    /// When disabled, requests proceed directly to the target service without mock interception.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the base URL of the Mockery service.
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:5000";
}
