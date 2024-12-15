using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using RobotMaze.Models;
using RobotMaze.RobotLogic;
using RobotMaze.Utils.Svg;
using RobotMaze.Views;
using RobotMaze.MazeGeneration;
using RobotMaze.Commands;
using RobotMaze.Metrics;

namespace RobotMaze.ViewModels
{
    public class MazeViewModel : INotifyPropertyChanged
    {
        private readonly Canvas mazeCanvas;
        private readonly int cellSize = 50;
        private Canvas? robot1 = null;
        private Canvas? robot2 = null;
        private Canvas? robot3 = null; // Новый робот
        private readonly List<UIElement> mazeElements = new List<UIElement>();
        private readonly DispatcherTimer timer;
        private List<(int, int)> path1 = new List<(int, int)>();
        private List<(int, int)> path2 = new List<(int, int)>();
        private List<(int, int)> path3 = new List<(int, int)>(); // Путь для нового робота
        private int currentStep1;
        private int currentStep2;
        private int currentStep3; // Текущий шаг для нового робота
        private bool isSettingStart = false;
        private bool isSettingGoal = false;
        private bool isCreatingSpikes = false;
        private bool isCreatingWalls = false;
        private double currentSpeed1 = 1.0;
        private double currentSpeed2 = 1.0;
        private double currentSpeed3 = 1.0; // Скорость для нового робота
        private double currentEnergy1 = 100.0;
        private double currentEnergy2 = 100.0;
        private double currentEnergy3 = 100.0; // Энергия для нового робота
        private int[,] dangerField;
        private int[,] safetyField;
        private Queue<(int, int)> memory1 = new Queue<(int, int)>();
        private Queue<(int, int)> memory2 = new Queue<(int, int)>();
        private Queue<(int, int)> memory3 = new Queue<(int, int)>(); // Память для нового робота
        private Maze maze;
        private double scale = 1.0;
        private RobotMetrics robotMetrics1 = new RobotMetrics();
        private RobotMetrics robotMetrics2 = new RobotMetrics();
        private RobotMetrics robotMetrics3 = new RobotMetrics(); // Метрики для нового робота
        private bool robot1Stopped = false;
        private bool robot2Stopped = false;
        private bool robot3Stopped = false; // Флаг для нового робота
        public bool useGoalPositionAStar = true;
        public bool useGoalPositionFuzzy = true;
        public bool useGoalPositionACO = true; // Флаг для использования позиции цели в ACO

        public event PropertyChangedEventHandler PropertyChanged;

        public MazeViewModel(MazeCanvas mazeCanvas)
        {
            this.mazeCanvas = mazeCanvas.MazeCanvasElementProperty;
            maze = new Maze(10, 10);
            dangerField = new int[maze.M, maze.N];
            safetyField = new int[maze.M, maze.N];

            DrawMaze();
            DrawStartAndGoal();

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            timer.Tick += Timer_Tick;
        }

        private void DrawMaze()
        {
            foreach (var element in mazeElements)
            {
                mazeCanvas.Children.Remove(element);
            }
            mazeElements.Clear();

            for (int i = 0; i < maze.Field.GetLength(0); i++)
            {
                for (int j = 0; j < maze.Field.GetLength(1); j++)
                {
                    Canvas svgElement;
                    if (maze.Field[i, j] == 2)
                    {
                        svgElement = SvgLoader.LoadSvgSpikes();
                        Console.WriteLine($"Spike at cell ({i}, {j})");
                    }
                    else if (maze.Field[i, j] == 1)
                    {
                        svgElement = SvgLoader.LoadSvgWall();
                    }
                    else
                    {
                        Rectangle rect = new Rectangle
                        {
                            Width = cellSize,
                            Height = cellSize,
                            Fill = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                            Stroke = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                            StrokeThickness = 1
                        };
                        Canvas.SetLeft(rect, j * cellSize);
                        Canvas.SetTop(rect, i * cellSize);
                        mazeCanvas.Children.Add(rect);
                        mazeElements.Add(rect);
                        continue;
                    }

                    svgElement.Width = cellSize;
                    svgElement.Height = cellSize;
                    Canvas.SetLeft(svgElement, j * cellSize);
                    Canvas.SetTop(svgElement, i * cellSize);
                    mazeCanvas.Children.Add(svgElement);
                    mazeElements.Add(svgElement);
                }
            }
        }

