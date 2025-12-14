using FluentAssertions;
using Moq;
using Weather.BusinessLogic;
using Weather.Clients;
using Weather.Clients.Models;

namespace Weather.Tests.BusinessLogic;

/// <summary>
/// Unit tests for <see cref="PrecipitationBusinessLogic"/>.
/// </summary>
public class PrecipitationBusinessLogicTests
{
    private readonly Mock<IPrecipitationSensorClient> _mockClient;
    private readonly PrecipitationBusinessLogic _sut;

    public PrecipitationBusinessLogicTests()
    {
        _mockClient = new Mock<IPrecipitationSensorClient>();
        _sut = new PrecipitationBusinessLogic(_mockClient.Object);
    }

    [Fact]
    public async Task GetCurrentPrecipitationAsync_ShouldMapSensorResponseCorrectly()
    {
        // Arrange
        var sensorResponse = new SensorPrecipitationResponse
        {
            PrecipAmount = 0.25m,
            PrecipUnit = "in",
            PrecipType = "rain",
            RelativeHumidity = 85,
            ReadingTime = DateTime.UtcNow
        };

        _mockClient
            .Setup(x => x.GetPrecipitationAsync())
            .ReturnsAsync(sensorResponse);

        // Act
        var result = await _sut.GetCurrentPrecipitationAsync();

        // Assert
        result.Should().NotBeNull();
        result.Amount.Should().Be(0.25m);
        result.Unit.Should().Be("in");
        result.Type.Should().Be("rain");
        result.Humidity.Should().Be(85);
    }

    [Fact]
    public async Task GetCurrentPrecipitationAsync_WithNoPrecipitation_ShouldMapCorrectly()
    {
        // Arrange
        var sensorResponse = new SensorPrecipitationResponse
        {
            PrecipAmount = 0m,
            PrecipUnit = "in",
            PrecipType = "none",
            RelativeHumidity = 45,
            ReadingTime = DateTime.UtcNow
        };

        _mockClient
            .Setup(x => x.GetPrecipitationAsync())
            .ReturnsAsync(sensorResponse);

        // Act
        var result = await _sut.GetCurrentPrecipitationAsync();

        // Assert
        result.Should().NotBeNull();
        result.Amount.Should().Be(0m);
        result.Type.Should().Be("none");
        result.Humidity.Should().Be(45);
    }

    [Fact]
    public async Task GetCurrentPrecipitationAsync_ShouldCallClientOnce()
    {
        // Arrange
        var sensorResponse = new SensorPrecipitationResponse
        {
            PrecipAmount = 0m,
            PrecipUnit = "in",
            PrecipType = "none",
            RelativeHumidity = 50,
            ReadingTime = DateTime.UtcNow
        };

        _mockClient
            .Setup(x => x.GetPrecipitationAsync())
            .ReturnsAsync(sensorResponse);

        // Act
        await _sut.GetCurrentPrecipitationAsync();

        // Assert
        _mockClient.Verify(x => x.GetPrecipitationAsync(), Times.Once);
    }
}
