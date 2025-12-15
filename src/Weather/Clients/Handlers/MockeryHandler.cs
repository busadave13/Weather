#nullable enable

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Weather.Clients.Handlers;

/// <summary>
/// A delegating handler that propagates the X-Mock-ID header from incoming requests
/// and routes requests to the Mockery service when the header is present.
/// </summary>
/// <remarks>
/// This handler is self-contained and combines header propagation with mock routing.
/// It uses IHttpClientFactory to create its own HttpClient for calling the Mockery service.
/// 
/// When processing an outgoing request, this handler will:
/// 1. Check the incoming HTTP request (via IHttpContextAccessor) for the X-Mock-ID header
/// 2. If present, propagate the header to the outgoing request
/// 3. Route the request to the Mockery service instead of the original endpoint
/// 4. Return the mocked response from the Mockery service
/// 
/// If the X-Mock-ID header is not present, the request proceeds normally to the next handler.
/// </remarks>
public class MockeryHandler : DelegatingHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<MockeryHandler> _logger;
    private readonly MockeryHandlerOptions _options;

    /// <summary>
    /// The name of the header that triggers mock routing with a direct mock ID.
    /// </summary>
    public const string MockIdHeaderName = "X-Mock-ID";

    /// <summary>
    /// The name of the header that contains a comma-delimited list of mock identifiers.
    /// Each mock identifier should have the service name as the first segment (e.g., "wind/prod/success").
    /// The handler will match mocks based on the ServiceName and randomly select one if multiple match.
    /// </summary>
    public const string MockeryMocksHeaderName = "X-Mockery-Mocks";

    /// <summary>
    /// The name of the HttpClient used to call the Mockery service.
    /// </summary>
    private const string MockeryClientName = "MockeryClient";

    /// <summary>
    /// The API path for the Mockery service mock endpoint.
    /// </summary>
    private const string MockApiPath = "api/mock";

    /// <summary>
    /// Gets the name of the downstream service this handler is configured for.
    /// </summary>
    public string ServiceName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MockeryHandler"/> class.
    /// </summary>
    /// <param name="serviceName">The name of the downstream service this handler is configured for.</param>
    /// <param name="httpClientFactory">The HTTP client factory for creating clients.</param>
    /// <param name="httpContextAccessor">The HTTP context accessor to read incoming request headers.</param>
    /// <param name="logger">The logger instance for this handler.</param>
    /// <param name="options">The configuration options for this handler.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public MockeryHandler(
        string serviceName,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        ILogger<MockeryHandler> logger,
        IOptions<MockeryHandlerOptions> options)
    {
        ServiceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        _options = options.Value;
    }

    /// <summary>
    /// Sends an HTTP request after propagating the X-Mock-ID header and routing to Mockery if present.
    /// </summary>
    /// <param name="request">The HTTP request message to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The HTTP response message.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        // Check if the handler is disabled - if so, bypass all mock logic
        if (!_options.Enabled)
        {
            _logger.LogDebug("MockeryHandler is disabled. Bypassing mock routing and proceeding with normal request pipeline.");
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        // Step 1: Check for X-Mock-ID header (direct mock ID takes priority)
        var mockId = PropagateAndGetMockIdHeader(request);

        // Step 2: If no X-Mock-ID, check X-Mockery-Mocks for service-specific mock
        if (string.IsNullOrWhiteSpace(mockId))
        {
            mockId = GetMockIdFromMocksList();
        }

        // Step 3: If a mock ID was found (from either header), route to Mockery service
        if (!string.IsNullOrWhiteSpace(mockId))
        {
            _logger.LogInformation(
                "Intercepted request for service '{ServiceName}' with MockId: {MockId}. Routing to Mockery service.",
                ServiceName,
                mockId);

            return await CallMockeryServiceAsync(request, mockId, cancellationToken)
                .ConfigureAwait(false);
        }

        // No mock routing applicable, proceed with normal request pipeline
        _logger.LogDebug(
            "No mock ID found for service '{ServiceName}'. Proceeding with normal request pipeline.",
            ServiceName);

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Propagates the X-Mock-ID header from the incoming HTTP request to the outgoing request
    /// and returns the mock ID value.
    /// </summary>
    /// <param name="request">The outgoing HTTP request to add the header to.</param>
    /// <returns>The mock ID value if present, null otherwise.</returns>
    private string? PropagateAndGetMockIdHeader(HttpRequestMessage request)
    {
        // First check if header is already on the outgoing request
        if (request.Headers.TryGetValues(MockIdHeaderName, out var existingValues))
        {
            var existingValue = existingValues.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(existingValue))
            {
                _logger.LogDebug("Header {HeaderName} already present on outgoing request", MockIdHeaderName);
                return existingValue;
            }
        }

        // Try to get the header from the incoming HTTP context
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            _logger.LogDebug("No HttpContext available for header propagation");
            return null;
        }

        if (httpContext.Request.Headers.TryGetValue(MockIdHeaderName, out var headerValue))
        {
            var value = headerValue.ToString();
            if (!string.IsNullOrWhiteSpace(value))
            {
                // Add the header to the outgoing request
                request.Headers.TryAddWithoutValidation(MockIdHeaderName, value);
                _logger.LogDebug(
                    "Propagated header {HeaderName}={HeaderValue} from incoming to outgoing request",
                    MockIdHeaderName,
                    value);
                return value;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the mock ID from the X-Mockery-Mocks header by matching the first segment
    /// of each mock identifier against the ServiceName.
    /// </summary>
    /// <returns>
    /// The matched mock ID if found, null otherwise. If multiple mocks match,
    /// one is randomly selected.
    /// </returns>
    private string? GetMockIdFromMocksList()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            _logger.LogDebug("No HttpContext available for {HeaderName} header lookup", MockeryMocksHeaderName);
            return null;
        }

        if (!httpContext.Request.Headers.TryGetValue(MockeryMocksHeaderName, out var headerValue))
        {
            return null;
        }

        var mocksList = headerValue.ToString();
        if (string.IsNullOrWhiteSpace(mocksList))
        {
            return null;
        }

        var matchedMock = FindMatchingMockFromList(mocksList);
        
        if (matchedMock != null)
        {
            _logger.LogDebug(
                "Found matching mock '{MatchedMock}' for service '{ServiceName}' from {HeaderName} header",
                matchedMock,
                ServiceName,
                MockeryMocksHeaderName);
        }
        
        return matchedMock;
    }

    /// <summary>
    /// Parses a comma-delimited list of mock identifiers and finds mocks that match
    /// the current ServiceName. The first segment of each mock (before the first '/')
    /// is compared against the ServiceName using case-insensitive matching.
    /// </summary>
    /// <param name="mocksList">A comma-delimited list of mock identifiers (e.g., "wind/prod/success,temperature/dev/error").</param>
    /// <returns>
    /// A matching mock identifier if found, null otherwise.
    /// If multiple mocks match the ServiceName, one is randomly selected.
    /// </returns>
    private string? FindMatchingMockFromList(string mocksList)
    {
        var mocks = mocksList.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        
        // Collect ALL matching mocks
        var matchingMocks = new List<string>();
        
        foreach (var mock in mocks)
        {
            // Get the first segment (before the first '/')
            var separatorIndex = mock.IndexOf('/');
            var firstSegment = separatorIndex >= 0 ? mock[..separatorIndex] : mock;
            
            // Case-insensitive comparison: check if ServiceName contains the first segment
            if (ServiceName.Contains(firstSegment, StringComparison.OrdinalIgnoreCase))
            {
                matchingMocks.Add(mock);
                _logger.LogDebug(
                    "Mock '{Mock}' matches ServiceName '{ServiceName}' (first segment: '{FirstSegment}')",
                    mock,
                    ServiceName,
                    firstSegment);
            }
        }
        
        if (matchingMocks.Count == 0)
        {
            _logger.LogDebug(
                "No matching mocks found for ServiceName '{ServiceName}' in mocks list",
                ServiceName);
            return null;
        }
        
        if (matchingMocks.Count == 1)
        {
            return matchingMocks[0];
        }
        
        // Randomly select one when multiple mocks match
        var randomIndex = Random.Shared.Next(matchingMocks.Count);
        var selectedMock = matchingMocks[randomIndex];
        
        _logger.LogDebug(
            "Multiple mocks matched ServiceName '{ServiceName}'. Randomly selected '{SelectedMock}' from {Count} matches",
            ServiceName,
            selectedMock,
            matchingMocks.Count);
        
        return selectedMock;
    }

    /// <summary>
    /// Calls the Mockery service and returns the mocked response.
    /// </summary>
    /// <param name="originalRequest">The original HTTP request.</param>
    /// <param name="mockId">The mock ID value from the header.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// The HTTP response from the Mockery service, including:
    /// - Status code from the mock response
    /// - Response content (body) from the mock response
    /// - Response headers from the mock response
    /// </returns>
    private async Task<HttpResponseMessage> CallMockeryServiceAsync(
        HttpRequestMessage originalRequest,
        string mockId,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Forwarding {Method} request to Mockery service. OriginalEndpoint: {OriginalEndpoint}, MockId: {MockId}",
                originalRequest.Method,
                originalRequest.RequestUri?.PathAndQuery ?? "/",
                mockId);

            // Create an HttpClient using the factory
            var mockeryClient = _httpClientFactory.CreateClient(MockeryClientName);

            // Create the request to the Mockery service
            using var mockeryRequest = new HttpRequestMessage(HttpMethod.Get, MockApiPath);

            // Add the X-Mock-ID header
            mockeryRequest.Headers.TryAddWithoutValidation(MockIdHeaderName, mockId);
            _logger.LogDebug("Added header {HeaderName}: {HeaderValue} to Mockery request", MockIdHeaderName, mockId);

            _logger.LogInformation(
                "Sending GET request to Mockery service at {BaseAddress}/{ApiPath}",
                mockeryClient.BaseAddress,
                MockApiPath);

            // Send the request to Mockery service
            var mockeryResponse = await mockeryClient.SendAsync(mockeryRequest, cancellationToken).ConfigureAwait(false);

            // Log successful response details
            _logger.LogInformation(
                "Received response from Mockery service. StatusCode: {StatusCode}, " +
                "ContentType: {ContentType}, ContentLength: {ContentLength}",
                (int)mockeryResponse.StatusCode,
                mockeryResponse.Content.Headers.ContentType?.MediaType ?? "unknown",
                mockeryResponse.Content.Headers.ContentLength ?? -1);

            // Check if the response is successful but has no content
            if (mockeryResponse.IsSuccessStatusCode)
            {
                var contentLength = mockeryResponse.Content.Headers.ContentLength;
                if (contentLength == 0 || contentLength == null)
                {
                    // Read the content to verify it's actually empty
                    var content = await mockeryResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        _logger.LogWarning(
                            "Mockery service returned {StatusCode} with no content for MockId: {MockId}. " +
                            "This may indicate the mock data is not configured or the mock ID is invalid.",
                            (int)mockeryResponse.StatusCode,
                            mockId);

                        // Return a 204 No Content to indicate the mock was found but has no data
                        return new HttpResponseMessage(System.Net.HttpStatusCode.NoContent)
                        {
                            ReasonPhrase = $"Mock '{mockId}' returned no content"
                        };
                    }
                }
            }

            // Return the mockery response directly - this includes:
            // - The status code (e.g., 200, 404, 500, etc.)
            // - The response content/body
            // - All response headers
            return mockeryResponse;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Failed to call Mockery service for MockId: {MockId}. Error: {ErrorMessage}",
                mockId,
                ex.Message);

            // Return a 502 Bad Gateway response indicating the mock service is unavailable
            return new HttpResponseMessage(System.Net.HttpStatusCode.BadGateway)
            {
                Content = new StringContent($"Failed to connect to Mockery service: {ex.Message}"),
                ReasonPhrase = "Mockery Service Unavailable"
            };
        }
    }

}
