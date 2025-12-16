using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Weather.Configuration;
using Weather.Controllers;
using Weather.Models.Requests;
using Weather.Models.Responses;

namespace Weather.Tests.Controllers;

public class ConfigControllerTests
{
    private readonly Mock<ITestConfigurationState> _mockTestState;
    private readonly Mock<ILogger<ConfigController>> _mockLogger;
    private readonly ConfigController _controller;

    public ConfigControllerTests()
    {
        _mockTestState = new Mock<ITestConfigurationState>();
        _mockLogger = new Mock<ILogger<ConfigController>>();
        _controller = new ConfigController(_mockTestState.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullTestState_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ConfigController(null!, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("testState");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ConfigController(_mockTestState.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public void GetConfiguration_ReturnsCurrentState()
    {
        // Arrange
        _mockTestState.Setup(s => s.ForceStartupFail).Returns(true);
        _mockTestState.Setup(s => s.ForceReadyFail).Returns(false);
        _mockTestState.Setup(s => s.ForceLiveFail).Returns(true);
        _mockTestState.Setup(s => s.StartupDelayMs).Returns(100);
        _mockTestState.Setup(s => s.ReadyDelayMs).Returns(200);
        _mockTestState.Setup(s => s.LiveDelayMs).Returns(300);

        // Act
        var result = _controller.GetConfiguration();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<TestConfigurationResponse>().Subject;
        response.ForceStartupFail.Should().BeTrue();
        response.ForceReadyFail.Should().BeFalse();
        response.ForceLiveFail.Should().BeTrue();
        response.StartupDelayMs.Should().Be(100);
        response.ReadyDelayMs.Should().Be(200);
        response.LiveDelayMs.Should().Be(300);
    }

    [Fact]
    public void UpdateConfiguration_WithAllProperties_UpdatesState()
    {
        // Arrange
        var request = new TestConfigurationRequest
        {
            ForceStartupFail = true,
            ForceReadyFail = true,
            ForceLiveFail = true,
            StartupDelayMs = 500,
            ReadyDelayMs = 600,
            LiveDelayMs = 700
        };

        _mockTestState.SetupProperty(s => s.ForceStartupFail);
        _mockTestState.SetupProperty(s => s.ForceReadyFail);
        _mockTestState.SetupProperty(s => s.ForceLiveFail);
        _mockTestState.SetupProperty(s => s.StartupDelayMs);
        _mockTestState.SetupProperty(s => s.ReadyDelayMs);
        _mockTestState.SetupProperty(s => s.LiveDelayMs);

        // Act
        var result = _controller.UpdateConfiguration(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<TestConfigurationResponse>().Subject;
        response.ForceStartupFail.Should().BeTrue();
        response.ForceReadyFail.Should().BeTrue();
        response.ForceLiveFail.Should().BeTrue();
        response.StartupDelayMs.Should().Be(500);
        response.ReadyDelayMs.Should().Be(600);
        response.LiveDelayMs.Should().Be(700);
    }

    [Fact]
    public void UpdateConfiguration_WithPartialProperties_OnlyUpdatesProvidedValues()
    {
        // Arrange
        var request = new TestConfigurationRequest
        {
            ForceReadyFail = true,
            ReadyDelayMs = 1000
        };

        // Pre-set some values
        _mockTestState.SetupProperty(s => s.ForceStartupFail, false);
        _mockTestState.SetupProperty(s => s.ForceReadyFail, false);
        _mockTestState.SetupProperty(s => s.ForceLiveFail, false);
        _mockTestState.SetupProperty(s => s.StartupDelayMs, 0);
        _mockTestState.SetupProperty(s => s.ReadyDelayMs, 0);
        _mockTestState.SetupProperty(s => s.LiveDelayMs, 0);

        // Act
        var result = _controller.UpdateConfiguration(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<TestConfigurationResponse>().Subject;
        
        // Only ForceReadyFail and ReadyDelayMs should be updated
        response.ForceStartupFail.Should().BeFalse();
        response.ForceReadyFail.Should().BeTrue();
        response.ForceLiveFail.Should().BeFalse();
        response.StartupDelayMs.Should().Be(0);
        response.ReadyDelayMs.Should().Be(1000);
        response.LiveDelayMs.Should().Be(0);
    }

    [Fact]
    public void UpdateConfiguration_WithNullRequest_ReturnsBadRequest()
    {
        // Act
        var result = _controller.UpdateConfiguration(null!);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Request body is required");
    }

    [Fact]
    public void UpdateConfiguration_WithEmptyRequest_ReturnsCurrentState()
    {
        // Arrange
        var request = new TestConfigurationRequest();
        _mockTestState.Setup(s => s.ForceStartupFail).Returns(false);
        _mockTestState.Setup(s => s.ForceReadyFail).Returns(false);
        _mockTestState.Setup(s => s.ForceLiveFail).Returns(false);
        _mockTestState.Setup(s => s.StartupDelayMs).Returns(0);
        _mockTestState.Setup(s => s.ReadyDelayMs).Returns(0);
        _mockTestState.Setup(s => s.LiveDelayMs).Returns(0);

        // Act
        var result = _controller.UpdateConfiguration(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<TestConfigurationResponse>();
    }

    [Fact]
    public void UpdateConfiguration_WithOnlyForceStartupFail_OnlyUpdatesForceStartupFail()
    {
        // Arrange
        var request = new TestConfigurationRequest
        {
            ForceStartupFail = true
        };

        _mockTestState.SetupProperty(s => s.ForceStartupFail, false);
        _mockTestState.Setup(s => s.ForceReadyFail).Returns(false);
        _mockTestState.Setup(s => s.ForceLiveFail).Returns(false);
        _mockTestState.Setup(s => s.StartupDelayMs).Returns(0);
        _mockTestState.Setup(s => s.ReadyDelayMs).Returns(0);
        _mockTestState.Setup(s => s.LiveDelayMs).Returns(0);

        // Act
        var result = _controller.UpdateConfiguration(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<TestConfigurationResponse>().Subject;
        response.ForceStartupFail.Should().BeTrue();
        
        // Verify only ForceStartupFail setter was called
        _mockTestState.VerifySet(s => s.ForceStartupFail = true, Times.Once);
        _mockTestState.VerifySet(s => s.ForceReadyFail = It.IsAny<bool>(), Times.Never);
        _mockTestState.VerifySet(s => s.ForceLiveFail = It.IsAny<bool>(), Times.Never);
    }
}
