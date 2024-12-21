
using System;
using System.Collections.Generic;
using System.Linq;

namespace RobotMaze.RobotLogic
{
    public class AStarLogic
    {
        private readonly int[,] _maze;
        private readonly int _rows;
        private readonly int _cols;
        private readonly int _spikeCost;
        private double _distanceWeight = 0.8; // Параметр расстояния до цели, который можно менять
        private bool _useGoalPosition = true; // Флаг для использования позиции цели

        public AStarLogic(int[,] maze, int spikeCost)
        {
            _maze = maze;
            _rows = maze.GetLength(0);
            _cols = maze.GetLength(1);
            _spikeCost = spikeCost;
        }

        public void SetDistanceWeight(double weight)
        {
            _distanceWeight = weight;
        }

        public void SetUseGoalPosition(bool useGoalPosition)
        {
            _useGoalPosition = useGoalPosition;
        }

        public List<(int, int)> FindPath((int, int) start, (int, int) goal)
        {
            var openSet = new SortedSet<(double, (int, int))>(Comparer<(double, (int, int))>.Create((a, b) => a.Item1.Equals(b.Item1) ? a.Item2.CompareTo(b.Item2) : a.Item1.CompareTo(b.Item1)));
            var cameFrom = new Dictionary<(int, int), (int, int)>();
            var gScore = new Dictionary<(int, int), double>();
            var fScore = new Dictionary<(int, int), double>();

            for (int r = 0; r < _rows; r++)
            {
                for (int c = 0; c < _cols; c++)
                {
                    gScore[(r, c)] = double.MaxValue;
                    fScore[(r, c)] = double.MaxValue;
                }
            }

            gScore[start] = 0;
            fScore[start] = _useGoalPosition ? Heuristic(start, goal) : 0;
            openSet.Add((fScore[start], start));

            while (openSet.Count > 0)
            {
                var current = openSet.First().Item2;
                openSet.Remove(openSet.First());

                if (current == goal)
                    return ReconstructPath(cameFrom, current);

                foreach (var neighbor in GetNeighbors(current))
                {
                    double tentativeGScore = gScore[current] + GetTraversalCost(neighbor);

                    if (tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = gScore[neighbor] + (_useGoalPosition ? Heuristic(neighbor, goal) : 0);

                        if (!openSet.Any(x => x.Item2 == neighbor))
                        {
                            openSet.Add((fScore[neighbor], neighbor));
                        }
                    }
                }
            }

            return new List<(int, int)>(); // Путь не найден
        }

        private double Heuristic((int, int) a, (int, int) b)
        {
            return _distanceWeight * Math.Sqrt(Math.Pow(a.Item1 - b.Item1, 2) + Math.Pow(a.Item2 - b.Item2, 2));
        }

        private List<(int, int)> GetNeighbors((int, int) node)
        {
            var directions = new (int, int)[]
            {
                (-1, 0), (1, 0), (0, -1), (0, 1)
            };

            var neighbors = new List<(int, int)>();
            foreach (var dir in directions)
            {
                var newRow = node.Item1 + dir.Item1;
                var newCol = node.Item2 + dir.Item2;

                if (IsInBounds(newRow, newCol) && _maze[newRow, newCol] != 1) // Проверка на препятствие
                {
                    neighbors.Add((newRow, newCol));
                }
            }

            return neighbors;
        }

        private bool IsInBounds(int row, int col)
        {
            return row >= 0 && row < _rows && col >= 0 && col < _cols;
        }

        private double GetTraversalCost((int, int) node)
        {
            return _maze[node.Item1, node.Item2] == 2 ? _spikeCost : 1; // Учитываем стоимость шипов
        }

        private List<(int, int)> ReconstructPath(Dictionary<(int, int), (int, int)> cameFrom, (int, int) current)
        {
            var path = new List<(int, int)> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current);
            }

            path.Reverse();
            return path;
        }
    }
}