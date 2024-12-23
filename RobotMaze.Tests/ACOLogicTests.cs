using Xunit;
using RobotMaze.RobotLogic;
using System;
using System.Collections.Generic;

namespace RobotMaze.Tests
{
    public class ACOLogicTests
    {
        [Fact]
        public void InitializePheromone_ShouldSetInitialPheromone()
        {
            int[,] maze = new int[,] { { 0, 0 }, { 0, 0 } };
            double evaporationRate = 0.1;
            double initialPheromone = 1.0;
            int maxIterations = 10;
            int numAnts = 5;

            ACOLogic acoLogic = new ACOLogic(maze, evaporationRate, initialPheromone, maxIterations, numAnts);

            for (int i = 0; i < maze.GetLength(0); i++)
            {
                for (int j = 0; j < maze.GetLength(1); j++)
                {
                    Assert.Equal(initialPheromone, acoLogic.GetPheromone(i, j));
                }
            }
        }

        [Fact]
        public void SetUseGoalPosition_ShouldSetFlag()
        {
            int[,] maze = new int[,] { { 0, 0 }, { 0, 0 } };
            double evaporationRate = 0.1;
            double initialPheromone = 1.0;
            int maxIterations = 10;
            int numAnts = 5;

            ACOLogic acoLogic = new ACOLogic(maze, evaporationRate, initialPheromone, maxIterations, numAnts);

            acoLogic.SetUseGoalPosition(false);
            Assert.False(acoLogic.GetUseGoalPosition());

            acoLogic.SetUseGoalPosition(true);
            Assert.True(acoLogic.GetUseGoalPosition());
        }

        [Theory]
        [InlineData(0, 0, 0, 0, 0)]
        [InlineData(1, 1, 1, 1, 0)] // Исправлено ожидаемое значение
        [InlineData(2, 2, 2, 2, 0)]
        public void CalculateDistance_ShouldCalculateCorrectDistance(int x1, int y1, int x2, int y2, double expected)
        {
            int[,] maze = new int[,] { { 0, 0 }, { 0, 0 } };
            double evaporationRate = 0.1;
            double initialPheromone = 1.0;
            int maxIterations = 10;
            int numAnts = 5;

            ACOLogic acoLogic = new ACOLogic(maze, evaporationRate, initialPheromone, maxIterations, numAnts);

            double result = acoLogic.CalculateDistance((x1, y1), (x2, y2));
            Assert.Equal(expected, result, 5);
        }

        [Fact]
        public void ChooseNextMove_ShouldChooseValidMove()
        {
            int[,] maze = new int[,] { { 0, 0 }, { 0, 0 } };
            double evaporationRate = 0.1;
            double initialPheromone = 1.0;
            int maxIterations = 10;
            int numAnts = 5;

            ACOLogic acoLogic = new ACOLogic(maze, evaporationRate, initialPheromone, maxIterations, numAnts);

            var current = (0, 0);
            var goal = (1, 1);
            var nextMove = acoLogic.ChooseNextMove(current, goal);

            Assert.Contains(nextMove, new List<(int, int)> { (0, 1), (1, 0) });
        }

        [Fact]
        public void GetChangeLog_ShouldReturnChangeLog()
        {
            int[,] maze = new int[,] { { 0, 0 }, { 0, 0 } };
            double evaporationRate = 0.1;
            double initialPheromone = 1.0;
            int maxIterations = 10;
            int numAnts = 5;

            ACOLogic acoLogic = new ACOLogic(maze, evaporationRate, initialPheromone, maxIterations, numAnts);

            List<(int, int)>[] antPaths = new List<(int, int)>[numAnts];
            double[] antPathLengths = new double[numAnts];

            for (int ant = 0; ant < numAnts; ant++)
            {
                antPaths[ant] = new List<(int, int)> { (0, 0), (1, 1) };
                antPathLengths[ant] = Math.Sqrt(2);
            }

            acoLogic.UpdatePheromone(antPaths, antPathLengths);

            var changeLog = acoLogic.GetChangeLog();
            Assert.NotEmpty(changeLog);
        }
    }
}
