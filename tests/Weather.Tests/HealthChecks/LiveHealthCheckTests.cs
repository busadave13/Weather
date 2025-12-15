using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Weather.HealthChecks;

namespace Weather.Tests.HealthChecks;

public class LiveHealthCheckTests
{
    private readonly Mock<IRequestCounter> _mockCounter;
    private readonly Mock<IOptions<HealthCheckOptions>> _mockOptions;
    private readonly Mock<ILogger<LiveHealthCheck>> _mockLogger;
    private readonly HealthCheckOptions _options;

    public LiveHealthCheckTests()
    {
        _mockCounter = new Mock<IRequestCounter>();
        _mockOptions = new Mock<IOptions<HealthCheckOptions>>();
        _mockLogger = new Mock<ILogger<LiveHealthCheck>>();
        _options = new HealthCheckOptions();
        _mockOptions.Setup(o => o.Value).Returns(_options);
    }

    private HealthCheckContext CreateContext(LiveHealthCheck healthCheck)
    {
        return new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("live", healthCheck, null, null)
        };
    }

    [Fact]
    public void Constructor_WithNullCounter_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new LiveHealthCheck(null!, _mockOptions.Object, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("counter");
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new LiveHealthCheck(_mockCounter.Object, null!, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new LiveHealthCheck(_mockCounter.Object, _mockOptions.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public async Task CheckHealthAsync_WithThresholdDisabled_ReturnsHealthy()
    {
        // Arrange
        _options.RequestCountThreshold = 0;
        _mockCounter.Setup(c => c.CurrentCount).Returns(1000);

        var healthCheck = new LiveHealthCheck(_mockCounter.Object, _mockOptions.Object, _mockLogger.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Contain("threshold disabled");
    }

    [Fact]
    public async Task CheckHealthAsync_WithNegativeThreshold_ReturnsHealthy()
    {
        // Arrange
        _options.RequestCountThreshold = -1;
        _mockCounter.Setup(c => c.CurrentCount).Returns(1000);

        var healthCheck = new LiveHealthCheck(_mockCounter.Object, _mockOptions.Object, _mockLogger.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Contain("threshold disabled");
    }

    [Fact]
    public async Task CheckHealthAsync_WithCountBelowLiveThreshold_ReturnsHealthy()
    {
        // Arrange
        _options.RequestCountThreshold = 100;
        _options.LiveGracePeriodRequests = 50;
        _mockCounter.Setup(c => c.CurrentCount).Returns(120); // Below 100+50=150

        var healthCheck = new LiveHealthCheck(_mockCounter.Object, _mockOptions.Object, _mockLogger.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Be("Request count: 120/150");
    }

    [Fact]
    public async Task CheckHealthAsync_WithCountAtLiveThreshold_ReturnsUnhealthy()
    {
        // Arrange
        _options.RequestCountThreshold = 100;
        _options.LiveGracePeriodRequests = 50;
        _mockCounter.Setup(c => c.CurrentCount).Returns(150); // At 100+50=150

        var healthCheck = new LiveHealthCheck(_mockCounter.Object, _mockOptions.Object, _mockLogger.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Be("Request count 150 exceeded live threshold 150");
    }

    [Fact]
    public async Task CheckHealthAsync_WithCountAboveLiveThreshold_ReturnsUnhealthy()
    {
        // Arrange
        _options.RequestCountThreshold = 100;
        _options.LiveGracePeriodRequests = 50;
        _mockCounter.Setup(c => c.CurrentCount).Returns(200); // Above 100+50=150

        var healthCheck = new LiveHealthCheck(_mockCounter.Object, _mockOptions.Object, _mockLogger.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Be("Request count 200 exceeded live threshold 150");
    }

    [Fact]
    public async Task CheckHealthAsync_WithCountBetweenReadyAndLiveThreshold_ReturnsHealthy()
    {
        // Arrange
        // Ready fails at 100, Live fails at 150
        _options.RequestCountThreshold = 100;
        _options.LiveGracePeriodRequests = 50;
        _mockCounter.Setup(c => c.CurrentCount).Returns(100); // At ready threshold but below live

        var healthCheck = new LiveHealthCheck(_mockCounter.Object, _mockOptions.Object, _mockLogger.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Be("Request count: 100/150");
    }

    [Fact]
    public async Task CheckHealthAsync_WithZeroGracePeriod_FailsAtReadyThreshold()
    {
        // Arrange
        _options.RequestCountThreshold = 100;
        _options.LiveGracePeriodRequests = 0;
        _mockCounter.Setup(c => c.CurrentCount).Returns(100); // At threshold

        var healthCheck = new LiveHealthCheck(_mockCounter.Object, _mockOptions.Object, _mockLogger.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Be("Request count 100 exceeded live threshold 100");
    }
}
