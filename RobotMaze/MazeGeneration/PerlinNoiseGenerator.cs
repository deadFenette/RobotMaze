using System;

namespace RobotMaze.MazeGeneration
{
    public static class PerlinNoiseGenerator
    {
        private static readonly int[] p = {
            151,160,137,91,90,15,131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
            190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,88,237,149,56,87,174,20,125,
            136,171,168, 68,175,74,165,71,134,139,48,27,166,77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,
            55,46,245,40,244,102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,135,130,116,
            188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,5,202,38,147,118,126,255,82,85,212,207,206,
            59,227,47,16,58,17,182,189,28,42,223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
            129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,251,34,242,193,238,210,144,12,191,
            179,162,241, 81,51,145,235,249,14,239,107,49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,
            150,254,138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
        };

        public static void GeneratePerlinNoise(int[,] field, Random random)
        {
            int width = field.GetLength(0);
            int height = field.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Генерируем шум Перлина
                    double noise = PerlinNoise(x / 10.0, y / 10.0, random);
                    // Если значение шума больше 0.5, ставим стену
                    if (noise > 0.5)
                    {
                        field[x, y] = 1; // Стена
                    }
                    else
                    {
                        // Добавляем случайные элементы для усложнения лабиринта
                        if (random.NextDouble() < 0.1)
                        {
                            field[x, y] = 2; // Шипы
                        }
                    }
                }
            }
        }

        private static double PerlinNoise(double x, double y, Random random)
        {
            int X = (int)Math.Floor(x) & 255;
            int Y = (int)Math.Floor(y) & 255;

            x -= Math.Floor(x);
            y -= Math.Floor(y);

            double u = Fade(x);
            double v = Fade(y);

            int aa = p[p[X] + Y];
            int ab = p[p[X] + Y + 1];
            int ba = p[p[X + 1] + Y];
            int bb = p[p[X + 1] + Y + 1];

            return Lerp(v, Lerp(u, Grad(aa, x, y),
                                   Grad(ba, x - 1, y)),
                           Lerp(u, Grad(ab, x, y - 1),
                                   Grad(bb, x - 1, y - 1)));
        }

        private static double Fade(double t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        private static double Lerp(double t, double a, double b)
        {
            return a + t * (b - a);
        }

        private static double Grad(int hash, double x, double y)
        {
            int h = hash & 3;
            double u = h < 2 ? x : -x;
            double v = h < 1 ? y : -y;
            return (h & 1) == 0 ? u : v;
        }
    }
}