        private void DrawStartAndGoal()
        {
            var startElement = SvgLoader.LoadSvgStart();
            startElement.Width = cellSize;
            startElement.Height = cellSize;
            Canvas.SetLeft(startElement, maze.Start.Item2 * cellSize);
            Canvas.SetTop(startElement, maze.Start.Item1 * cellSize);
            mazeCanvas.Children.Add(startElement);
            mazeElements.Add(startElement);

            var goalElement = SvgLoader.LoadSvgGoal();
            goalElement.Width = cellSize;
            goalElement.Height = cellSize;
            Canvas.SetLeft(goalElement, maze.Goal.Item2 * cellSize);
            Canvas.SetTop(goalElement, maze.Goal.Item1 * cellSize);
            mazeCanvas.Children.Add(goalElement);
            mazeElements.Add(goalElement);
        }

        private List<(int, int)> MoveRobot((int, int) start, (int, int) goal, bool isRobot1)
        {
            int x = start.Item1;
            int y = start.Item2;
            List<(int, int)> path = new List<(int, int)> { (x, y) };
            HashSet<(int, int)> visited = new HashSet<(int, int)> { (x, y) };
            int maxSteps = maze.M * maze.N;
            int steps = 0;

            while ((x, y) != goal && steps < maxSteps)
            {
                (int, int) nextMove = ChooseNextMove(x, y, goal, visited, isRobot1);
                if (nextMove != (-1, -1))
                {
                    (x, y) = nextMove;
                    path.Add((x, y));
                    visited.Add((x, y));
                    UpdateSpeed(x, y, isRobot1);
                }
                else
                {
                    if (path.Count > 1)
                    {
                        path.RemoveAt(path.Count - 1);
                        (x, y) = path[path.Count - 1];
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return path;
        }

        private (int, int) ChooseNextMove(int x, int y, (int, int) goal, HashSet<(int, int)> visited, bool isRobot1)
        {
            List<(int, int)> possibleMoves = new List<(int, int)>
            {
                (x, y + 1),
                (x + 1, y),
                (x, y - 1),
                (x - 1, y)
            };

            possibleMoves = possibleMoves.FindAll(move =>
                move.Item1 >= 0 && move.Item1 < maze.Field.GetLength(0) &&
                move.Item2 >= 0 && move.Item2 < maze.Field.GetLength(1) &&
                maze.Field[move.Item1, move.Item2] != 1 &&
                !visited.Contains(move));

            if (possibleMoves.Count == 0)
            {
                return (-1, -1);
            }

            List<(int, int)> movesWithoutSpikes = possibleMoves.FindAll(move => maze.Field[move.Item1, move.Item2] != 2);
            List<(int, int)> movesWithSpikes = possibleMoves.FindAll(move => maze.Field[move.Item1, move.Item2] == 2);

            (int, int) bestMove = (-1, -1);
            double maxScore = double.MinValue;

            foreach (var move in movesWithoutSpikes)
            {
                double distance = FuzzyLogic.FuzzyDistanceToGoal(move.Item1, move.Item2, goal);
                double obstacleDensity = FuzzyLogic.FuzzyObstacleDensity(move.Item1, move.Item2, maze.Field);
                double speed = FuzzyLogic.FuzzySpeed(isRobot1 ? currentSpeed1 : currentSpeed2);
                double direction = FuzzyLogic.FuzzyDirection(CalculateAngle(x, y, move.Item1, move.Item2));
                double energyLevel = FuzzyLogic.FuzzyEnergyLevel(isRobot1 ? currentEnergy1 : currentEnergy2);
                double dangerLevel = FuzzyLogic.FuzzyDangerLevel(move.Item1, move.Item2, dangerField);
                double safetyLevel = FuzzyLogic.FuzzySafetyLevel(move.Item1, move.Item2, safetyField);
                double oscillationPenalty = FuzzyLogic.AvoidOscillation(move.Item1, move.Item2, visited);

                double score = FuzzyLogic.FuzzyDecision(distance, obstacleDensity, speed, direction, energyLevel, dangerLevel, safetyLevel, isRobot1 ? memory1 : memory2, visited) * oscillationPenalty;

                if (score > maxScore)
                {
                    maxScore = score;
                    bestMove = move;
                }
            }

            if (bestMove == (-1, -1) || (isRobot1 ? currentEnergy1 : currentEnergy2) < 50)
            {
                foreach (var move in movesWithSpikes)
                {
                    double distance = FuzzyLogic.FuzzyDistanceToGoal(move.Item1, move.Item2, goal);
                    double obstacleDensity = FuzzyLogic.FuzzyObstacleDensity(move.Item1, move.Item2, maze.Field);
                    double speed = FuzzyLogic.FuzzySpeed(isRobot1 ? currentSpeed1 : currentSpeed2);
                    double direction = FuzzyLogic.FuzzyDirection(CalculateAngle(x, y, move.Item1, move.Item2));
                    double energyLevel = FuzzyLogic.FuzzyEnergyLevel(isRobot1 ? currentEnergy1 : currentEnergy2);
                    double dangerLevel = FuzzyLogic.FuzzyDangerLevel(move.Item1, move.Item2, dangerField);
                    double safetyLevel = FuzzyLogic.FuzzySafetyLevel(move.Item1, move.Item2, safetyField);
                    double oscillationPenalty = FuzzyLogic.AvoidOscillation(move.Item1, move.Item2, visited);

                    double score = FuzzyLogic.FuzzyDecision(distance, obstacleDensity, speed, direction, energyLevel, dangerLevel, safetyLevel, isRobot1 ? memory1 : memory2, visited) * oscillationPenalty;

                    if (score > maxScore)
                    {
                        maxScore = score;
                        bestMove = move;
                    }
                }
            }

            return bestMove;
        }

        private double CalculateAngle(int x1, int y1, int x2, int y2)
        {
            double angle = Math.Atan2(y2 - y1, x2 - x1) * 180 / Math.PI;
            if (angle < 0) angle += 360;
            return angle;
        }

        public void HandleMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(mazeCanvas);
            int x = (int)(position.Y / cellSize);
            int y = (int)(position.X / cellSize);

            if (x >= 0 && x < maze.M && y >= 0 && y < maze.N)
            {
                if (isSettingStart)
                {
                    maze.Start = (x, y);
                    isSettingStart = false;
                }
                else if (isSettingGoal)
                {
                    maze.Goal = (x, y);
                    isSettingGoal = false;
                }
                else if (isCreatingSpikes)
                {
                    maze.Field[x, y] = maze.Field[x, y] == 2 ? 0 : 2;
                    dangerField[x, y] = maze.Field[x, y] == 2 ? 1 : 0;
                    safetyField[x, y] = maze.Field[x, y] == 2 ? 0 : 1;
                }
                else if (isCreatingWalls)
                {
                    maze.Field[x, y] = maze.Field[x, y] == 1 ? 0 : 1;
                }
                else
                {
                    maze.Field[x, y] = maze.Field[x, y] == 1 ? 0 : 1;
                }
                DrawMaze();
                DrawStartAndGoal();
            }
        }

        public void StartButtonClick()
        {
            if (maze.Field[maze.Start.Item1, maze.Start.Item2] != 0)
            {
                MessageBox.Show("Start position is blocked by a cell!");
                return;
            }

            if (maze.Field[maze.Goal.Item1, maze.Goal.Item2] != 0)
            {
                MessageBox.Show("Goal position is blocked by a cell!");
                return;
            }

            AStarLogic aStar = new AStarLogic(maze.Field, 50);
            aStar.SetUseGoalPosition(useGoalPositionAStar);
            path1 = MoveRobot(maze.Start, maze.Goal, true);
            path2 = aStar.FindPath(maze.Start, maze.Goal);

            FuzzyLogic.SetUseGoalPosition(useGoalPositionFuzzy);

            ACOLogic aco = new ACOLogic(maze.Field, 0.1, 1.0, 100, 10);
            aco.SetUseGoalPosition(useGoalPositionACO);
            path3 = aco.FindPath(maze.Start, maze.Goal);

            currentStep1 = 0;
            currentStep2 = 0;
            currentStep3 = 0;
            robot1Stopped = false;
            robot2Stopped = false;
            robot3Stopped = false;
            timer.Start();
        }


        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (!robot1Stopped && currentStep1 < path1.Count)
            {
                AddPathPoint(path1[currentStep1], true, false);
                DrawRobot(path1[currentStep1], true, false);
                UpdateEnergy(path1[currentStep1], true);
                UpdateMetrics(path1[currentStep1], true);
                currentStep1++;
            }

            if (!robot2Stopped && currentStep2 < path2.Count)
            {
                AddPathPoint(path2[currentStep2], false, true);
                DrawRobot(path2[currentStep2], false, true);
                UpdateEnergy(path2[currentStep2], false);
                UpdateMetrics(path2[currentStep2], false);
                currentStep2++;
            }

            if (!robot3Stopped && currentStep3 < path3.Count)
            {
                AddPathPoint(path3[currentStep3], false, false);
                DrawRobot(path3[currentStep3], false, false);
                UpdateEnergy(path3[currentStep3], false);
                UpdateMetrics(path3[currentStep3], false);
                currentStep3++;
            }

            if (robot1Stopped && robot2Stopped && robot3Stopped)
            {
                timer.Stop();
            }
        }


        private void AddPathPoint((int, int) position, bool isRobot1, bool isRobot2)
        {
            Brush fillColor = isRobot1 ? Brushes.Red : isRobot2 ? Brushes.Blue : Brushes.Green;
            Ellipse ellipse = RobotDrawer.LoadSvgPathPoint(fillColor);
            Canvas.SetLeft(ellipse, position.Item2 * cellSize + 22.5);
            Canvas.SetTop(ellipse, position.Item1 * cellSize + 22.5);
            mazeCanvas.Children.Add(ellipse);
            mazeElements.Add(ellipse);
        }

        private void DrawRobot((int, int) position, bool isRobot1, bool isRobot2)
        {
            Canvas? robot = isRobot1 ? robot1 : isRobot2 ? robot2 : robot3;
            if (robot != null)
            {
                mazeCanvas.Children.Remove(robot);
            }

            Brush headColor = isRobot1 ? Brushes.Red : isRobot2 ? Brushes.Blue : Brushes.Green;
            robot = RobotDrawer.LoadSvgRobot(headColor);

            Canvas.SetLeft(robot, position.Item2 * cellSize);
            Canvas.SetTop(robot, position.Item1 * cellSize);
            mazeCanvas.Children.Add(robot);

            if (isRobot1)
            {
                robot1 = robot;
            }
            else if (isRobot2)
            {
                robot2 = robot;
            }
            else
            {
                robot3 = robot;
            }
        }

        public void ClearButtonClick()
        {
            maze.Field = new int[maze.M, maze.N];
            dangerField = new int[maze.M, maze.N];
            safetyField = new int[maze.M, maze.N];
            DrawMaze();
            DrawStartAndGoal();
            timer.Stop();
            currentStep1 = 0;
            currentStep2 = 0;
            currentStep3 = 0;
            path1.Clear();
            path2.Clear();
            path3.Clear();
            if (robot1 != null)
            {
                mazeCanvas.Children.Remove(robot1);
                robot1 = null;
            }
            if (robot2 != null)
            {
                mazeCanvas.Children.Remove(robot2);
                robot2 = null;
            }
            if (robot3 != null)
            {
                mazeCanvas.Children.Remove(robot3);
                robot3 = null;
            }
            currentEnergy1 = 100.0;
            currentEnergy2 = 100.0;
            currentEnergy3 = 100.0;
            robotMetrics1 = new RobotMetrics();
            robotMetrics2 = new RobotMetrics();
            robotMetrics3 = new RobotMetrics();
            robot1Stopped = false;
            robot2Stopped = false;
            robot3Stopped = false;
            OnPropertyChanged(nameof(CurrentEnergy1));
            OnPropertyChanged(nameof(CurrentEnergy2));
            OnPropertyChanged(nameof(CurrentEnergy3));
            OnPropertyChanged(nameof(RobotMetrics1));
            OnPropertyChanged(nameof(RobotMetrics2));
            OnPropertyChanged(nameof(RobotMetrics3));
        }

        public void SetStartButtonClick()
        {
            isSettingStart = true;
            isSettingGoal = false;
            isCreatingSpikes = false;
            isCreatingWalls = false;
        }

        public void SetGoalButtonClick()
        {
            isSettingStart = false;
            isSettingGoal = true;
            isCreatingSpikes = false;
            isCreatingWalls = false;
        }

        public void CreateSpikesButtonClick()
        {
            isSettingStart = false;
            isSettingGoal = false;
            isCreatingSpikes = true;
            isCreatingWalls = false;
        }

        public void CreateWallsButtonClick()
        {
            isSettingStart = false;
            isSettingGoal = false;
            isCreatingSpikes = false;
            isCreatingWalls = true;
        }

        private void UpdateSpeed(int x, int y, bool isRobot1)
        {
            double distanceToGoal = FuzzyLogic.FuzzyDistanceToGoal(x, y, maze.Goal);
            double obstacleDensity = FuzzyLogic.FuzzyObstacleDensity(x, y, maze.Field);
            double energyLevel = FuzzyLogic.FuzzyEnergyLevel(isRobot1 ? currentEnergy1 : isRobot1 ? currentEnergy2 : currentEnergy3);
            double dangerLevel = FuzzyLogic.FuzzyDangerLevel(x, y, dangerField);
            double safetyLevel = FuzzyLogic.FuzzySafetyLevel(x, y, safetyField);

            if (distanceToGoal > 0.5 && obstacleDensity < 0.5 && energyLevel > 0.5 && dangerLevel < 0.5 && safetyLevel > 0.5)
            {
                if (isRobot1)
                {
                    currentSpeed1 = Math.Min(currentSpeed1 + 0.1, 3.0);
                }
                else if (isRobot1)
                {
                    currentSpeed2 = Math.Min(currentSpeed2 + 0.1, 3.0);
                }
                else
                {
                    currentSpeed3 = Math.Min(currentSpeed3 + 0.1, 3.0);
                }
            }
            else
            {
                if (isRobot1)
                {
                    currentSpeed1 = Math.Max(currentSpeed1 - 0.1, 0.1);
                }
                else if (isRobot1)
                {
                    currentSpeed2 = Math.Max(currentSpeed2 - 0.1, 0.1);
                }
                else
                {
                    currentSpeed3 = Math.Max(currentSpeed3 - 0.1, 0.1);
                }
            }
        }

        private void UpdateEnergy((int, int) position, bool isRobot1)
        {
            int energyConsumed = maze.Field[position.Item1, position.Item2] == 2 ? 50 : 1;
            if (isRobot1)
            {
                currentEnergy1 -= energyConsumed;
                if (currentEnergy1 <= 0)
                {
                    robot1Stopped = true;
                    MessageBox.Show("Robot 1 is out of energy!");
                }
                OnPropertyChanged(nameof(CurrentEnergy1));
            }
            else if (isRobot1)
            {
                currentEnergy2 -= energyConsumed;
                if (currentEnergy2 <= 0)
                {
                    robot2Stopped = true;
                    MessageBox.Show("Robot 2 is out of energy!");
                }
                OnPropertyChanged(nameof(CurrentEnergy2));
            }
            else
            {
                currentEnergy3 -= energyConsumed;
                if (currentEnergy3 <= 0)
                {
                    robot3Stopped = true;
                    MessageBox.Show("Robot 3 is out of energy!");
                }
                OnPropertyChanged(nameof(CurrentEnergy3));
            }
        }

        private void UpdateMetrics((int, int) position, bool isRobot1)
        {
            int steps = 1;
            int energyConsumed = maze.Field[position.Item1, position.Item2] == 2 ? 50 : 1;
            TimeSpan timeElapsed = TimeSpan.FromMilliseconds(500);
            int distanceTraveled = 1;

            if (isRobot1)
            {
                robotMetrics1.UpdateMetrics(steps, energyConsumed, timeElapsed, distanceTraveled);
                OnPropertyChanged(nameof(RobotMetrics1));
            }
            else if (isRobot1)
            {
                robotMetrics2.UpdateMetrics(steps, energyConsumed, timeElapsed, distanceTraveled);
                OnPropertyChanged(nameof(RobotMetrics2));
            }
            else
            {
                robotMetrics3.UpdateMetrics(steps, energyConsumed, timeElapsed, distanceTraveled);
                OnPropertyChanged(nameof(RobotMetrics3));
            }
        }

        public double CurrentEnergy1
        {
            get { return currentEnergy1; }
            set
            {
                if (currentEnergy1 != value)
                {
                    currentEnergy1 = value;
                    OnPropertyChanged(nameof(CurrentEnergy1));
                }
            }
        }

        public double CurrentEnergy2
        {
            get { return currentEnergy2; }
            set
            {
                if (currentEnergy2 != value)
                {
                    currentEnergy2 = value;
                    OnPropertyChanged(nameof(CurrentEnergy2));
                }
            }
        }

        public double CurrentEnergy3
        {
            get { return currentEnergy3; }
            set
            {
                if (currentEnergy3 != value)
                {
                    currentEnergy3 = value;
                    OnPropertyChanged(nameof(CurrentEnergy3));
                }
            }
        }

        public RobotMetrics RobotMetrics1
        {
            get { return robotMetrics1; }
            set
            {
                if (robotMetrics1 != value)
                {
                    robotMetrics1 = value;
                    OnPropertyChanged(nameof(RobotMetrics1));
                }
            }
        }

        public RobotMetrics RobotMetrics2
        {
            get { return robotMetrics2; }
            set
            {
                if (robotMetrics2 != value)
                {
                    robotMetrics2 = value;
                    OnPropertyChanged(nameof(RobotMetrics2));
                }
            }
        }

        public RobotMetrics RobotMetrics3
        {
            get { return robotMetrics3; }
            set
            {
                if (robotMetrics3 != value)
                {
                    robotMetrics3 = value;
                    OnPropertyChanged(nameof(RobotMetrics3));
                }
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void GenerateMaze(int width, int height)
        {
            maze = new Maze(width, height);
            dangerField = new int[width, height];
            safetyField = new int[width, height];

            Random random = new Random();

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    maze.Field[i, j] = 1;
                }
            }

            PrimsAlgorithmGenerator.GeneratePrimsAlgorithm(maze.Field, random);

            PerlinNoiseGenerator.GeneratePerlinNoise(maze.Field, random);

            DrawMaze();
            DrawStartAndGoal();
        }

        public void ZoomIn()
        {
            scale += 0.1;
            ApplyScaleTransform();
        }

        public void ZoomOut()
        {
            scale = Math.Max(0.1, scale - 0.1);
            ApplyScaleTransform();
        }

        private void ApplyScaleTransform()
        {
            ScaleTransform scaleTransform = new ScaleTransform(scale, scale);
            mazeCanvas.RenderTransform = scaleTransform;
        }
    }
}
