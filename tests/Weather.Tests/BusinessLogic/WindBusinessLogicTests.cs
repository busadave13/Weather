using FluentAssertions;
using Moq;
using Weather.BusinessLogic;
using Weather.Clients;
using Weather.Clients.Models;

namespace Weather.Tests.BusinessLogic;

/// <summary>
/// Unit tests for <see cref="WindBusinessLogic"/>.
/// </summary>
public class WindBusinessLogicTests
{
    private readonly Mock<IWindSensorClient> _mockClient;
    private readonly WindBusinessLogic _sut;

    public WindBusinessLogicTests()
    {
        _mockClient = new Mock<IWindSensorClient>();
        _sut = new WindBusinessLogic(_mockClient.Object);
    }

    [Fact]
    public async Task GetCurrentWindAsync_ShouldMapSensorResponseCorrectly()
    {
        // Arrange
        var sensorResponse = new SensorWindResponse
        {
            WindSpeed = 15.5m,
            SpeedUnit = "mph",
            WindDirection = 45,
            GustSpeed = 22.0m,
            ReadingTime = DateTime.UtcNow
        };

        _mockClient
            .Setup(x => x.GetWindAsync())
            .ReturnsAsync(sensorResponse);

        // Act
        var result = await _sut.GetCurrentWindAsync();

        // Assert
        result.Should().NotBeNull();
        result.Speed.Should().Be(15.5m);
        result.Unit.Should().Be("mph");
        result.Direction.Should().Be("NE");
        result.Gusts.Should().Be(22.0m);
    }

    [Theory]
    [InlineData(0, "N")]
    [InlineData(45, "NE")]
    [InlineData(90, "E")]
    [InlineData(135, "SE")]
    [InlineData(180, "S")]
    [InlineData(225, "SW")]
    [InlineData(270, "W")]
    [InlineData(315, "NW")]
    [InlineData(360, "N")]
    public async Task GetCurrentWindAsync_ShouldConvertDegreesToCardinalCorrectly(int degrees, string expectedDirection)
    {
        // Arrange
        var sensorResponse = new SensorWindResponse
        {
            WindSpeed = 10.0m,
            SpeedUnit = "mph",
            WindDirection = degrees,
            GustSpeed = 15.0m,
            ReadingTime = DateTime.UtcNow
        };

        _mockClient
            .Setup(x => x.GetWindAsync())
            .ReturnsAsync(sensorResponse);

        // Act
        var result = await _sut.GetCurrentWindAsync();

        // Assert
        result.Direction.Should().Be(expectedDirection);
    }

    [Fact]
    public async Task GetCurrentWindAsync_ShouldCallClientOnce()
    {
        // Arrange
        var sensorResponse = new SensorWindResponse
        {
            WindSpeed = 10.0m,
            SpeedUnit = "mph",
            WindDirection = 90,
            GustSpeed = 15.0m,
            ReadingTime = DateTime.UtcNow
        };

        _mockClient
            .Setup(x => x.GetWindAsync())
            .ReturnsAsync(sensorResponse);

        // Act
        await _sut.GetCurrentWindAsync();

        // Assert
        _mockClient.Verify(x => x.GetWindAsync(), Times.Once);
    }
}
