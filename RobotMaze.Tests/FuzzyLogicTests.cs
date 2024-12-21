using Xunit;
using RobotMaze.RobotLogic;
using System;
using System.Collections.Generic;

namespace RobotMaze.Tests
{
    public class FuzzyLogicTests
    {

        [Theory]
        [InlineData(0, 0, 0, 0, 1)]
        [InlineData(1, 1, 1, 1, 1)]
        [InlineData(2, 2, 2, 2, 1)]
        public void FuzzyDistanceToGoal_ShouldCalculateCorrectDistance(int x, int y, int goalX, int goalY, double expected)
        {
            var goal = (goalX, goalY);
            double result = FuzzyLogic.FuzzyDistanceToGoal(x, y, goal);
            Assert.Equal(expected, result, 5);
        }

        [Theory]
        [InlineData(0.5, 0.1)]
        [InlineData(2, 0.5)]
        [InlineData(5, 1.0)]
        public void FuzzySpeed_ShouldCalculateCorrectSpeed(double currentSpeed, double expected)
        {
            double result = FuzzyLogic.FuzzySpeed(currentSpeed);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(0, 0.9)]
        [InlineData(45, 0.5)]
        [InlineData(90, 0.5)]
        [InlineData(180, 0.1)]
        public void FuzzyDirection_ShouldCalculateCorrectDirection(double currentAngle, double expected)
        {
            double result = FuzzyLogic.FuzzyDirection(currentAngle);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(10, 0.1)]
        [InlineData(30, 0.5)]
        [InlineData(60, 1.0)]
        public void FuzzyEnergyLevel_ShouldCalculateCorrectEnergyLevel(double energy, double expected)
        {
            double result = FuzzyLogic.FuzzyEnergyLevel(energy);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.7, 0.98)]
        [InlineData(1, 1, 1, 1, 1, 1, 1, 1, 2.8)]
        public void CalculateDecisionWeight_ShouldCalculateCorrectWeight(double distanceToGoal, double obstacleDensity, double currentSpeed, double direction, double energyLevel, double dangerLevel, double safetyLevel, double confidence, double expected)
        {
            double result = FuzzyLogic.CalculateDecisionWeight(distanceToGoal, obstacleDensity, currentSpeed, direction, energyLevel, dangerLevel, safetyLevel, confidence);
            Assert.Equal(expected, result, 5);
        }
    }
}
