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
        private Canvas? robot = null;
        private readonly List<UIElement> mazeElements = new List<UIElement>();
        private readonly DispatcherTimer timer;
        private List<(int, int)> path = new List<(int, int)>();
        private int currentStep;
        private bool isSettingStart = false;
        private bool isSettingGoal = false;
        private bool isCreatingSpikes = false;
        private bool isCreatingWalls = false;
        private double currentSpeed = 1.0;
        private double currentEnergy = 100.0; // Новый параметр для уровня энергии
        private int[,] dangerField; // Новый параметр для уровня опасности
        private int[,] safetyField; // Новый параметр для уровня безопасности
        private Queue<(int, int)> memory = new Queue<(int, int)>(); // Память для избегания колебаний
        private Maze maze;
        private double scale = 1.0;
        private RobotMetrics robotMetrics = new RobotMetrics();

        public event PropertyChangedEventHandler PropertyChanged;

        public MazeViewModel(MazeCanvas mazeCanvas)
        {
            this.mazeCanvas = mazeCanvas.MazeCanvasElementProperty;
            maze = new Maze(10, 10); // Инициализация лабиринта с размерами по умолчанию
            dangerField = new int[maze.M, maze.N]; // Инициализация уровня опасности
            safetyField = new int[maze.M, maze.N]; // Инициализация уровня безопасности

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

        private List<(int, int)> MoveRobot((int, int) start, (int, int) goal)
        {
            int x = start.Item1;
            int y = start.Item2;
            List<(int, int)> path = new List<(int, int)> { (x, y) };
            HashSet<(int, int)> visited = new HashSet<(int, int)> { (x, y) };
            int maxSteps = maze.M * maze.N;
            int steps = 0;

            while ((x, y) != goal && steps < maxSteps)
            {
                (int, int) nextMove = ChooseNextMove(x, y, goal, visited);
                if (nextMove != (-1, -1))
                {
                    (x, y) = nextMove;
                    path.Add((x, y));
                    visited.Add((x, y));
                    UpdateSpeed(x, y);
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

        private (int, int) ChooseNextMove(int x, int y, (int, int) goal, HashSet<(int, int)> visited)
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
                maze.Field[move.Item1, move.Item2] != 1 && // Робот не может ходить через стены
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
                double speed = FuzzyLogic.FuzzySpeed(currentSpeed);
                double direction = FuzzyLogic.FuzzyDirection(CalculateAngle(x, y, move.Item1, move.Item2));
                double energyLevel = FuzzyLogic.FuzzyEnergyLevel(currentEnergy);
                double dangerLevel = FuzzyLogic.FuzzyDangerLevel(move.Item1, move.Item2, dangerField);
                double safetyLevel = FuzzyLogic.FuzzySafetyLevel(move.Item1, move.Item2, safetyField);
                double oscillationPenalty = FuzzyLogic.AvoidOscillation(move.Item1, move.Item2, visited);

                double score = FuzzyLogic.FuzzyDecision(distance, obstacleDensity, speed, direction, energyLevel, dangerLevel, safetyLevel, memory, visited) * oscillationPenalty;

                if (score > maxScore)
                {
                    maxScore = score;
                    bestMove = move;
                }
            }

            if (bestMove == (-1, -1) || currentEnergy < 50)
            {
                foreach (var move in movesWithSpikes)
                {
                    double distance = FuzzyLogic.FuzzyDistanceToGoal(move.Item1, move.Item2, goal);
                    double obstacleDensity = FuzzyLogic.FuzzyObstacleDensity(move.Item1, move.Item2, maze.Field);
                    double speed = FuzzyLogic.FuzzySpeed(currentSpeed);
                    double direction = FuzzyLogic.FuzzyDirection(CalculateAngle(x, y, move.Item1, move.Item2));
                    double energyLevel = FuzzyLogic.FuzzyEnergyLevel(currentEnergy);
                    double dangerLevel = FuzzyLogic.FuzzyDangerLevel(move.Item1, move.Item2, dangerField);
                    double safetyLevel = FuzzyLogic.FuzzySafetyLevel(move.Item1, move.Item2, safetyField);
                    double oscillationPenalty = FuzzyLogic.AvoidOscillation(move.Item1, move.Item2, visited);

                    double score = FuzzyLogic.FuzzyDecision(distance, obstacleDensity, speed, direction, energyLevel, dangerLevel, safetyLevel, memory, visited) * oscillationPenalty;

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
            // Проверка, чтобы стартовая позиция не была заблокирована клеткой
            if (maze.Field[maze.Start.Item1, maze.Start.Item2] != 0)
            {
                MessageBox.Show("Start position is blocked by a cell!");
                return;
            }

            // Проверка, чтобы финишная позиция не была заблокирована клеткой
            if (maze.Field[maze.Goal.Item1, maze.Goal.Item2] != 0)
            {
                MessageBox.Show("Goal position is blocked by a cell!");
                return;
            }

            path = MoveRobot(maze.Start, maze.Goal);
            currentStep = 0;
            timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (currentStep < path.Count)
            {
                AddPathPoint(path[currentStep]);
                DrawRobot(path[currentStep]);
                UpdateEnergy(path[currentStep]);
                UpdateMetrics(path[currentStep]);
                currentStep++;
            }
            else
            {
                timer.Stop();
            }
        }

        private void AddPathPoint((int, int) position)
        {
            Ellipse ellipse = new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = Brushes.Red
            };
            Canvas.SetLeft(ellipse, position.Item2 * cellSize + 22.5);
            Canvas.SetTop(ellipse, position.Item1 * cellSize + 22.5);
            mazeCanvas.Children.Add(ellipse);
            mazeElements.Add(ellipse);
        }

        private void DrawRobot((int, int) position)
        {
            if (robot != null)
            {
                mazeCanvas.Children.Remove(robot);
            }

            robot = new Canvas
            {
                Width = 50,
                Height = 50
            };

            Ellipse head = new Ellipse
            {
                Width = 20,
                Height = 20,
                Fill = Brushes.Red
            };
            Canvas.SetLeft(head, 15);
            Canvas.SetTop(head, 5);
            robot.Children.Add(head);

            Rectangle body = new Rectangle
            {
                Width = 30,
                Height = 30,
                Fill = Brushes.Gray
            };
            Canvas.SetLeft(body, 10);
            Canvas.SetTop(body, 20);
            robot.Children.Add(body);

            Rectangle leftArm = new Rectangle
            {
                Width = 10,
                Height = 20,
                Fill = Brushes.Gray
            };
            Canvas.SetLeft(leftArm, 0);
            Canvas.SetTop(leftArm, 25);
            robot.Children.Add(leftArm);

            Rectangle rightArm = new Rectangle
            {
                Width = 10,
                Height = 20,
                Fill = Brushes.Gray
            };
            Canvas.SetLeft(rightArm, 40);
            Canvas.SetTop(rightArm, 25);
            robot.Children.Add(rightArm);

            Canvas.SetLeft(robot, position.Item2 * cellSize);
            Canvas.SetTop(robot, position.Item1 * cellSize);
            mazeCanvas.Children.Add(robot);
        }

        public void ClearButtonClick()
        {
            maze.Field = new int[maze.M, maze.N];
            dangerField = new int[maze.M, maze.N];
            safetyField = new int[maze.M, maze.N];
            DrawMaze();
            DrawStartAndGoal();
            timer.Stop();
            currentStep = 0;
            path.Clear();
            if (robot != null)
            {
                mazeCanvas.Children.Remove(robot);
                robot = null;
            }
            currentEnergy = 100.0;
            robotMetrics = new RobotMetrics();
            OnPropertyChanged(nameof(CurrentEnergy));
            OnPropertyChanged(nameof(RobotMetrics));
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

        private void UpdateSpeed(int x, int y)
        {
            double distanceToGoal = FuzzyLogic.FuzzyDistanceToGoal(x, y, maze.Goal);
            double obstacleDensity = FuzzyLogic.FuzzyObstacleDensity(x, y, maze.Field);
            double energyLevel = FuzzyLogic.FuzzyEnergyLevel(currentEnergy);
            double dangerLevel = FuzzyLogic.FuzzyDangerLevel(x, y, dangerField);
            double safetyLevel = FuzzyLogic.FuzzySafetyLevel(x, y, safetyField);

            if (distanceToGoal > 0.5 && obstacleDensity < 0.5 && energyLevel > 0.5 && dangerLevel < 0.5 && safetyLevel > 0.5)
            {
                currentSpeed = Math.Min(currentSpeed + 0.1, 3.0);
            }
            else
            {
                currentSpeed = Math.Max(currentSpeed - 0.1, 0.1);
            }
        }

        private void UpdateEnergy((int, int) position)
        {
            int energyConsumed = maze.Field[position.Item1, position.Item2] == 2 ? 50 : 1;
            currentEnergy -= energyConsumed;

            if (currentEnergy <= 0)
            {
                timer.Stop();
                MessageBox.Show("Robot is out of energy!");
            }

            OnPropertyChanged(nameof(CurrentEnergy));
        }

        private void UpdateMetrics((int, int) position)
        {
            int steps = 1;
            int energyConsumed = maze.Field[position.Item1, position.Item2] == 2 ? 50 : 1;
            TimeSpan timeElapsed = TimeSpan.FromMilliseconds(500);
            int distanceTraveled = 1;

            robotMetrics.UpdateMetrics(steps, energyConsumed, timeElapsed, distanceTraveled);
            OnPropertyChanged(nameof(RobotMetrics));
        }

        public double CurrentEnergy
        {
            get { return currentEnergy; }
            set
            {
                if (currentEnergy != value)
                {
                    currentEnergy = value;
                    OnPropertyChanged(nameof(CurrentEnergy));
                }
            }
        }

        public RobotMetrics RobotMetrics
        {
            get { return robotMetrics; }
            set
            {
                if (robotMetrics != value)
                {
                    robotMetrics = value;
                    OnPropertyChanged(nameof(RobotMetrics));
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

            // Инициализируем все клетки как стены
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    maze.Field[i, j] = 1;
                }
            }

            // Генерация основной структуры лабиринта с помощью алгоритма Прима
            PrimsAlgorithmGenerator.GeneratePrimsAlgorithm(maze.Field, random);

            // Добавление случайных препятствий с помощью шума Перлина
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