using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;
using Weather.Configuration;
using Weather.HealthChecks;

namespace Weather.Tests.HealthChecks;

public class StartupHealthCheckTests
{
    private readonly Mock<ITestConfigurationState> _mockTestState;
    private readonly Mock<ILogger<StartupHealthCheck>> _mockLogger;

    public StartupHealthCheckTests()
    {
        _mockTestState = new Mock<ITestConfigurationState>();
        _mockLogger = new Mock<ILogger<StartupHealthCheck>>();
    }

    private HealthCheckContext CreateContext(StartupHealthCheck healthCheck)
    {
        return new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("startup", healthCheck, null, null)
        };
    }

    [Fact]
    public void Constructor_WithNullTestState_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new StartupHealthCheck(null!, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("testState");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new StartupHealthCheck(_mockTestState.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public async Task CheckHealthAsync_WhenNotForced_ReturnsHealthy()
    {
        // Arrange
        _mockTestState.Setup(s => s.ForceStartupFail).Returns(false);
        _mockTestState.Setup(s => s.StartupDelayMs).Returns(0);

        var healthCheck = new StartupHealthCheck(_mockTestState.Object, _mockLogger.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Be("Application has started");
    }

    [Fact]
    public async Task CheckHealthAsync_WhenForceStartupFail_ReturnsUnhealthy()
    {
        // Arrange
        _mockTestState.Setup(s => s.ForceStartupFail).Returns(true);
        _mockTestState.Setup(s => s.StartupDelayMs).Returns(0);

        var healthCheck = new StartupHealthCheck(_mockTestState.Object, _mockLogger.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Be("Forced failure via /api/config");
    }

    [Fact]
    public async Task CheckHealthAsync_WithDelay_AppliesDelay()
    {
        // Arrange
        var delayMs = 100;
        _mockTestState.Setup(s => s.ForceStartupFail).Returns(false);
        _mockTestState.Setup(s => s.StartupDelayMs).Returns(delayMs);

        var healthCheck = new StartupHealthCheck(_mockTestState.Object, _mockLogger.Object);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));
        stopwatch.Stop();

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        stopwatch.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(delayMs - 20); // Allow some tolerance
    }

    [Fact]
    public async Task CheckHealthAsync_WithZeroDelay_DoesNotDelay()
    {
        // Arrange
        _mockTestState.Setup(s => s.ForceStartupFail).Returns(false);
        _mockTestState.Setup(s => s.StartupDelayMs).Returns(0);

        var healthCheck = new StartupHealthCheck(_mockTestState.Object, _mockLogger.Object);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await healthCheck.CheckHealthAsync(CreateContext(healthCheck));
        stopwatch.Stop();

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(50); // Should be nearly instant
    }

    [Fact]
    public async Task CheckHealthAsync_WithCancellation_ThrowsTaskCanceledException()
    {
        // Arrange
        _mockTestState.Setup(s => s.ForceStartupFail).Returns(false);
        _mockTestState.Setup(s => s.StartupDelayMs).Returns(5000);

        var healthCheck = new StartupHealthCheck(_mockTestState.Object, _mockLogger.Object);
        var context = CreateContext(healthCheck);
        var cts = new CancellationTokenSource();
        cts.CancelAfter(50);

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => healthCheck.CheckHealthAsync(context, cts.Token));
    }
}
