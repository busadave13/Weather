using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Weather.Middleware;
using Weather.Services;
using Xunit;

namespace Weather.Tests.Services;

public class LoadSheddingServiceTests
{
    private readonly Mock<ILogger<LoadSheddingService>> _mockLogger;

    public LoadSheddingServiceTests()
    {
        _mockLogger = new Mock<ILogger<LoadSheddingService>>();
    }

    private LoadSheddingService CreateService(LoadSheddingOptions options)
    {
        var optionsWrapper = Options.Create(options);
        return new LoadSheddingService(optionsWrapper, _mockLogger.Object);
    }

    [Fact]
    public void ShouldRejectRequest_WhenDisabled_ReturnsFalse()
    {
        // Arrange
        var options = new LoadSheddingOptions { Enabled = false };
        var service = CreateService(options);

        // Act
        var result = service.ShouldRejectRequest();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldRejectRequest_WhenEnabledAndUnderThreshold_ReturnsFalse()
    {
        // Arrange
        var options = new LoadSheddingOptions
        {
            Enabled = true,
            RpsThreshold = 100,
            FailurePercentage = 50,
            WindowSizeSeconds = 1
        };
        var service = CreateService(options);

        // Act - single request should be under threshold
        var result = service.ShouldRejectRequest();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldRejectRequest_WhenEnabledAndOverThreshold_MayRejectBasedOnPercentage()
    {
        // Arrange
        var options = new LoadSheddingOptions
        {
            Enabled = true,
            RpsThreshold = 5,
            FailurePercentage = 100, // Always reject when over threshold
            WindowSizeSeconds = 1
        };
        var service = CreateService(options);

        // Act - make enough requests to exceed threshold
        for (int i = 0; i < 10; i++)
        {
            service.ShouldRejectRequest();
        }

        // The next request should be rejected
        var result = service.ShouldRejectRequest();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldRejectRequest_WithZeroFailurePercentage_NeverRejects()
    {
        // Arrange
        var options = new LoadSheddingOptions
        {
            Enabled = true,
            RpsThreshold = 1,
            FailurePercentage = 0, // Never reject
            WindowSizeSeconds = 1
        };
        var service = CreateService(options);

        // Act - make many requests
        bool anyRejected = false;
        for (int i = 0; i < 100; i++)
        {
            if (service.ShouldRejectRequest())
            {
                anyRejected = true;
            }
        }

        // Assert
        anyRejected.Should().BeFalse();
    }

    [Fact]
    public void ShouldRejectRequest_With100FailurePercentage_AlwaysRejectsOverThreshold()
    {
        // Arrange
        var options = new LoadSheddingOptions
        {
            Enabled = true,
            RpsThreshold = 5,
            FailurePercentage = 100, // Always reject
            WindowSizeSeconds = 1
        };
        var service = CreateService(options);

        // First, exceed the threshold
        for (int i = 0; i < 10; i++)
        {
            service.ShouldRejectRequest();
        }

        // Act - subsequent requests should all be rejected
        bool allRejected = true;
        for (int i = 0; i < 5; i++)
        {
            if (!service.ShouldRejectRequest())
            {
                allRejected = false;
            }
        }

        // Assert
        allRejected.Should().BeTrue();
    }

    [Fact]
    public void GetCurrentRps_ReturnsZero_WhenNoRequests()
    {
        // Arrange
        var options = new LoadSheddingOptions
        {
            Enabled = true,
            WindowSizeSeconds = 1
        };
        var service = CreateService(options);

        // Act
        var rps = service.GetCurrentRps();

        // Assert
        rps.Should().Be(0);
    }

    [Fact]
    public void GetCurrentRps_ReturnsCorrectCount_AfterRequests()
    {
        // Arrange
        var options = new LoadSheddingOptions
        {
            Enabled = true,
            RpsThreshold = 1000, // High threshold so nothing gets rejected
            WindowSizeSeconds = 1
        };
        var service = CreateService(options);

        // Act - make 10 requests
        for (int i = 0; i < 10; i++)
        {
            service.ShouldRejectRequest();
        }

        var rps = service.GetCurrentRps();

        // Assert
        rps.Should().Be(10);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOptionsIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new LoadSheddingService(null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        // Arrange
        var options = Options.Create(new LoadSheddingOptions());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new LoadSheddingService(options, null!));
    }
}
