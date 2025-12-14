using FluentAssertions;
using Moq;
using Weather.BusinessLogic;
using Weather.Clients;
using Weather.Clients.Models;

namespace Weather.Tests.BusinessLogic;

/// <summary>
/// Unit tests for <see cref="WeatherBusinessLogic"/>.
/// </summary>
public class WeatherBusinessLogicTests
{
    private readonly Mock<ITemperatureSensorClient> _mockTempClient;
    private readonly Mock<IWindSensorClient> _mockWindClient;
    private readonly Mock<IPrecipitationSensorClient> _mockPrecipClient;
    private readonly WeatherBusinessLogic _sut;

    public WeatherBusinessLogicTests()
    {
        _mockTempClient = new Mock<ITemperatureSensorClient>();
        _mockWindClient = new Mock<IWindSensorClient>();
        _mockPrecipClient = new Mock<IPrecipitationSensorClient>();
        _sut = new WeatherBusinessLogic(
            _mockTempClient.Object,
            _mockWindClient.Object,
            _mockPrecipClient.Object);
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_ShouldMapAllSensorResponsesCorrectly()
    {
        // Arrange
        var tempResponse = new SensorTemperatureResponse
        {
            Temp = 72.5m,
            TempUnit = "F",
            ApparentTemp = 75.0m,
            ReadingTime = DateTime.UtcNow
        };

        var windResponse = new SensorWindResponse
        {
            WindSpeed = 15.5m,
            SpeedUnit = "mph",
            WindDirection = 90,
            GustSpeed = 22.0m,
            ReadingTime = DateTime.UtcNow
        };

        var precipResponse = new SensorPrecipitationResponse
        {
            PrecipAmount = 0.25m,
            PrecipUnit = "in",
            PrecipType = "rain",
            RelativeHumidity = 85,
            ReadingTime = DateTime.UtcNow
        };

        _mockTempClient
            .Setup(x => x.GetTemperatureAsync())
            .ReturnsAsync(tempResponse);

        _mockWindClient
            .Setup(x => x.GetWindAsync())
            .ReturnsAsync(windResponse);

        _mockPrecipClient
            .Setup(x => x.GetPrecipitationAsync())
            .ReturnsAsync(precipResponse);

        // Act
        var result = await _sut.GetCurrentWeatherAsync();

        // Assert
        result.Should().NotBeNull();

        // Temperature assertions
        result.Temperature.Should().NotBeNull();
        result.Temperature.Value.Should().Be(72.5m);
        result.Temperature.Unit.Should().Be("F");
        result.Temperature.FeelsLike.Should().Be(75.0m);

        // Wind assertions
        result.Wind.Should().NotBeNull();
        result.Wind.Speed.Should().Be(15.5m);
        result.Wind.Unit.Should().Be("mph");
        result.Wind.Direction.Should().Be("E");
        result.Wind.Gusts.Should().Be(22.0m);

        // Precipitation assertions
        result.Precipitation.Should().NotBeNull();
        result.Precipitation.Amount.Should().Be(0.25m);
        result.Precipitation.Unit.Should().Be("in");
        result.Precipitation.Type.Should().Be("rain");
        result.Precipitation.Humidity.Should().Be(85);

        // Timestamp should be set
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_ShouldCallAllClientsOnce()
    {
        // Arrange
        SetupDefaultMockResponses();

        // Act
        await _sut.GetCurrentWeatherAsync();

        // Assert
        _mockTempClient.Verify(x => x.GetTemperatureAsync(), Times.Once);
        _mockWindClient.Verify(x => x.GetWindAsync(), Times.Once);
        _mockPrecipClient.Verify(x => x.GetPrecipitationAsync(), Times.Once);
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_ShouldCallAllClientsInParallel()
    {
        // Arrange
        var callOrder = new List<string>();

        _mockTempClient
            .Setup(x => x.GetTemperatureAsync())
            .Returns(async () =>
            {
                callOrder.Add("temp_start");
                await Task.Delay(50);
                callOrder.Add("temp_end");
                return new SensorTemperatureResponse
                {
                    Temp = 72.5m,
                    TempUnit = "F",
                    ApparentTemp = 75.0m,
                    ReadingTime = DateTime.UtcNow
                };
            });

        _mockWindClient
            .Setup(x => x.GetWindAsync())
            .Returns(async () =>
            {
                callOrder.Add("wind_start");
                await Task.Delay(50);
                callOrder.Add("wind_end");
                return new SensorWindResponse
                {
                    WindSpeed = 15.5m,
                    SpeedUnit = "mph",
                    WindDirection = 90,
                    GustSpeed = 22.0m,
                    ReadingTime = DateTime.UtcNow
                };
            });

        _mockPrecipClient
            .Setup(x => x.GetPrecipitationAsync())
            .Returns(async () =>
            {
                callOrder.Add("precip_start");
                await Task.Delay(50);
                callOrder.Add("precip_end");
                return new SensorPrecipitationResponse
                {
                    PrecipAmount = 0.25m,
                    PrecipUnit = "in",
                    PrecipType = "rain",
                    RelativeHumidity = 85,
                    ReadingTime = DateTime.UtcNow
                };
            });

        // Act
        await _sut.GetCurrentWeatherAsync();

        // Assert - all starts should come before all ends (parallel execution)
        var startIndices = new[]
        {
            callOrder.IndexOf("temp_start"),
            callOrder.IndexOf("wind_start"),
            callOrder.IndexOf("precip_start")
        };

        var endIndices = new[]
        {
            callOrder.IndexOf("temp_end"),
            callOrder.IndexOf("wind_end"),
            callOrder.IndexOf("precip_end")
        };

        // All calls should have started
        startIndices.Should().NotContain(-1);
        endIndices.Should().NotContain(-1);

        // The maximum start index should be less than the minimum end index
        // This indicates parallel execution
        startIndices.Max().Should().BeLessThan(endIndices.Min());
    }

    private void SetupDefaultMockResponses()
    {
        _mockTempClient
            .Setup(x => x.GetTemperatureAsync())
            .ReturnsAsync(new SensorTemperatureResponse
            {
                Temp = 72.5m,
                TempUnit = "F",
                ApparentTemp = 75.0m,
                ReadingTime = DateTime.UtcNow
            });

        _mockWindClient
            .Setup(x => x.GetWindAsync())
            .ReturnsAsync(new SensorWindResponse
            {
                WindSpeed = 15.5m,
                SpeedUnit = "mph",
                WindDirection = 90,
                GustSpeed = 22.0m,
                ReadingTime = DateTime.UtcNow
            });

        _mockPrecipClient
            .Setup(x => x.GetPrecipitationAsync())
            .ReturnsAsync(new SensorPrecipitationResponse
            {
                PrecipAmount = 0.25m,
                PrecipUnit = "in",
                PrecipType = "rain",
                RelativeHumidity = 85,
                ReadingTime = DateTime.UtcNow
            });
    }
}
