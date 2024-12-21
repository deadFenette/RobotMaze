using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RobotMaze.RobotLogic
{
    public class ACOLogic
    {
        private readonly int[,] maze;
        private readonly int m;
        private readonly int n;
        private readonly double[,] pheromone;
        private readonly double evaporationRate;
        private readonly double initialPheromone;
        private readonly int maxIterations;
        private readonly int numAnts;
        private readonly Random random;
        private bool _useGoalPosition = true; // Флаг для использования позиции цели
        private List<string> changeLog = new List<string>(); // Журнал изменений

        public double GetPheromone(int i, int j)
        {
            return pheromone[i, j];
        }

        public bool GetUseGoalPosition()
        {
            return _useGoalPosition;
        }

        public ACOLogic(int[,] maze, double evaporationRate, double initialPheromone, int maxIterations, int numAnts)
        {
            this.maze = maze;
            this.m = maze.GetLength(0);
            this.n = maze.GetLength(1);
            this.evaporationRate = evaporationRate;
            this.initialPheromone = initialPheromone;
            this.maxIterations = maxIterations;
            this.numAnts = numAnts;
            this.pheromone = new double[m, n];
            this.random = new Random();

            InitializePheromone();
        }

        public void SetUseGoalPosition(bool useGoalPosition)
        {
            _useGoalPosition = useGoalPosition;
        }

        private void InitializePheromone()
        {
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    pheromone[i, j] = initialPheromone;
                }
            }
        }

        public List<(int, int)> FindPath((int, int) start, (int, int) goal)
        {
            Debug.WriteLine("ACO algorithm started.");
            List<(int, int)> bestPath = null;
            double bestPathLength = double.MaxValue;

            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                List<(int, int)>[] antPaths = new List<(int, int)>[numAnts];
                double[] antPathLengths = new double[numAnts];

                for (int ant = 0; ant < numAnts; ant++)
                {
                    antPaths[ant] = new List<(int, int)>();
                    antPathLengths[ant] = 0;
                    (int, int) currentPosition = start;
                    int steps = 0;
                    const int maxSteps = 1000; // Ограничение на количество шагов

                    while (currentPosition != goal && steps < maxSteps)
                    {
                        antPaths[ant].Add(currentPosition);
                        (int, int) nextPosition = ChooseNextMove(currentPosition, goal);
                        antPathLengths[ant] += CalculateDistance(currentPosition, nextPosition);
                        currentPosition = nextPosition;
                        steps++;
                    }

                    if (currentPosition == goal)
                    {
                        antPaths[ant].Add(goal);
                    }
                }

                UpdatePheromone(antPaths, antPathLengths);

                for (int ant = 0; ant < numAnts; ant++)
                {
                    if (antPathLengths[ant] < bestPathLength)
                    {
                        bestPathLength = antPathLengths[ant];
                        bestPath = antPaths[ant];
                    }
                }
            }

            Debug.WriteLine("ACO algorithm finished.");
            return bestPath;
        }

        internal (int, int) ChooseNextMove((int, int) current, (int, int) goal)
        {
            List<(int, int)> possibleMoves = new List<(int, int)>
            {
                (current.Item1, current.Item2 + 1),
                (current.Item1 + 1, current.Item2),
                (current.Item1, current.Item2 - 1),
                (current.Item1 - 1, current.Item2)
            };

            possibleMoves = possibleMoves.FindAll(move =>
                move.Item1 >= 0 && move.Item1 < m &&
                move.Item2 >= 0 && move.Item2 < n &&
                maze[move.Item1, move.Item2] != 1); // Избегаем стен

            if (possibleMoves.Count == 0)
            {
                return current;
            }

            double[] probabilities = new double[possibleMoves.Count];
            double total = 0;

            for (int i = 0; i < possibleMoves.Count; i++)
            {
                double pheromoneValue = pheromone[possibleMoves[i].Item1, possibleMoves[i].Item2];
                double heuristicValue = _useGoalPosition ? 1.0 / CalculateDistance(possibleMoves[i], goal) : 1.0;
                probabilities[i] = pheromoneValue * heuristicValue;
                total += probabilities[i];
            }

            for (int i = 0; i < possibleMoves.Count; i++)
            {
                probabilities[i] /= total;
            }

            double r = random.NextDouble();
            double cumulativeProbability = 0.0;

            for (int i = 0; i < possibleMoves.Count; i++)
            {
                cumulativeProbability += probabilities[i];
                if (r <= cumulativeProbability)
                {
                    return possibleMoves[i];
                }
            }

            return current;
        }

        internal double CalculateDistance((int, int) a, (int, int) b)
        {
            return Math.Sqrt(Math.Pow(a.Item1 - b.Item1, 2) + Math.Pow(a.Item2 - b.Item2, 2));
        }

        internal void UpdatePheromone(List<(int, int)>[] antPaths, double[] antPathLengths)
        {
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    pheromone[i, j] *= (1 - evaporationRate);
                }
            }

            for (int ant = 0; ant < numAnts; ant++)
            {
                double contribution = 1.0 / antPathLengths[ant];
                foreach (var position in antPaths[ant])
                {
                    pheromone[position.Item1, position.Item2] += contribution;
                    changeLog.Add($"Ant {ant} updated pheromone at ({position.Item1}, {position.Item2}) to {pheromone[position.Item1, position.Item2]}");
                }
            }
        }

        public List<string> GetChangeLog()
        {
            return changeLog;
        }
    }
}
