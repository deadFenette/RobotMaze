using Xunit;
using RobotMaze.RobotLogic;
using System;
using System.Collections.Generic;

namespace RobotMaze.Tests
{
    public class FuzzyLogicTests
    {
        [Fact]
        public void TestFuzzyDistanceToGoal()
        {
            // Arrange
            int x = 0;
            int y = 0;
            (int, int) goal = (2, 2);

            // Act
            double distance = FuzzyLogic.FuzzyDistanceToGoal(x, y, goal);

            // Assert
            Assert.Equal(1 / (1 + Math.Sqrt(8)), distance);
        }

        [Fact]
        public void TestFuzzyObstacleDensity()
        {
            // Arrange
            int[,] field = new int[,]
            {
                { 0, 0, 0 },
                { 0, 1, 0 },
                { 0, 0, 0 }
            };

            // Act
            double density = FuzzyLogic.FuzzyObstacleDensity(1, 1, field);

            // Assert
            Assert.Equal(1.0 / 9.0, density);
        }

        [Fact]
        public void TestFuzzySpeed()
        {
            // Arrange
            double currentSpeed = 2.5;

            // Act
            double fuzzySpeed = FuzzyLogic.FuzzySpeed(currentSpeed);

            // Assert
            Assert.Equal(0.5, fuzzySpeed);
        }

        [Fact]
        public void TestFuzzyDirection()
        {
            // Arrange
            double currentAngle = 30;

            // Act
            double fuzzyDirection = FuzzyLogic.FuzzyDirection(currentAngle);

            // Assert
            Assert.Equal(0.5, fuzzyDirection);
        }

        [Fact]
        public void TestFuzzyEnergyLevel()
        {
            // Arrange
            double energy = 30;

            // Act
            double fuzzyEnergyLevel = FuzzyLogic.FuzzyEnergyLevel(energy);

            // Assert
            Assert.Equal(0.5, fuzzyEnergyLevel);
        }

        [Fact]
        public void TestFuzzyDangerLevel()
        {
            // Arrange
            int[,] dangerField = new int[,]
            {
                { 0, 0, 0 },
                { 0, 1, 0 },
                { 0, 0, 0 }
            };

            // Act
            double dangerLevel = FuzzyLogic.FuzzyDangerLevel(1, 1, dangerField);

            // Assert
            Assert.Equal(1.0 / 9.0, dangerLevel);
        }

        [Fact]
        public void TestFuzzySafetyLevel()
        {
            // Arrange
            int[,] safetyField = new int[,]
            {
                { 0, 0, 0 },
                { 0, 1, 0 },
                { 0, 0, 0 }
            };

            // Act
            double safetyLevel = FuzzyLogic.FuzzySafetyLevel(1, 1, safetyField);

            // Assert
            Assert.Equal(1 - (1.0 / 9.0), safetyLevel);
        }

        [Fact]
        public void TestCalculateDecisionWeight()
        {
            // Arrange
            double distanceToGoal = 0.5;
            double obstacleDensity = 0.2;
            double currentSpeed = 1.0;
            double direction = 0.5;
            double energyLevel = 0.8;
            double dangerLevel = 0.1;
            double safetyLevel = 0.9;
            double confidence = 1.0;

            // Act
            double decisionWeight = FuzzyLogic.CalculateDecisionWeight(distanceToGoal, obstacleDensity, currentSpeed, direction, energyLevel, dangerLevel, safetyLevel, confidence);

            // Assert
            Assert.Equal(0.93, decisionWeight, 2); // Проверка с точностью до 2 десятичных знаков
        }

        [Fact]
        public void TestFuzzyDecision()
        {
            // Arrange
            double distanceToGoal = 0.5;
            double obstacleDensity = 0.2;
            double currentSpeed = 1.0;
            double direction = 0.5;
            double energyLevel = 0.8;
            double dangerLevel = 0.1;
            double safetyLevel = 0.9;
            Queue<(int, int)> memory = new Queue<(int, int)>();
            HashSet<(int, int)> visited = new HashSet<(int, int)>();

            // Act
            double decision = FuzzyLogic.FuzzyDecision(distanceToGoal, obstacleDensity, currentSpeed, direction, energyLevel, dangerLevel, safetyLevel, memory, visited);

            // Assert
            Assert.True(decision > 0);
        }

        [Fact]
        public void TestAvoidOscillation()
        {
            // Arrange
            HashSet<(int, int)> visited = new HashSet<(int, int)>();
            visited.Add((1, 1));

            // Act
            double avoidOscillation = FuzzyLogic.AvoidOscillation(1, 1, visited);

            // Assert
            Assert.Equal(0.0, avoidOscillation);
        }
    }
}
