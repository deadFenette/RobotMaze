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
using System.Text.Json;
using System.IO;
using System.Diagnostics;

namespace RobotMaze.ViewModels
{
    public class MazeViewModel : INotifyPropertyChanged
    {
        private readonly Canvas mazeCanvas;
        private readonly int cellSize = 50;
        private readonly Canvas[] robots = new Canvas[3]; // Массив для хранения роботов
        private readonly List<UIElement> mazeElements = new List<UIElement>();
        private readonly DispatcherTimer timer;
        private readonly DispatcherTimer recalculateTimer; // Таймер для задержки пересчета путей
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

            recalculateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1) // Задержка в 1 секунду перед пересчетом путей
            };
            recalculateTimer.Tick += RecalculateTimer_Tick;
        }

        // Метод для рисования лабиринта
        private void DrawMaze()
        {
            // Очистка текущих элементов лабиринта
            foreach (var element in mazeElements)
            {
                mazeCanvas.Children.Remove(element);
            }
            mazeElements.Clear();

            // Рисование каждой клетки лабиринта
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

            // Перерисовка роботов и стартовых позиций
            DrawRobots();
            DrawStartAndGoal();
        }

        // Метод для рисования стартовой и целевой точек
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

        // Метод для рисования роботов
        private void DrawRobots()
        {
            for (int i = 0; i < robots.Length; i++)
            {
                if (robots[i] != null)
                {
                    mazeCanvas.Children.Remove(robots[i]);
                }

                Brush headColor = i switch
                {
                    0 => Brushes.Red,
                    1 => Brushes.Blue,
                    _ => Brushes.Green
                };
                Brush bodyColor = i switch
                {
                    0 => Brushes.LightGray,
                    1 => Brushes.DarkGray,
                    _ => Brushes.Gray
                };
                Brush eyeColor = Brushes.Black;
                Brush mouthColor = Brushes.Black;

                // Add horns to the third robot and star to the second robot
                bool addHorns = i == 2;
                bool addStar = i == 1;

                robots[i] = RobotDrawer.LoadSvgRobot(headColor, bodyColor, eyeColor, mouthColor, addHorns, addStar);

                Canvas.SetLeft(robots[i], GetRobotPosition(i).Item2 * cellSize);
                Canvas.SetTop(robots[i], GetRobotPosition(i).Item1 * cellSize);
                mazeCanvas.Children.Add(robots[i]);
            }
        }

        // Метод для получения текущей позиции робота
        private (int, int) GetRobotPosition(int robotIndex)
        {
            return robotIndex switch
            {
                0 => currentStep1 < path1.Count ? path1[currentStep1] : maze.Start,
                1 => currentStep2 < path2.Count ? path2[currentStep2] : maze.Start,
                _ => currentStep3 < path3.Count ? path3[currentStep3] : maze.Start
            };
        }

        // Метод для движения робота
        private List<(int, int)> MoveRobot((int, int) start, (int, int) goal, int robotIndex)
        {
            int x = start.Item1;
            int y = start.Item2;
            List<(int, int)> path = new List<(int, int)> { (x, y) };
            HashSet<(int, int)> visited = new HashSet<(int, int)> { (x, y) };
            int maxSteps = maze.M * maze.N;
            int steps = 0;

            while ((x, y) != goal && steps < maxSteps)
            {
                (int, int) nextMove = ChooseNextMove(x, y, goal, visited, robotIndex);
                if (nextMove != (-1, -1))
                {
                    (x, y) = nextMove;
                    path.Add((x, y));
                    visited.Add((x, y));
                    UpdateSpeed(x, y, robotIndex);
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

                // Введение стохастических факторов
                IntroduceStochasticFactors();
            }

            return path;
        }

        // Метод для выбора следующего хода робота
        private (int, int) ChooseNextMove(int x, int y, (int, int) goal, HashSet<(int, int)> visited, int robotIndex)
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
                double speed = FuzzyLogic.FuzzySpeed(robotIndex switch
                {
                    0 => currentSpeed1,
                    1 => currentSpeed2,
                    _ => currentSpeed3
                });
                double direction = FuzzyLogic.FuzzyDirection(CalculateAngle(x, y, move.Item1, move.Item2));
                double energyLevel = FuzzyLogic.FuzzyEnergyLevel(robotIndex switch
                {
                    0 => currentEnergy1,
                    1 => currentEnergy2,
                    _ => currentEnergy3
                });
                double dangerLevel = FuzzyLogic.FuzzyDangerLevel(move.Item1, move.Item2, dangerField);
                double safetyLevel = FuzzyLogic.FuzzySafetyLevel(move.Item1, move.Item2, safetyField);
                double oscillationPenalty = FuzzyLogic.AvoidOscillation(move.Item1, move.Item2, visited);

                double score = FuzzyLogic.FuzzyDecision(distance, obstacleDensity, speed, direction, energyLevel, dangerLevel, safetyLevel, robotIndex switch
                {
                    0 => memory1,
                    1 => memory2,
                    _ => memory3
                }, visited) * oscillationPenalty;

                if (score > maxScore)
                {
                    maxScore = score;
                    bestMove = move;
                }
            }

            if (bestMove == (-1, -1) || (robotIndex switch
            {
                0 => currentEnergy1,
                1 => currentEnergy2,
                _ => currentEnergy3
            }) < 50)
            {
                foreach (var move in movesWithSpikes)
                {
                    double distance = FuzzyLogic.FuzzyDistanceToGoal(move.Item1, move.Item2, goal);
                    double obstacleDensity = FuzzyLogic.FuzzyObstacleDensity(move.Item1, move.Item2, maze.Field);
                    double speed = FuzzyLogic.FuzzySpeed(robotIndex switch
                    {
                        0 => currentSpeed1,
                        1 => currentSpeed2,
                        _ => currentSpeed3
                    });
                    double direction = FuzzyLogic.FuzzyDirection(CalculateAngle(x, y, move.Item1, move.Item2));
                    double energyLevel = FuzzyLogic.FuzzyEnergyLevel(robotIndex switch
                    {
                        0 => currentEnergy1,
                        1 => currentEnergy2,
                        _ => currentEnergy3
                    });
                    double dangerLevel = FuzzyLogic.FuzzyDangerLevel(move.Item1, move.Item2, dangerField);
                    double safetyLevel = FuzzyLogic.FuzzySafetyLevel(move.Item1, move.Item2, safetyField);
                    double oscillationPenalty = FuzzyLogic.AvoidOscillation(move.Item1, move.Item2, visited);

                    double score = FuzzyLogic.FuzzyDecision(distance, obstacleDensity, speed, direction, energyLevel, dangerLevel, safetyLevel, robotIndex switch
                    {
                        0 => memory1,
                        1 => memory2,
                        _ => memory3
                    }, visited) * oscillationPenalty;

                    if (score > maxScore)
                    {
                        maxScore = score;
                        bestMove = move;
                    }
                }
            }

            return bestMove;
        }

        // Метод для расчета угла между двумя точками
        private double CalculateAngle(int x1, int y1, int x2, int y2)
        {
            double angle = Math.Atan2(y2 - y1, x2 - x1) * 180 / Math.PI;
            if (angle < 0) angle += 360;
            return angle;
        }

        // Метод для обработки нажатия мыши
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
                recalculateTimer.Start(); // Запуск таймера для задержки пересчета путей
            }
        }

        // Метод для запуска роботов
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

            AStarLogic aStar = new AStarLogic(maze.Field, 5);
            aStar.SetUseGoalPosition(useGoalPositionAStar);
            path1 = MoveRobot(maze.Start, maze.Goal, 0);
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

        // Метод для обработки тика таймера
        private void Timer_Tick(object? sender, EventArgs e)
        {
            // Движение роботов по шагам
            MoveRobotsStepByStep();

            // Введение стохастических факторов в реальном времени
            IntroduceStochasticFactors();

            // Проверка, все ли роботы остановились
            if (robot1Stopped && robot2Stopped && robot3Stopped)
            {
                timer.Stop();
            }
        }

        // Метод для обработки тика таймера пересчета путей
        private void RecalculateTimer_Tick(object? sender, EventArgs e)
        {
            recalculateTimer.Stop();
            RecalculatePaths();
        }

        // Метод для движения роботов по шагам
      
        private void MoveRobotsStepByStep()
        {
            if (!robot1Stopped && currentStep1 < path1.Count)
            {
                AddPathPoint(path1[currentStep1], 0);
                DrawRobot(path1[currentStep1], 0);
                UpdateEnergy(path1[currentStep1], 0);
                UpdateMetrics(path1[currentStep1], 0);
                currentStep1++;

                // Проверка, достиг ли робот цели
                if (path1[currentStep1 - 1] == maze.Goal)
                {
                    robot1Stopped = true;
                    currentStep1 = path1.Count; // Устанавливаем текущий шаг на конец пути, чтобы робот не двигался дальше
                }
            }

            if (!robot2Stopped && currentStep2 < path2.Count)
            {
                AddPathPoint(path2[currentStep2], 1);
                DrawRobot(path2[currentStep2], 1);
                UpdateEnergy(path2[currentStep2], 1);
                UpdateMetrics(path2[currentStep2], 1);
                currentStep2++;

                // Проверка, достиг ли робот цели
                if (path2[currentStep2 - 1] == maze.Goal)
                {
                    robot2Stopped = true;
                    currentStep2 = path2.Count; // Устанавливаем текущий шаг на конец пути, чтобы робот не двигался дальше
                }
            }

            if (!robot3Stopped && currentStep3 < path3.Count)
            {
                AddPathPoint(path3[currentStep3], 2);
                DrawRobot(path3[currentStep3], 2);
                UpdateEnergy(path3[currentStep3], 2);
                UpdateMetrics(path3[currentStep3], 2);
                currentStep3++;

                // Проверка, достиг ли робот цели
                if (path3[currentStep3 - 1] == maze.Goal)
                {
                    robot3Stopped = true;
                    currentStep3 = path3.Count; // Устанавливаем текущий шаг на конец пути, чтобы робот не двигался дальше
                }
            }
        }


        // Метод для рисования робота
        private void DrawRobot((int, int) position, int robotIndex)
        {
            if (robots[robotIndex] != null)
            {
                mazeCanvas.Children.Remove(robots[robotIndex]);
            }

            Brush headColor = robotIndex switch
            {
                0 => Brushes.Red,
                1 => Brushes.Blue,
                _ => Brushes.Green
            };
            Brush bodyColor = robotIndex switch
            {
                0 => Brushes.LightGray,
                1 => Brushes.DarkGray,
                _ => Brushes.Gray
            };
            Brush eyeColor = Brushes.Black;
            Brush mouthColor = Brushes.Black;

            // Add horns to the third robot and star to the second robot
            bool addHorns = robotIndex == 2;
            bool addStar = robotIndex == 1;

            robots[robotIndex] = RobotDrawer.LoadSvgRobot(headColor, bodyColor, eyeColor, mouthColor, addHorns, addStar);

            Canvas.SetLeft(robots[robotIndex], position.Item2 * cellSize);
            Canvas.SetTop(robots[robotIndex], position.Item1 * cellSize);
            mazeCanvas.Children.Add(robots[robotIndex]);
        }

        // Метод для добавления точки пути робота
        private void AddPathPoint((int, int) position, int robotIndex)
        {
            Brush fillColor = robotIndex switch
            {
                0 => Brushes.Red,
                1 => Brushes.Blue,
                _ => Brushes.Green
            };
            Ellipse ellipse = RobotDrawer.LoadSvgPathPoint(fillColor);
            Canvas.SetLeft(ellipse, position.Item2 * cellSize + 22.5);
            Canvas.SetTop(ellipse, position.Item1 * cellSize + 22.5);
            mazeCanvas.Children.Add(ellipse);
            mazeElements.Add(ellipse);
        }

        // Метод для очистки лабиринта
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
            for (int i = 0; i < robots.Length; i++)
            {
                if (robots[i] != null)
                {
                    mazeCanvas.Children.Remove(robots[i]);
                    robots[i] = null;
                }
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

        // Метод для установки стартовой точки
        public void SetStartButtonClick()
        {
            isSettingStart = true;
            isSettingGoal = false;
            isCreatingSpikes = false;
            isCreatingWalls = false;
        }

        // Метод для установки целевой точки
        public void SetGoalButtonClick()
        {
            isSettingStart = false;
            isSettingGoal = true;
            isCreatingSpikes = false;
            isCreatingWalls = false;
        }

        // Метод для создания шипов
        public void CreateSpikesButtonClick()
        {
            isSettingStart = false;
            isSettingGoal = false;
            isCreatingSpikes = true;
            isCreatingWalls = false;
        }

        // Метод для создания стен
        public void CreateWallsButtonClick()
        {
            isSettingStart = false;
            isSettingGoal = false;
            isCreatingSpikes = false;
            isCreatingWalls = true;
        }

        // Метод для обновления скорости робота
        private void UpdateSpeed(int x, int y, int robotIndex)
        {
            double distanceToGoal = FuzzyLogic.FuzzyDistanceToGoal(x, y, maze.Goal);
            double obstacleDensity = FuzzyLogic.FuzzyObstacleDensity(x, y, maze.Field);
            double energyLevel = FuzzyLogic.FuzzyEnergyLevel(robotIndex switch
            {
                0 => currentEnergy1,
                1 => currentEnergy2,
                _ => currentEnergy3
            });
            double dangerLevel = FuzzyLogic.FuzzyDangerLevel(x, y, dangerField);
            double safetyLevel = FuzzyLogic.FuzzySafetyLevel(x, y, safetyField);

            if (distanceToGoal > 0.5 && obstacleDensity < 0.5 && energyLevel > 0.5 && dangerLevel < 0.5 && safetyLevel > 0.5)
            {
                switch (robotIndex)
                {
                    case 0:
                        currentSpeed1 = Math.Min(currentSpeed1 + 0.1, 3.0);
                        break;
                    case 1:
                        currentSpeed2 = Math.Min(currentSpeed2 + 0.1, 3.0);
                        break;
                    default:
                        currentSpeed3 = Math.Min(currentSpeed3 + 0.1, 3.0);
                        break;
                }
            }
            else
            {
                switch (robotIndex)
                {
                    case 0:
                        currentSpeed1 = Math.Max(currentSpeed1 - 0.1, 0.1);
                        break;
                    case 1:
                        currentSpeed2 = Math.Max(currentSpeed2 - 0.1, 0.1);
                        break;
                    default:
                        currentSpeed3 = Math.Max(currentSpeed3 - 0.1, 0.1);
                        break;
                }
            }
        }

        // Метод для обновления энергии робота
        private void UpdateEnergy((int, int) position, int robotIndex)
        {
            int energyConsumed = maze.Field[position.Item1, position.Item2] == 2 ? 5 : 1;
            switch (robotIndex)
            {
                case 0:
                    currentEnergy1 -= energyConsumed;
                    if (currentEnergy1 <= 0)
                    {
                        robot1Stopped = true;
                        MessageBox.Show("Robot 1 is out of energy!");
                    }
                    OnPropertyChanged(nameof(CurrentEnergy1));
                    break;
                case 1:
                    currentEnergy2 -= energyConsumed;
                    if (currentEnergy2 <= 0)
                    {
                        robot2Stopped = true;
                        MessageBox.Show("Robot 2 is out of energy!");
                    }
                    OnPropertyChanged(nameof(CurrentEnergy2));
                    break;
                default:
                    currentEnergy3 -= energyConsumed;
                    if (currentEnergy3 <= 0)
                    {
                        robot3Stopped = true;
                        MessageBox.Show("Robot 3 is out of energy!");
                    }
                    OnPropertyChanged(nameof(CurrentEnergy3));
                    break;
            }
        }

        private (int, int)? previousDirection = null;

        private bool HasDirectionChanged((int, int) currentPosition, (int, int) previousPosition)
        {
            if (previousDirection == null)
            {
                previousDirection = (currentPosition.Item1 - previousPosition.Item1, currentPosition.Item2 - previousPosition.Item2);
                return false;
            }

            (int, int) currentDirection = (currentPosition.Item1 - previousPosition.Item1, currentPosition.Item2 - previousPosition.Item2);
            bool directionChanged = currentDirection != previousDirection;
            previousDirection = currentDirection;
            return directionChanged;
        }


        // Метод для обновления метрик робота
        private void UpdateMetrics((int, int) position, int robotIndex)
        {
            int steps = 1;
            int energyConsumed = maze.Field[position.Item1, position.Item2] == 2 ? 5 : 1;
            TimeSpan timeElapsed = TimeSpan.FromMilliseconds(500);
            int distanceTraveled = 1;
            bool spikeCollision = maze.Field[position.Item1, position.Item2] == 2;
            bool directionChange = HasDirectionChanged(position, GetRobotPosition(robotIndex));

            switch (robotIndex)
            {
                case 0:
                    robotMetrics1.UpdateMetrics(steps, energyConsumed, timeElapsed, distanceTraveled, spikeCollision, directionChange);
                    OnPropertyChanged(nameof(RobotMetrics1));
                    break;
                case 1:
                    robotMetrics2.UpdateMetrics(steps, energyConsumed, timeElapsed, distanceTraveled, spikeCollision, directionChange);
                    OnPropertyChanged(nameof(RobotMetrics2));
                    break;
                default:
                    robotMetrics3.UpdateMetrics(steps, energyConsumed, timeElapsed, distanceTraveled, spikeCollision, directionChange);
                    OnPropertyChanged(nameof(RobotMetrics3));
                    break;
            }
        }


        // Свойство для текущей энергии робота 1
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

        // Свойство для текущей энергии робота 2
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

        // Свойство для текущей энергии робота 3
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

        // Свойство для метрик робота 1
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

        // Свойство для метрик робота 2
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

        // Свойство для метрик робота 3
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

        // Метод для уведомления об изменении свойства
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Метод для генерации лабиринта
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

        // Метод для увеличения масштаба
        public void ZoomIn()
        {
            scale += 0.1;
            ApplyScaleTransform();
        }

        // Метод для уменьшения масштаба
        public void ZoomOut()
        {
            scale = Math.Max(0.1, scale - 0.1);
            ApplyScaleTransform();
        }

        // Метод для применения масштабирования
        private void ApplyScaleTransform()
        {
            ScaleTransform scaleTransform = new ScaleTransform(scale, scale);
            mazeCanvas.RenderTransform = scaleTransform;
        }

        // Метод для введения стохастических факторов в реальном времени
        private void IntroduceStochasticFactors()
        {
            Random random = new Random();
            int width = maze.Field.GetLength(0);
            int height = maze.Field.GetLength(1);

            // Введение случайных шипов
            if (random.NextDouble() < 0.02) // N% вероятность появления шипа
            {
                int x, y;
                do
                {
                    x = random.Next(width);
                    y = random.Next(height);
                } while (maze.Field[x, y] != 0 || (x == maze.Start.Item1 && y == maze.Start.Item2));

                maze.Field[x, y] = 2;
                dangerField[x, y] = 1;
                safetyField[x, y] = 0;
                DrawMaze();
                recalculateTimer.Start(); // Запуск таймера для задержки пересчета путей
            }
        }

        // Метод для пересчета путей роботов
        private void RecalculatePaths()
        {
            var currentPositions = new List<(int, int)>
            {
                GetRobotPosition(0),
                GetRobotPosition(1),
                GetRobotPosition(2)
            };

            AStarLogic aStar = new AStarLogic(maze.Field, 5);
            aStar.SetUseGoalPosition(useGoalPositionAStar);
            path1 = MoveRobot(currentPositions[0], maze.Goal, 0);
            path2 = aStar.FindPath(currentPositions[1], maze.Goal);

            FuzzyLogic.SetUseGoalPosition(useGoalPositionFuzzy);

            ACOLogic aco = new ACOLogic(maze.Field, 0.1, 1.0, 100, 10);
            aco.SetUseGoalPosition(useGoalPositionACO);
            path3 = aco.FindPath(currentPositions[2], maze.Goal);

            currentStep1 = 0;
            currentStep2 = 0;
            currentStep3 = 0;
            robot1Stopped = false;
            robot2Stopped = false;
            robot3Stopped = false;
        }

        public void SaveMaze(string filePath)
        {
            try
            {
                // Логирование данных перед сериализацией
                Debug.WriteLine("Saving maze to: " + filePath);
                Debug.WriteLine("Maze Field: " + maze.Field);
                Debug.WriteLine("Maze Start: " + maze.Start);
                Debug.WriteLine("Maze Goal: " + maze.Goal);
                Debug.WriteLine("Danger Field: " + dangerField);
                Debug.WriteLine("Safety Field: " + safetyField);

                var mazeData = new
                {
                    Field = ConvertToJaggedArray(maze.Field),
                    Start = maze.Start,
                    Goal = maze.Goal,
                    DangerField = ConvertToJaggedArray(dangerField),
                    SafetyField = ConvertToJaggedArray(safetyField)
                };

                string json = JsonSerializer.Serialize(mazeData, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                // Логирование JSON перед записью в файл
                Debug.WriteLine("Serialized JSON: " + json);

                File.WriteAllText(filePath, json);
                MessageBox.Show("Maze saved successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while saving the maze: {ex.Message}");
                Debug.WriteLine($"Exception: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }

        public void LoadMaze(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                var mazeData = JsonSerializer.Deserialize<MazeData>(json);

                if (mazeData != null)
                {
                    maze.Field = ConvertToMultidimensionalArray(mazeData.Field);
                    maze.Start = mazeData.Start;
                    maze.Goal = mazeData.Goal;
                    dangerField = ConvertToMultidimensionalArray(mazeData.DangerField);
                    safetyField = ConvertToMultidimensionalArray(mazeData.SafetyField);

                    DrawMaze();
                    DrawStartAndGoal();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading the maze: {ex.Message}");
                Debug.WriteLine($"Exception: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }

        private int[][] ConvertToJaggedArray(int[,] array)
        {
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);
            int[][] jaggedArray = new int[rows][];

            for (int i = 0; i < rows; i++)
            {
                jaggedArray[i] = new int[cols];
                for (int j = 0; j < cols; j++)
                {
                    jaggedArray[i][j] = array[i, j];
                }
            }

            return jaggedArray;
        }

        private int[,] ConvertToMultidimensionalArray(int[][] jaggedArray)
        {
            int rows = jaggedArray.Length;
            int cols = jaggedArray[0].Length;
            int[,] array = new int[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    array[i, j] = jaggedArray[i][j];
                }
            }

            return array;
        }

        private class MazeData
        {
            public required int[][] Field { get; set; }
            public (int, int) Start { get; set; }
            public (int, int) Goal { get; set; }
            public required int[][] DangerField { get; set; }
            public required int[][] SafetyField { get; set; }
        }
    }
}
