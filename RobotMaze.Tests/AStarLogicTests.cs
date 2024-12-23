using System.Collections.Generic;
using Xunit;
using RobotMaze.RobotLogic;

namespace RobotMaze.Tests
{
    public class AStarLogicTests
    {
        [Fact]
        public void FindPath_ShouldReturnCorrectPath()
        {
            // Arrange
            int[,] maze = new int[,]
            {
                { 0, 0, 0, 0 },
                { 0, 1, 1, 0 },
                { 0, 0, 0, 0 },
                { 0, 1, 1, 0 }
            };
            var start = (0, 0);
            var goal = (3, 3);
            var aStarLogic = new AStarLogic(maze, 2);

            // Act
            var path = aStarLogic.FindPath(start, goal);

            // Assert
            Assert.NotNull(path);
            Assert.Equal(goal, path[path.Count - 1]);
        }

        [Fact]
        public void FindPath_ShouldReturnEmptyPath_WhenNoPathExists()
        {
            // Arrange
            int[,] maze = new int[,]
            {
                { 0, 1, 1, 1 },
                { 0, 1, 1, 1 },
                { 0, 1, 1, 1 },
                { 0, 1, 1, 0 }
            };
            var start = (0, 0);
            var goal = (3, 3);
            var aStarLogic = new AStarLogic(maze, 2);

            // Act
            var path = aStarLogic.FindPath(start, goal);

            // Assert
            Assert.Empty(path);
        }

        [Fact]
        public void SetDistanceWeight_ShouldUpdateDistanceWeight()
        {
            // Arrange
            int[,] maze = new int[,]
            {
                { 0, 0, 0, 0 },
                { 0, 1, 1, 0 },
                { 0, 0, 0, 0 },
                { 0, 1, 1, 0 }
            };
            var aStarLogic = new AStarLogic(maze, 2);

            // Act
            aStarLogic.SetDistanceWeight(0.5);

            // Assert
            var distanceWeightField = typeof(AStarLogic).GetField("_distanceWeight", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var distanceWeight = (double)distanceWeightField.GetValue(aStarLogic);
            Assert.Equal(0.5, distanceWeight);
        }

        [Fact]
        public void SetUseGoalPosition_ShouldUpdateUseGoalPosition()
        {
            // Arrange
            int[,] maze = new int[,]
            {
                { 0, 0, 0, 0 },
                { 0, 1, 1, 0 },
                { 0, 0, 0, 0 },
                { 0, 1, 1, 0 }
            };
            var aStarLogic = new AStarLogic(maze, 2);

            // Act
            aStarLogic.SetUseGoalPosition(false);

            // Assert
            var useGoalPositionField = typeof(AStarLogic).GetField("_useGoalPosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var useGoalPosition = (bool)useGoalPositionField.GetValue(aStarLogic);
            Assert.False(useGoalPosition);
        }
    }
}
