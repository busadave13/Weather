using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Weather.HealthChecks;

namespace Weather.Tests.HealthChecks;

public class StartupHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_ReturnsHealthy()
    {
        // Arrange
        var healthCheck = new StartupHealthCheck();
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("startup", healthCheck, null, null)
        };

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Be("Application has started");
    }
}
