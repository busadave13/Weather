using FluentAssertions;
using Weather.HealthChecks;

namespace Weather.Tests.HealthChecks;

public class RequestCounterTests
{
    [Fact]
    public void CurrentCount_InitialValue_ReturnsZero()
    {
        // Arrange
        var counter = new RequestCounter();

        // Act
        var result = counter.CurrentCount;

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void IncrementAndGet_FirstCall_ReturnsOne()
    {
        // Arrange
        var counter = new RequestCounter();

        // Act
        var result = counter.IncrementAndGet();

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void IncrementAndGet_MultipleCalls_ReturnsIncrementedValues()
    {
        // Arrange
        var counter = new RequestCounter();

        // Act & Assert
        counter.IncrementAndGet().Should().Be(1);
        counter.IncrementAndGet().Should().Be(2);
        counter.IncrementAndGet().Should().Be(3);
        counter.CurrentCount.Should().Be(3);
    }

    [Fact]
    public async Task IncrementAndGet_ConcurrentCalls_CountsAllRequests()
    {
        // Arrange
        var counter = new RequestCounter();
        const int threadCount = 100;
        const int incrementsPerThread = 1000;
        var tasks = new Task[threadCount];

        // Act
        for (int i = 0; i < threadCount; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                for (int j = 0; j < incrementsPerThread; j++)
                {
                    counter.IncrementAndGet();
                }
            });
        }

        await Task.WhenAll(tasks);

        // Assert
        counter.CurrentCount.Should().Be(threadCount * incrementsPerThread);
    }

    [Fact]
    public void CurrentCount_AfterIncrements_ReturnsCorrectValue()
    {
        // Arrange
        var counter = new RequestCounter();

        // Act
        for (int i = 0; i < 10; i++)
        {
            counter.IncrementAndGet();
        }

        // Assert
        counter.CurrentCount.Should().Be(10);
    }
}
