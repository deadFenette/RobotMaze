using System.Collections.Generic;
using Xunit;
using RobotMaze.RobotLogic;

namespace RobotMaze.Tests
{
    public class ACOLogicTests
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
            var acoLogic = new ACOLogic(maze, 0.5, 1.0, 100, 10);

            // Act
            var path = acoLogic.FindPath(start, goal);

            // Assert
            Assert.NotNull(path);
            Assert.Equal(goal, path[path.Count - 1]);
        }

        [Fact]
        public void InitializePheromone_ShouldSetInitialPheromone()
        {
            // Arrange
            int[,] maze = new int[,]
            {
                { 0, 0, 0, 0 },
                { 0, 1, 1, 0 },
                { 0, 0, 0, 0 },
                { 0, 1, 1, 0 }
            };
            var acoLogic = new ACOLogic(maze, 0.5, 1.0, 100, 10);

            // Act
            var pheromone = acoLogic.GetType().GetField("pheromone", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(acoLogic) as double[,];

            // Assert
            for (int i = 0; i < maze.GetLength(0); i++)
            {
                for (int j = 0; j < maze.GetLength(1); j++)
                {
                    Assert.Equal(1.0, pheromone[i, j]);
                }
            }
        }
    }
}
