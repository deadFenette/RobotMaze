using System;
using System.Collections.Generic;

namespace RobotMaze.MazeGeneration
{
    public static class PrimsAlgorithmGenerator
    {
        public static void GeneratePrimsAlgorithm(int[,] field, Random random)
        {
            int width = field.GetLength(0);
            int height = field.GetLength(1);
            bool[,] visited = new bool[width, height];

            // Выбираем случайную начальную точку
            int startX = random.Next(width);
            int startY = random.Next(height);
            visited[startX, startY] = true;
            field[startX, startY] = 0; // Начальная клетка - не стена

            // Инициализируем фронтир (границу)
            List<(int, int)> frontier = new List<(int, int)> { (startX, startY) };

            while (frontier.Count > 0)
            {
                // Выбираем случайную точку из фронтира
                (int x, int y) = frontier[random.Next(frontier.Count)];
                frontier.Remove((x, y));

                // Определяем соседние клетки
                List<(int, int)> neighbors = new List<(int, int)>
                {
                    (x + 2, y),
                    (x - 2, y),
                    (x, y + 2),
                    (x, y - 2)
                };

                foreach (var (nx, ny) in neighbors)
                {
                    // Проверяем, что соседняя клетка в пределах поля и не посещена
                    if (nx >= 0 && nx < width && ny >= 0 && ny < height && !visited[nx, ny])
                    {
                        visited[nx, ny] = true;
                        frontier.Add((nx, ny));
                        // Удаляем стену между текущей и соседней клеткой
                        field[nx, ny] = 0;
                        field[(nx + x) / 2, (ny + y) / 2] = 0;
                    }
                }
            }
        }
    }
}
