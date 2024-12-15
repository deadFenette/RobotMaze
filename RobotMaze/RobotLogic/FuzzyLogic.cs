using System;
using System.Collections.Generic;
using System.Linq;

namespace RobotMaze.RobotLogic
{
    public static class FuzzyLogic
    {
        private const int FieldCheckRadius = 1;
        private const int TotalCheckedCells = 9;
        private const double MaxDistanceWeight = 0.8; // Увеличен вес для расстояния до цели
        private const double ObstacleDensityWeight = 0.7; // Вес для плотности препятствий
        private const double CurrentSpeedWeight = 0.2; // Вес для текущей скорости
        private const double DirectionWeight = 0.4; // Вес для направления
        private const double EnergyLevelWeight = 0.2; // Вес для уровня энергии
        private const double DangerLevelWeight = 0.4; // Вес для уровня опасности
        private const double SafetyLevelWeight = 0.1; // Вес для уровня безопасности
        private const int MemorySize = 10; // Увеличен размер памяти для избегания колебаний
        private const double ConfidenceThreshold = 0.7; // Порог уверенности в принятии решений
        private static bool _useGoalPosition = true; // Флаг для использования позиции цели

        public static void SetUseGoalPosition(bool useGoalPosition)
        {
            _useGoalPosition = useGoalPosition;
        }

        // Вычисляет нечёткое расстояние до цели
        public static double FuzzyDistanceToGoal(int x, int y, (int, int) goal)
        {
            if (!_useGoalPosition) return 0;
            double distance = Math.Sqrt(Math.Pow(x - goal.Item1, 2) + Math.Pow(y - goal.Item2, 2));
            return 1 / (1 + distance);
        }

        // Вычисляет нечёткую плотность препятствий вокруг текущей позиции
        public static double FuzzyObstacleDensity(int x, int y, int[,] field)
        {
            int count = CountObstaclesAround(x, y, field);
            return count / (double)TotalCheckedCells;
        }

        // Проверяет, является ли позиция допустимой
        private static bool IsValidPosition(int x, int y, int[,] field)
        {
            return x >= 0 && x < field.GetLength(0) && y >= 0 && y < field.GetLength(1);
        }

        // Вычисляет нечёткую скорость
        public static double FuzzySpeed(double currentSpeed)
        {
            return currentSpeed switch
            {
                < 1 => 0.1,
                < 3 => 0.5,
                _ => 1.0
            };
        }

        // Вычисляет нечёткое направление
        public static double FuzzyDirection(double currentAngle)
        {
            currentAngle = NormalizeAngle(currentAngle);
            return currentAngle switch
            {
                < 45 or > 315 => 0.9,
                < 135 => 0.5,
                _ => 0.1
            };
        }

        // Вычисляет нечёткий уровень энергии
        public static double FuzzyEnergyLevel(double energy)
        {
            return energy switch
            {
                < 20 => 0.1,
                < 50 => 0.5,
                _ => 1.0
            };
        }

        // Вычисляет нечёткий уровень опасности
        public static double FuzzyDangerLevel(int x, int y, int[,] dangerField)
        {
            int count = CountObstaclesAround(x, y, dangerField);
            return count / (double)TotalCheckedCells;
        }

        // Вычисляет нечёткий уровень безопасности
        public static double FuzzySafetyLevel(int x, int y, int[,] safetyField)
        {
            int count = CountObstaclesAround(x, y, safetyField);
            return 1 - (count / (double)TotalCheckedCells);
        }

        // Вычисляет вес решения на основе нечётких параметров
        public static double CalculateDecisionWeight(double distanceToGoal, double obstacleDensity, double currentSpeed, double direction, double energyLevel, double dangerLevel, double safetyLevel, double confidence)
        {
            double totalWeight = MaxDistanceWeight * distanceToGoal +
                                 ObstacleDensityWeight * obstacleDensity +
                                 CurrentSpeedWeight * currentSpeed +
                                 DirectionWeight * direction +
                                 EnergyLevelWeight * energyLevel +
                                 DangerLevelWeight * dangerLevel +
                                 SafetyLevelWeight * safetyLevel;
            return totalWeight * confidence;
        }

        // Принимает решение на основе нечётких параметров
        public static double FuzzyDecision(double distanceToGoal, double obstacleDensity, double currentSpeed, double direction, double energyLevel, double dangerLevel, double safetyLevel, Queue<(int, int)> memory, HashSet<(int, int)> visited)
        {
            double confidence = 1.0; // Начальное значение уверенности
            double forwardDecision = CalculateDecisionWeight(distanceToGoal, obstacleDensity, currentSpeed, direction, energyLevel, dangerLevel, safetyLevel, confidence);
            double leftDecision = CalculateDecisionWeight(distanceToGoal - 1, obstacleDensity + 0.5, currentSpeed, NormalizeAngle(direction + 90), energyLevel, dangerLevel, safetyLevel, confidence);
            double rightDecision = CalculateDecisionWeight(distanceToGoal - 1, obstacleDensity + 0.5, currentSpeed, NormalizeAngle(direction - 90), energyLevel, dangerLevel, safetyLevel, confidence);

            // Избегание колебаний
            if (memory.Count >= MemorySize && memory.Any(m => m.Item1 == (int)direction && m.Item2 == (int)direction))
            {
                forwardDecision *= 0.5;
            }

            // Избегание повторного посещения клеток
            forwardDecision *= AvoidOscillation((int)direction, (int)direction, visited);
            leftDecision *= AvoidOscillation((int)NormalizeAngle(direction + 90), (int)NormalizeAngle(direction + 90), visited);
            rightDecision *= AvoidOscillation((int)NormalizeAngle(direction - 90), (int)NormalizeAngle(direction - 90), visited);

            // Проверка уверенности в решении
            if (forwardDecision < ConfidenceThreshold && leftDecision < ConfidenceThreshold && rightDecision < ConfidenceThreshold)
            {
                confidence = 0.5; // Уменьшение уверенности
            }

            return Math.Max(forwardDecision, Math.Max(leftDecision, rightDecision));
        }

        // Избегает колебаний, уменьшая вероятность возврата на уже посещённую клетку
        public static double AvoidOscillation(int x, int y, HashSet<(int, int)> visited)
        {
            return visited.Contains((x, y)) ? 0.0 : 1.0;
        }

        // Нормализует угол в диапазоне [0, 360)
        private static double NormalizeAngle(double angle)
        {
            return (angle % 360 + 360) % 360;
        }

        // Считает количество препятствий вокруг текущей позиции
        private static int CountObstaclesAround(int x, int y, int[,] field)
        {
            int count = 0;
            for (int i = -FieldCheckRadius; i <= FieldCheckRadius; i++)
            {
                for (int j = -FieldCheckRadius; j <= FieldCheckRadius; j++)
                {
                    if (IsValidPosition(x + i, y + j, field) && field[x + i, y + j] == 1)
                    {
                        count++;
                    }
                }
            }
            // Учитываем текущую позицию
            if (field[x, y] == 1)
            {
                count++;
            }
            return count;
        }
    }
}
