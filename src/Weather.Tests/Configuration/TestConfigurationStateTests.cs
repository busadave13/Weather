using FluentAssertions;
using Weather.Configuration;

namespace Weather.Tests.Configuration;

public class TestConfigurationStateTests
{
    [Fact]
    public void DefaultValues_AreAllFalseAndZero()
    {
        // Arrange & Act
        var state = new TestConfigurationState();

        // Assert
        state.ForceStartupFail.Should().BeFalse();
        state.ForceReadyFail.Should().BeFalse();
        state.ForceLiveFail.Should().BeFalse();
        state.StartupDelayMs.Should().Be(0);
        state.ReadyDelayMs.Should().Be(0);
        state.LiveDelayMs.Should().Be(0);
    }

    [Fact]
    public void ForceStartupFail_CanBeSetAndRetrieved()
    {
        // Arrange
        var state = new TestConfigurationState();

        // Act
        state.ForceStartupFail = true;

        // Assert
        state.ForceStartupFail.Should().BeTrue();
    }

    [Fact]
    public void ForceReadyFail_CanBeSetAndRetrieved()
    {
        // Arrange
        var state = new TestConfigurationState();

        // Act
        state.ForceReadyFail = true;

        // Assert
        state.ForceReadyFail.Should().BeTrue();
    }

    [Fact]
    public void ForceLiveFail_CanBeSetAndRetrieved()
    {
        // Arrange
        var state = new TestConfigurationState();

        // Act
        state.ForceLiveFail = true;

        // Assert
        state.ForceLiveFail.Should().BeTrue();
    }

    [Fact]
    public void StartupDelayMs_CanBeSetAndRetrieved()
    {
        // Arrange
        var state = new TestConfigurationState();

        // Act
        state.StartupDelayMs = 500;

        // Assert
        state.StartupDelayMs.Should().Be(500);
    }

    [Fact]
    public void ReadyDelayMs_CanBeSetAndRetrieved()
    {
        // Arrange
        var state = new TestConfigurationState();

        // Act
        state.ReadyDelayMs = 1000;

        // Assert
        state.ReadyDelayMs.Should().Be(1000);
    }

    [Fact]
    public void LiveDelayMs_CanBeSetAndRetrieved()
    {
        // Arrange
        var state = new TestConfigurationState();

        // Act
        state.LiveDelayMs = 2000;

        // Assert
        state.LiveDelayMs.Should().Be(2000);
    }

    [Fact]
    public void StartupDelayMs_WithNegativeValue_SetsToZero()
    {
        // Arrange
        var state = new TestConfigurationState();

        // Act
        state.StartupDelayMs = -100;

        // Assert
        state.StartupDelayMs.Should().Be(0);
    }

    [Fact]
    public void ReadyDelayMs_WithNegativeValue_SetsToZero()
    {
        // Arrange
        var state = new TestConfigurationState();

        // Act
        state.ReadyDelayMs = -100;

        // Assert
        state.ReadyDelayMs.Should().Be(0);
    }

    [Fact]
    public void LiveDelayMs_WithNegativeValue_SetsToZero()
    {
        // Arrange
        var state = new TestConfigurationState();

        // Act
        state.LiveDelayMs = -100;

        // Assert
        state.LiveDelayMs.Should().Be(0);
    }

    [Fact]
    public void AllProperties_CanBeSetIndependently()
    {
        // Arrange
        var state = new TestConfigurationState();

        // Act
        state.ForceStartupFail = true;
        state.ForceReadyFail = false;
        state.ForceLiveFail = true;
        state.StartupDelayMs = 100;
        state.ReadyDelayMs = 200;
        state.LiveDelayMs = 300;

        // Assert
        state.ForceStartupFail.Should().BeTrue();
        state.ForceReadyFail.Should().BeFalse();
        state.ForceLiveFail.Should().BeTrue();
        state.StartupDelayMs.Should().Be(100);
        state.ReadyDelayMs.Should().Be(200);
        state.LiveDelayMs.Should().Be(300);
    }
}
