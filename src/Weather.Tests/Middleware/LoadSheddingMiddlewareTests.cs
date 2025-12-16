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

    private static IOptions<LoadSheddingOptions> CreateOptions(LoadSheddingOptions options)
    {
        return Options.Create(options);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    [Fact]
    public void Constructor_WithNullNext_ThrowsArgumentNullException()
    {
        // Arrange
        var options = CreateOptions(new LoadSheddingOptions());

        // Act
        var act = () => new LoadSheddingMiddleware(null!, options, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("next");
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;

        // Act
        var act = () => new LoadSheddingMiddleware(next, null!, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;
        var options = CreateOptions(new LoadSheddingOptions());

        // Act
        var act = () => new LoadSheddingMiddleware(next, options, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public async Task InvokeAsync_WhenDisabled_PassesThroughToNext()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var options = CreateOptions(new LoadSheddingOptions { Enabled = false });
        var middleware = new LoadSheddingMiddleware(next, options, _mockLogger.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_WhenEnabled_BelowThreshold_PassesThroughToNext()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var options = CreateOptions(new LoadSheddingOptions
        {
            Enabled = true,
            RpsThreshold = 1000,
            FailurePercentage = 25
        });

        var middleware = new LoadSheddingMiddleware(next, options, _mockLogger.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_WhenEnabled_AboveThreshold_100PercentFailure_RejectsRequest()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;

        var options = CreateOptions(new LoadSheddingOptions
        {
            Enabled = true,
            RpsThreshold = 1, // Very low threshold
            FailurePercentage = 100, // Always fail when over threshold
            FailureStatusCode = 503,
            WindowSizeSeconds = 1
        });

        var middleware = new LoadSheddingMiddleware(next, options, _mockLogger.Object);

        // Generate enough requests to exceed threshold
        for (int i = 0; i < 5; i++)
        {
            var warmupContext = CreateHttpContext();
            await middleware.InvokeAsync(warmupContext);
        }

        // Act - This request should be rejected
        var context = CreateHttpContext();
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(503);
        context.Response.Headers.Should().ContainKey("X-Load-Shedding");
        context.Response.Headers["X-Load-Shedding"].ToString().Should().Be("true");
    }

    [Fact]
    public async Task InvokeAsync_WhenEnabled_AboveThreshold_0PercentFailure_AlwaysPasses()
    {
        // Arrange
        var nextCalledCount = 0;
        RequestDelegate next = _ =>
        {
            nextCalledCount++;
            return Task.CompletedTask;
        };

        var options = CreateOptions(new LoadSheddingOptions
        {
            Enabled = true,
            RpsThreshold = 1,
            FailurePercentage = 0, // Never fail
            WindowSizeSeconds = 1
        });

        var middleware = new LoadSheddingMiddleware(next, options, _mockLogger.Object);

        // Act - Make multiple requests
        for (int i = 0; i < 10; i++)
        {
            var context = CreateHttpContext();
            await middleware.InvokeAsync(context);
        }

        // Assert - All requests should pass through
        nextCalledCount.Should().Be(10);
    }

    [Fact]
    public async Task InvokeAsync_RejectedRequest_IncludesRetryAfterHeader()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;

        var options = CreateOptions(new LoadSheddingOptions
        {
            Enabled = true,
            RpsThreshold = 1,
            FailurePercentage = 100,
            FailureStatusCode = 503,
            WindowSizeSeconds = 1
        });

        var middleware = new LoadSheddingMiddleware(next, options, _mockLogger.Object);

        // Generate enough requests to exceed threshold
        for (int i = 0; i < 5; i++)
        {
            var warmupContext = CreateHttpContext();
            await middleware.InvokeAsync(warmupContext);
        }

        // Act
        var context = CreateHttpContext();
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey("Retry-After");
        context.Response.Headers["Retry-After"].ToString().Should().Be("1");
    }

    [Fact]
    public async Task InvokeAsync_CustomStatusCode_ReturnsConfiguredStatusCode()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;

        var options = CreateOptions(new LoadSheddingOptions
        {
            Enabled = true,
            RpsThreshold = 1,
            FailurePercentage = 100,
            FailureStatusCode = 429, // Too Many Requests
            WindowSizeSeconds = 1
        });

        var middleware = new LoadSheddingMiddleware(next, options, _mockLogger.Object);

        // Generate enough requests to exceed threshold
        for (int i = 0; i < 5; i++)
        {
            var warmupContext = CreateHttpContext();
            await middleware.InvokeAsync(warmupContext);
        }

        // Act
        var context = CreateHttpContext();
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(429);
    }

    [Fact]
    public async Task InvokeAsync_SlidingWindow_OldRequestsAreRemoved()
    {
        // Arrange
        var nextCalledCount = 0;
        RequestDelegate next = _ =>
        {
            nextCalledCount++;
            return Task.CompletedTask;
        };

        var options = CreateOptions(new LoadSheddingOptions
        {
            Enabled = true,
            RpsThreshold = 5,
            FailurePercentage = 100,
            WindowSizeSeconds = 1
        });

        var middleware = new LoadSheddingMiddleware(next, options, _mockLogger.Object);

        // Make initial requests
        for (int i = 0; i < 3; i++)
        {
            var context = CreateHttpContext();
            await middleware.InvokeAsync(context);
        }

        // Wait for window to slide past old requests
        await Task.Delay(1100);

        // Act - These should pass since old requests expired
        var newContext = CreateHttpContext();
        await middleware.InvokeAsync(newContext);

        // Assert - Request should pass through
        newContext.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_PartialFailure_SomeRequestsPass()
    {
        // Arrange
        var passedCount = 0;
        var rejectedCount = 0;

        RequestDelegate next = _ =>
        {
            passedCount++;
            return Task.CompletedTask;
        };

        var options = CreateOptions(new LoadSheddingOptions
        {
            Enabled = true,
            RpsThreshold = 1,
            FailurePercentage = 50,
            FailureStatusCode = 503,
            WindowSizeSeconds = 10 // Longer window to ensure we stay above threshold
        });

        var middleware = new LoadSheddingMiddleware(next, options, _mockLogger.Object);

        // Act - Make many requests
        var totalRequests = 100;
        for (int i = 0; i < totalRequests; i++)
        {
            var context = CreateHttpContext();
            await middleware.InvokeAsync(context);

            if (context.Response.StatusCode == 503)
            {
                rejectedCount++;
            }
        }

        // Assert - With 50% failure, we should have some of each
        // Due to the low threshold and ramp-up, most requests should be subject to rejection
        passedCount.Should().BeGreaterThan(0, "Some requests should pass");
        rejectedCount.Should().BeGreaterThan(0, "Some requests should be rejected");
    }

    [Fact]
    public async Task InvokeAsync_RejectedRequest_WritesJsonResponse()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;

        var options = CreateOptions(new LoadSheddingOptions
        {
            Enabled = true,
            RpsThreshold = 1,
            FailurePercentage = 100,
            FailureStatusCode = 503,
            WindowSizeSeconds = 1
        });

        var middleware = new LoadSheddingMiddleware(next, options, _mockLogger.Object);

        // Generate enough requests to exceed threshold
        for (int i = 0; i < 5; i++)
        {
            var warmupContext = CreateHttpContext();
            await middleware.InvokeAsync(warmupContext);
        }

        // Act
        var context = CreateHttpContext();
        await middleware.InvokeAsync(context);

        // Assert - Check response body contains JSON
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();

        responseBody.Should().Contain("error");
        responseBody.Should().Contain("Service is under heavy load");
    }
}
