using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using Weather.Clients.Handlers;

namespace Weather.Tests.Clients.Handlers;

/// <summary>
/// Unit tests for <see cref="MockeryHandler"/>.
/// </summary>
public class MockeryHandlerTests
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<ILogger<MockeryHandler>> _mockLogger;
    private readonly Mock<IOptions<MockeryHandlerOptions>> _mockOptions;
    private readonly MockeryHandlerOptions _options;
    private readonly DefaultHttpContext _httpContext;

    private const string TestServiceName = "WindSensor";

    public MockeryHandlerTests()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockLogger = new Mock<ILogger<MockeryHandler>>();
        _options = new MockeryHandlerOptions { Enabled = true, BaseUrl = "http://mockery:5000" };
        _mockOptions = new Mock<IOptions<MockeryHandlerOptions>>();
        _mockOptions.Setup(x => x.Value).Returns(_options);

        _httpContext = new DefaultHttpContext();
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_httpContext);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_SetsServiceName()
    {
        // Arrange & Act
        var handler = CreateHandler(TestServiceName);

        // Assert
        handler.ServiceName.Should().Be(TestServiceName);
    }

    [Fact]
    public void Constructor_WithNullServiceName_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new MockeryHandler(
            null!,
            _mockHttpClientFactory.Object,
            _mockHttpContextAccessor.Object,
            _mockLogger.Object,
            _mockOptions.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("serviceName");
    }

    #endregion

    #region X-Mockery-Mocks Header Parsing Tests

    [Fact]
    public async Task SendAsync_WithMockeryMocksHeader_MatchesByFirstSegment()
    {
        // Arrange
        var handler = CreateHandlerWithMockeryClient("WindSensor");
        _httpContext.Request.Headers[MockeryHandler.MockeryMocksHeaderName] = "wind/prod/success";

        var request = new HttpRequestMessage(HttpMethod.Get, "http://api.example.com/test");

        // Act
        var response = await handler.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SendAsync_WithMockeryMocksHeader_CaseInsensitiveMatch()
    {
        // Arrange
        var handler = CreateHandlerWithMockeryClient("WindSensor");
        _httpContext.Request.Headers[MockeryHandler.MockeryMocksHeaderName] = "WIND/prod/success";

        var request = new HttpRequestMessage(HttpMethod.Get, "http://api.example.com/test");

        // Act
        var response = await handler.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SendAsync_WithMockeryMocksHeader_NoMatchProceedsNormally()
    {
        // Arrange
        var handler = CreateHandlerWithInnerHandler("WindSensor");
        _httpContext.Request.Headers[MockeryHandler.MockeryMocksHeaderName] = "temperature/prod/success,precipitation/dev/error";

        var request = new HttpRequestMessage(HttpMethod.Get, "http://api.example.com/test");

        // Act
        var response = await handler.SendAsync(request);

        // Assert
        // Should proceed to inner handler (returns 200 from mock inner handler)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SendAsync_WithMultipleMocksMatchingSameName_SelectsRandomly()
    {
        // Arrange
        var selectedMocks = new HashSet<string>();
        
        // Run multiple times to verify random selection
        for (int i = 0; i < 50; i++)
        {
            var handler = CreateHandlerWithMockeryClientCapturingMockId("WindSensor", out var capturedMockId);
            _httpContext.Request.Headers[MockeryHandler.MockeryMocksHeaderName] = "wind/prod/success,wind/dev/error,wind/staging/timeout";

            var request = new HttpRequestMessage(HttpMethod.Get, "http://api.example.com/test");

            // Act
            await handler.SendAsync(request);

            // Capture which mock was selected
            if (capturedMockId.Value != null)
            {
                selectedMocks.Add(capturedMockId.Value);
            }
        }

        // Assert - with 50 iterations, we should see more than one mock selected
        selectedMocks.Should().HaveCountGreaterThan(1, 
            "random selection should choose different mocks over multiple iterations");
    }

    [Fact]
    public async Task SendAsync_WithMockeryMocksHeader_ParsesCommaSeparatedList()
    {
        // Arrange
        var handler = CreateHandlerWithMockeryClient("TemperatureSensor");
        _httpContext.Request.Headers[MockeryHandler.MockeryMocksHeaderName] = 
            "wind/prod/success, temperature/dev/error, precipitation/staging/timeout";

        var request = new HttpRequestMessage(HttpMethod.Get, "http://api.example.com/test");

        // Act
        var response = await handler.SendAsync(request);

        // Assert - should match "temperature/dev/error" for TemperatureSensor
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SendAsync_WithMockeryMocksHeader_HandlesWhitespace()
    {
        // Arrange
        var handler = CreateHandlerWithMockeryClient("WindSensor");
        _httpContext.Request.Headers[MockeryHandler.MockeryMocksHeaderName] = 
            "  wind/prod/success  ,  temperature/dev/error  ";

        var request = new HttpRequestMessage(HttpMethod.Get, "http://api.example.com/test");

        // Act
        var response = await handler.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SendAsync_WithMockeryMocksHeader_HandlesNoSlash()
    {
        // Arrange
        var handler = CreateHandlerWithMockeryClient("WindSensor");
        _httpContext.Request.Headers[MockeryHandler.MockeryMocksHeaderName] = "wind";

        var request = new HttpRequestMessage(HttpMethod.Get, "http://api.example.com/test");

        // Act
        var response = await handler.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SendAsync_WithEmptyMockeryMocksHeader_ProceedsNormally()
    {
        // Arrange
        var handler = CreateHandlerWithInnerHandler("WindSensor");
        _httpContext.Request.Headers[MockeryHandler.MockeryMocksHeaderName] = "";

        var request = new HttpRequestMessage(HttpMethod.Get, "http://api.example.com/test");

        // Act
        var response = await handler.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Header Priority Tests

    [Fact]
    public async Task SendAsync_WithBothHeaders_XMockIdTakesPriority()
    {
        // Arrange
        string? capturedMockId = null;
        var handler = CreateHandlerWithMockeryClientCapturingMockId("WindSensor", out var mockIdRef);
        
        _httpContext.Request.Headers[MockeryHandler.MockIdHeaderName] = "direct-mock-id";
        _httpContext.Request.Headers[MockeryHandler.MockeryMocksHeaderName] = "wind/prod/success";

        var request = new HttpRequestMessage(HttpMethod.Get, "http://api.example.com/test");

        // Act
        await handler.SendAsync(request);
        capturedMockId = mockIdRef.Value;

        // Assert - X-Mock-ID should take priority
        capturedMockId.Should().Be("direct-mock-id");
    }

    [Fact]
    public async Task SendAsync_WithOnlyMockeryMocks_UsesMockeryMocks()
    {
        // Arrange
        var handler = CreateHandlerWithMockeryClientCapturingMockId("WindSensor", out var mockIdRef);
        _httpContext.Request.Headers[MockeryHandler.MockeryMocksHeaderName] = "wind/prod/success";

        var request = new HttpRequestMessage(HttpMethod.Get, "http://api.example.com/test");

        // Act
        await handler.SendAsync(request);

        // Assert
        mockIdRef.Value.Should().Be("wind/prod/success");
    }

    #endregion

    #region Disabled Handler Tests

    [Fact]
    public async Task SendAsync_WhenDisabled_BypassesMockLogic()
    {
        // Arrange
        _options.Enabled = false;
        var handler = CreateHandlerWithInnerHandler("WindSensor");
        _httpContext.Request.Headers[MockeryHandler.MockeryMocksHeaderName] = "wind/prod/success";

        var request = new HttpRequestMessage(HttpMethod.Get, "http://api.example.com/test");

        // Act
        var response = await handler.SendAsync(request);

        // Assert - should proceed to inner handler without mock routing
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region No HttpContext Tests

    [Fact]
    public async Task SendAsync_WithNoHttpContext_ProceedsNormally()
    {
        // Arrange
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);
        var handler = CreateHandlerWithInnerHandler("WindSensor");

        var request = new HttpRequestMessage(HttpMethod.Get, "http://api.example.com/test");

        // Act
        var response = await handler.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Helper Methods

    private MockeryHandler CreateHandler(string serviceName)
    {
        return new MockeryHandler(
            serviceName,
            _mockHttpClientFactory.Object,
            _mockHttpContextAccessor.Object,
            _mockLogger.Object,
            _mockOptions.Object);
    }

    private TestableHandler CreateHandlerWithInnerHandler(string serviceName)
    {
        var innerHandler = new Mock<HttpMessageHandler>();
        innerHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var handler = new TestableHandler(
            serviceName,
            _mockHttpClientFactory.Object,
            _mockHttpContextAccessor.Object,
            _mockLogger.Object,
            _mockOptions.Object)
        {
            InnerHandler = innerHandler.Object
        };

        return handler;
    }

    private TestableHandler CreateHandlerWithMockeryClient(string serviceName)
    {
        // Setup MockeryClient to return success
        var mockeryHandler = new Mock<HttpMessageHandler>();
        mockeryHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"data\": \"mocked\"}")
            });

        var mockeryClient = new HttpClient(mockeryHandler.Object)
        {
            BaseAddress = new Uri("http://mockery:5000")
        };

        _mockHttpClientFactory
            .Setup(x => x.CreateClient("MockeryClient"))
            .Returns(mockeryClient);

        // Setup inner handler (should not be called when mock is matched)
        var innerHandler = new Mock<HttpMessageHandler>();
        innerHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var handler = new TestableHandler(
            serviceName,
            _mockHttpClientFactory.Object,
            _mockHttpContextAccessor.Object,
            _mockLogger.Object,
            _mockOptions.Object)
        {
            InnerHandler = innerHandler.Object
        };

        return handler;
    }

    private TestableHandler CreateHandlerWithMockeryClientCapturingMockId(string serviceName, out CapturedValue<string?> capturedMockId)
    {
        var captured = new CapturedValue<string?>();
        capturedMockId = captured;

        var mockeryHandler = new Mock<HttpMessageHandler>();
        mockeryHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) =>
            {
                if (req.Headers.TryGetValues(MockeryHandler.MockIdHeaderName, out var values))
                {
                    captured.Value = values.FirstOrDefault();
                }
            })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"data\": \"mocked\"}")
            });

        var mockeryClient = new HttpClient(mockeryHandler.Object)
        {
            BaseAddress = new Uri("http://mockery:5000")
        };

        _mockHttpClientFactory
            .Setup(x => x.CreateClient("MockeryClient"))
            .Returns(mockeryClient);

        var innerHandler = new Mock<HttpMessageHandler>();
        innerHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var handler = new TestableHandler(
            serviceName,
            _mockHttpClientFactory.Object,
            _mockHttpContextAccessor.Object,
            _mockLogger.Object,
            _mockOptions.Object)
        {
            InnerHandler = innerHandler.Object
        };

        return handler;
    }

    #endregion

    #region Helper Classes

    /// <summary>
    /// A wrapper to capture values from callbacks.
    /// </summary>
    private class CapturedValue<T>
    {
        public T? Value { get; set; }
    }

    /// <summary>
    /// A testable version of MockeryHandler that exposes SendAsync publicly.
    /// </summary>
    private class TestableHandler : MockeryHandler
    {
        public TestableHandler(
            string serviceName,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<MockeryHandler> logger,
            IOptions<MockeryHandlerOptions> options)
            : base(serviceName, httpClientFactory, httpContextAccessor, logger, options)
        {
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return base.SendAsync(request, CancellationToken.None);
        }
    }

    #endregion
}
