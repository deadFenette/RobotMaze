namespace RobotMaze.Models
{
    public class Maze(int m, int n)
    {
        public int[,] Field { get; set; } = new int[m, n];
        public (int, int) Start { get; set; } = (0, 0);
        public (int, int) Goal { get; set; } = (m - 1, n - 1);
        public int M { get; } = m;
        public int N { get; } = n;
    }
}
