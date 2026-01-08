using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Weather.Controllers;
using Xunit;

namespace Weather.Tests.Controllers;

public class AlertsControllerTests
{
    private readonly Mock<ILogger<AlertsController>> _mockLogger;

    public AlertsControllerTests()
    {
        _mockLogger = new Mock<ILogger<AlertsController>>();
    }

    private AlertsController CreateController()
    {
        return new AlertsController(_mockLogger.Object);
    }

    [Fact]
    public void GetAlerts_ReturnsNoContent()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.GetAlerts();

        // Assert
        result.Should().BeOfType<NoContentResult>();
        var noContentResult = result as NoContentResult;
        noContentResult!.StatusCode.Should().Be(204);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AlertsController(null!));
    }
}
