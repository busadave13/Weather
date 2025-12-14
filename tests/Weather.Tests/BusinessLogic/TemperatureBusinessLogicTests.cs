using FluentAssertions;
using Moq;
using Weather.BusinessLogic;
using Weather.Clients;
using Weather.Clients.Models;

namespace Weather.Tests.BusinessLogic;

/// <summary>
/// Unit tests for <see cref="TemperatureBusinessLogic"/>.
/// </summary>
public class TemperatureBusinessLogicTests
{
    private readonly Mock<ITemperatureSensorClient> _mockClient;
    private readonly TemperatureBusinessLogic _sut;

    public TemperatureBusinessLogicTests()
    {
        _mockClient = new Mock<ITemperatureSensorClient>();
        _sut = new TemperatureBusinessLogic(_mockClient.Object);
    }

    [Fact]
    public async Task GetCurrentTemperatureAsync_ShouldMapSensorResponseCorrectly()
    {
        // Arrange
        var sensorResponse = new SensorTemperatureResponse
        {
            Temp = 72.5m,
            TempUnit = "F",
            ApparentTemp = 75.0m,
            ReadingTime = DateTime.UtcNow
        };

        _mockClient
            .Setup(x => x.GetTemperatureAsync())
            .ReturnsAsync(sensorResponse);

        // Act
        var result = await _sut.GetCurrentTemperatureAsync();

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(72.5m);
        result.Unit.Should().Be("F");
        result.FeelsLike.Should().Be(75.0m);
    }

    [Fact]
    public async Task GetCurrentTemperatureAsync_ShouldCallClientOnce()
    {
        // Arrange
        var sensorResponse = new SensorTemperatureResponse
        {
            Temp = 20.0m,
            TempUnit = "C",
            ApparentTemp = 18.0m,
            ReadingTime = DateTime.UtcNow
        };

        _mockClient
            .Setup(x => x.GetTemperatureAsync())
            .ReturnsAsync(sensorResponse);

        // Act
        await _sut.GetCurrentTemperatureAsync();

        // Assert
        _mockClient.Verify(x => x.GetTemperatureAsync(), Times.Once);
    }
}
