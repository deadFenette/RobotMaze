using Xunit;
using RobotMaze.Metrics;
using System;

namespace RobotMaze.Tests
{
    public class RobotMetricsTests
    {
        [Fact]
        public void TestUpdateMetrics()
        {
            // Arrange
            var metrics = new RobotMetrics();
            int steps = 5;
            int energy = 10;
            TimeSpan time = TimeSpan.FromSeconds(30);
            int distance = 20;

            // Act
            metrics.UpdateMetrics(steps, energy, time, distance);

            // Assert
            Assert.Equal(5, metrics.StepsTaken);
            Assert.Equal(10, metrics.EnergyConsumed);
            Assert.Equal(TimeSpan.FromSeconds(30), metrics.TimeElapsed);
            Assert.Equal(20, metrics.DistanceTraveled);
        }

        [Fact]
        public void TestToString()
        {
            // Arrange
            var metrics = new RobotMetrics();
            metrics.UpdateMetrics(5, 10, TimeSpan.FromSeconds(30), 20);

            // Act
            string result = metrics.ToString();

            // Assert
            Assert.Equal("5, 10, 00:00:30, 20", result);
        }
    }
}
