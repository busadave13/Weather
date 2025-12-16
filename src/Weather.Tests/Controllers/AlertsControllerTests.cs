using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Weather.Controllers;
using Weather.Middleware;
using Weather.Services;
using Xunit;

namespace Weather.Tests.Controllers;

public class AlertsControllerTests
{
    private readonly Mock<ILoadSheddingService> _mockLoadSheddingService;
    private readonly Mock<ILogger<AlertsController>> _mockLogger;
    private readonly LoadSheddingOptions _options;

    public AlertsControllerTests()
    {
        _mockLoadSheddingService = new Mock<ILoadSheddingService>();
        _mockLogger = new Mock<ILogger<AlertsController>>();
        _options = new LoadSheddingOptions
        {
            Enabled = true,
            RpsThreshold = 100,
            FailurePercentage = 25,
            FailureStatusCode = 503
        };
    }

    private AlertsController CreateController()
    {
        var optionsWrapper = Options.Create(_options);
        var controller = new AlertsController(
            _mockLoadSheddingService.Object,
            optionsWrapper,
            _mockLogger.Object);

        // Set up HttpContext for Response.Headers
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        return controller;
    }

    [Fact]
    public void GetAlerts_WhenNotLoadShedding_ReturnsNoContent()
    {
        // Arrange
        _mockLoadSheddingService.Setup(x => x.ShouldRejectRequest()).Returns(false);
        var controller = CreateController();

        // Act
        var result = controller.GetAlerts();

        // Assert
        result.Should().BeOfType<NoContentResult>();
        var noContentResult = result as NoContentResult;
        noContentResult!.StatusCode.Should().Be(204);
    }

    [Fact]
    public void GetAlerts_WhenLoadShedding_Returns503()
    {
        // Arrange
        _mockLoadSheddingService.Setup(x => x.ShouldRejectRequest()).Returns(true);
        var controller = CreateController();

        // Act
        var result = controller.GetAlerts();

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(503);
    }

    [Fact]
    public void GetAlerts_WhenLoadShedding_SetsLoadSheddingHeader()
    {
        // Arrange
        _mockLoadSheddingService.Setup(x => x.ShouldRejectRequest()).Returns(true);
        var controller = CreateController();

        // Act
        controller.GetAlerts();

        // Assert
        controller.Response.Headers.Should().ContainKey("X-Load-Shedding");
        controller.Response.Headers["X-Load-Shedding"].ToString().Should().Be("true");
    }

    [Fact]
    public void GetAlerts_WhenLoadShedding_SetsRetryAfterHeader()
    {
        // Arrange
        _mockLoadSheddingService.Setup(x => x.ShouldRejectRequest()).Returns(true);
        var controller = CreateController();

        // Act
        controller.GetAlerts();

        // Assert
        controller.Response.Headers.Should().ContainKey("Retry-After");
        controller.Response.Headers["Retry-After"].ToString().Should().Be("1");
    }

    [Fact]
    public void GetAlerts_WhenLoadShedding_ReturnsErrorPayload()
    {
        // Arrange
        _mockLoadSheddingService.Setup(x => x.ShouldRejectRequest()).Returns(true);
        var controller = CreateController();

        // Act
        var result = controller.GetAlerts();

        // Assert
        var objectResult = result as ObjectResult;
        objectResult!.Value.Should().NotBeNull();
        var value = objectResult.Value;
        value.Should().BeEquivalentTo(new
        {
            error = "Service is under heavy load",
            message = "Please retry your request in a moment",
            retryAfter = 1
        });
    }

    [Fact]
    public void GetAlerts_UsesConfiguredFailureStatusCode()
    {
        // Arrange
        _options.FailureStatusCode = 429; // Use rate limit code instead
        _mockLoadSheddingService.Setup(x => x.ShouldRejectRequest()).Returns(true);
        var controller = CreateController();

        // Act
        var result = controller.GetAlerts();

        // Assert
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(429);
    }

    [Fact]
    public void GetAlerts_CallsLoadSheddingService()
    {
        // Arrange
        _mockLoadSheddingService.Setup(x => x.ShouldRejectRequest()).Returns(false);
        var controller = CreateController();

        // Act
        controller.GetAlerts();

        // Assert
        _mockLoadSheddingService.Verify(x => x.ShouldRejectRequest(), Times.Once);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoadSheddingServiceIsNull()
    {
        // Arrange
        var optionsWrapper = Options.Create(_options);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new AlertsController(null!, optionsWrapper, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOptionsIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new AlertsController(_mockLoadSheddingService.Object, null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        // Arrange
        var optionsWrapper = Options.Create(_options);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new AlertsController(_mockLoadSheddingService.Object, optionsWrapper, null!));
    }
}
