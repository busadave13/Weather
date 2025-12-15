using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Weather.HealthChecks;

namespace Weather.Tests.HealthChecks;

public class ReadyHealthCheckTests
{
    private readonly Mock<IRequestCounter> _mockCounter;
    private readonly Mock<IOptions<HealthCheckOptions>> _mockOptions;
    private readonly Mock<ILogger<ReadyHealthCheck>> _mockLogger;
    private readonly HealthCheckOptions _options;

    public ReadyHealthCheckTests()
    {
        _mockCounter = new Mock<IRequestCounter>();
        _mockOptions = new Mock<IOptions<HealthCheckOptions>>();
        _mockLogger = new Mock<ILogger<ReadyHealthCheck>>();
        _options = new HealthCheckOptions();
        _mockOptions.Setup(o => o.Value).Returns(_options);
    }

    private HealthCheckContext CreateContext(ReadyHealthCheck healthCheck)
    {
        return new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("ready", healthCheck, null, null)
        };
    }

    [Fact]
    public void Constructor_WithNullCounter_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ReadyHealthCheck(null!, _mockOptions.Object, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("counter");
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ReadyHealthCheck(_mockCounter.Object, null!, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ReadyHealthCheck(_mockCounter.Object, _mockOptions.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public async Task CheckHealthAsync_WithThresholdDisabled_ReturnsHealthy()
    {
        // Arrange
        _options.RequestCountThreshold = 0;
        _mockCounter.Setup(c => c.IncrementAndGet()).Returns(1000);

        var healthCheck = new ReadyHealthCheck(_mockCounter.Object, _mockOptions.Object, _mockLogger.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Contain("threshold disabled");
        _mockCounter.Verify(c => c.IncrementAndGet(), Times.Once);
    }

    [Fact]
    public async Task CheckHealthAsync_WithNegativeThreshold_ReturnsHealthy()
    {
        // Arrange
        _options.RequestCountThreshold = -1;
        _mockCounter.Setup(c => c.IncrementAndGet()).Returns(1000);

        var healthCheck = new ReadyHealthCheck(_mockCounter.Object, _mockOptions.Object, _mockLogger.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Contain("threshold disabled");
        _mockCounter.Verify(c => c.IncrementAndGet(), Times.Once);
    }

    [Fact]
    public async Task CheckHealthAsync_WithCountBelowThreshold_ReturnsHealthy()
    {
        // Arrange
        _options.RequestCountThreshold = 100;
        _mockCounter.Setup(c => c.IncrementAndGet()).Returns(50);

        var healthCheck = new ReadyHealthCheck(_mockCounter.Object, _mockOptions.Object, _mockLogger.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Be("Request count: 50/100");
        _mockCounter.Verify(c => c.IncrementAndGet(), Times.Once);
    }

    [Fact]
    public async Task CheckHealthAsync_WithCountAtThreshold_ReturnsUnhealthy()
    {
        // Arrange
        _options.RequestCountThreshold = 100;
        _mockCounter.Setup(c => c.IncrementAndGet()).Returns(100);

        var healthCheck = new ReadyHealthCheck(_mockCounter.Object, _mockOptions.Object, _mockLogger.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Be("Request count 100 exceeded threshold 100");
        _mockCounter.Verify(c => c.IncrementAndGet(), Times.Once);
    }

    [Fact]
    public async Task CheckHealthAsync_WithCountAboveThreshold_ReturnsUnhealthy()
    {
        // Arrange
        _options.RequestCountThreshold = 100;
        _mockCounter.Setup(c => c.IncrementAndGet()).Returns(150);

        var healthCheck = new ReadyHealthCheck(_mockCounter.Object, _mockOptions.Object, _mockLogger.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Be("Request count 150 exceeded threshold 100");
        _mockCounter.Verify(c => c.IncrementAndGet(), Times.Once);
    }

    [Fact]
    public async Task CheckHealthAsync_WithFirstCall_ReturnsHealthy()
    {
        // Arrange
        _options.RequestCountThreshold = 100;
        _mockCounter.Setup(c => c.IncrementAndGet()).Returns(1);

        var healthCheck = new ReadyHealthCheck(_mockCounter.Object, _mockOptions.Object, _mockLogger.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Be("Request count: 1/100");
        _mockCounter.Verify(c => c.IncrementAndGet(), Times.Once);
    }
}
