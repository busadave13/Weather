using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Weather.Middleware;

namespace Weather.Tests.Middleware;

public class LoadSheddingMiddlewareTests
{
    private readonly Mock<ILogger<LoadSheddingMiddleware>> _mockLogger;

    public LoadSheddingMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<LoadSheddingMiddleware>>();
    }

    private LoadSheddingMiddleware CreateMiddleware(
        RequestDelegate next,
        LoadSheddingOptions options)
    {
        var optionsWrapper = Options.Create(options);
        return new LoadSheddingMiddleware(next, _mockLogger.Object, optionsWrapper);
    }

    private static DefaultHttpContext CreateHttpContext(string path = "/api/weather")
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        return context;
    }

    [Fact]
    public async Task InvokeAsync_WhenDisabled_ShouldPassThrough()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var options = new LoadSheddingOptions { Enabled = false };
        var middleware = CreateMiddleware(next, options);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhenNotWeatherEndpoint_ShouldPassThrough()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var options = new LoadSheddingOptions
        {
            Enabled = true,
            RpsThreshold = 1,
            FailurePercentage = 100
        };
        var middleware = CreateMiddleware(next, options);
        var context = CreateHttpContext("/api/other");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhenBelowThreshold_ShouldPassThrough()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var options = new LoadSheddingOptions
        {
            Enabled = true,
            RpsThreshold = 1000, // High threshold
            FailurePercentage = 100,
            WindowDurationSeconds = 1
        };
        var middleware = CreateMiddleware(next, options);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhenAboveThreshold_WithZeroFailurePercentage_ShouldPassThrough()
    {
        // Arrange
        var nextCallCount = 0;
        RequestDelegate next = _ =>
        {
            nextCallCount++;
            return Task.CompletedTask;
        };

        var options = new LoadSheddingOptions
        {
            Enabled = true,
            RpsThreshold = 1,
            FailurePercentage = 0, // Never fail
            WindowDurationSeconds = 1
        };
        var middleware = CreateMiddleware(next, options);

        // Act - Make enough requests to exceed threshold
        for (var i = 0; i < 10; i++)
        {
            var context = CreateHttpContext();
            await middleware.InvokeAsync(context);
        }

        // Assert - All requests should pass through
        nextCallCount.Should().Be(10);
    }

    [Fact]
    public async Task InvokeAsync_WhenAboveThreshold_With100PercentFailure_ShouldRejectAllOverThreshold()
    {
        // Arrange
        var nextCallCount = 0;
        RequestDelegate next = _ =>
        {
            nextCallCount++;
            return Task.CompletedTask;
        };

        var options = new LoadSheddingOptions
        {
            Enabled = true,
            RpsThreshold = 1,
            FailurePercentage = 100, // Always fail when over threshold
            FailureStatusCode = 503,
            WindowDurationSeconds = 1
        };
        var middleware = CreateMiddleware(next, options);

        // Act - First request should pass (at threshold), subsequent should fail
        var contexts = new List<DefaultHttpContext>();
        for (var i = 0; i < 5; i++)
        {
            var context = CreateHttpContext();
            contexts.Add(context);
            await middleware.InvokeAsync(context);
        }

        // Assert
        // First request passes (RPS=1, at threshold, not over)
        nextCallCount.Should().Be(1);
        
        // Remaining requests should be rejected with 503
        contexts.Skip(1).Should().AllSatisfy(ctx =>
            ctx.Response.StatusCode.Should().Be(503));
    }

    [Fact]
    public async Task InvokeAsync_WhenAboveThreshold_ShouldReturnConfiguredStatusCode()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;

        var options = new LoadSheddingOptions
        {
            Enabled = true,
            RpsThreshold = 1,
            FailurePercentage = 100,
            FailureStatusCode = 429, // Custom status code
            WindowDurationSeconds = 1
        };
        var middleware = CreateMiddleware(next, options);

        // Act - First request at threshold, second over threshold
        var context1 = CreateHttpContext();
        await middleware.InvokeAsync(context1);
        
        var context2 = CreateHttpContext();
        await middleware.InvokeAsync(context2);

        // Assert
        context2.Response.StatusCode.Should().Be(429);
    }

    [Fact]
    public async Task InvokeAsync_WhenAboveThreshold_ShouldReturnJsonErrorBody()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;

        var options = new LoadSheddingOptions
        {
            Enabled = true,
            RpsThreshold = 1,
            FailurePercentage = 100,
            FailureStatusCode = 503,
            WindowDurationSeconds = 1
        };
        var middleware = CreateMiddleware(next, options);

        // Act - First request at threshold, second over threshold
        await middleware.InvokeAsync(CreateHttpContext());
        
        var context = CreateHttpContext();
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.ContentType.Should().StartWith("application/json");
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();
        body.Should().Contain("Service temporarily unavailable due to high load");
    }

    [Fact]
    public async Task InvokeAsync_WithWeatherSubEndpoint_ShouldApplyLoadShedding()
    {
        // Arrange
        var nextCallCount = 0;
        RequestDelegate next = _ =>
        {
            nextCallCount++;
            return Task.CompletedTask;
        };

        var options = new LoadSheddingOptions
        {
            Enabled = true,
            RpsThreshold = 1,
            FailurePercentage = 100,
            WindowDurationSeconds = 1
        };
        var middleware = CreateMiddleware(next, options);

        // Act - Test sub-endpoints
        await middleware.InvokeAsync(CreateHttpContext("/api/weather/temperature"));
        await middleware.InvokeAsync(CreateHttpContext("/api/weather/wind"));

        // Assert - First passes, second rejected
        nextCallCount.Should().Be(1);
    }

    [Fact]
    public async Task InvokeAsync_WithLongerWindowDuration_ShouldCalculateRpsCorrectly()
    {
        // Arrange
        var nextCallCount = 0;
        RequestDelegate next = _ =>
        {
            nextCallCount++;
            return Task.CompletedTask;
        };

        var options = new LoadSheddingOptions
        {
            Enabled = true,
            RpsThreshold = 5, // 5 requests per second
            FailurePercentage = 100,
            WindowDurationSeconds = 2 // 2 second window means 10 requests before exceeding
        };
        var middleware = CreateMiddleware(next, options);

        // Act - Make 10 requests (threshold is 5 RPS over 2 seconds = 10 requests)
        for (var i = 0; i < 10; i++)
        {
            var context = CreateHttpContext();
            await middleware.InvokeAsync(context);
        }

        // Assert - All 10 should pass (at threshold, not over)
        nextCallCount.Should().Be(10);
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogWarningWhenSheddingLoad()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;

        var options = new LoadSheddingOptions
        {
            Enabled = true,
            RpsThreshold = 1,
            FailurePercentage = 100,
            FailureStatusCode = 503,
            WindowDurationSeconds = 1
        };
        var middleware = CreateMiddleware(next, options);

        // Act - First request at threshold, second over threshold
        await middleware.InvokeAsync(CreateHttpContext());
        await middleware.InvokeAsync(CreateHttpContext());

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Load shedding triggered")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("/api/weather")]
    [InlineData("/api/Weather")]
    [InlineData("/API/WEATHER")]
    [InlineData("/api/weather/temperature")]
    public async Task InvokeAsync_WithVariousWeatherPaths_ShouldApplyLoadShedding(string path)
    {
        // Arrange
        var nextCallCount = 0;
        RequestDelegate next = _ =>
        {
            nextCallCount++;
            return Task.CompletedTask;
        };

        var options = new LoadSheddingOptions
        {
            Enabled = true,
            RpsThreshold = 1,
            FailurePercentage = 100,
            WindowDurationSeconds = 1
        };
        var middleware = CreateMiddleware(next, options);

        // Act
        await middleware.InvokeAsync(CreateHttpContext(path));
        await middleware.InvokeAsync(CreateHttpContext(path));

        // Assert - First passes, second rejected (load shedding applied)
        nextCallCount.Should().Be(1);
    }

    [Fact]
    public void Constructor_WithNullNext_ShouldThrowArgumentNullException()
    {
        // Arrange
        var options = Options.Create(new LoadSheddingOptions());

        // Act
        var act = () => new LoadSheddingMiddleware(null!, _mockLogger.Object, options);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("next");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;
        var options = Options.Create(new LoadSheddingOptions());

        // Act
        var act = () => new LoadSheddingMiddleware(next, null!, options);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;

        // Act
        var act = () => new LoadSheddingMiddleware(next, _mockLogger.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }
}
