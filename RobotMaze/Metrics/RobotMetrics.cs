namespace RobotMaze.Metrics
{
    public class RobotMetrics
    {
        public int StepsTaken { get; set; }
        public int EnergyConsumed { get; set; }
        public TimeSpan TimeElapsed { get; set; }
        public int DistanceTraveled { get; set; }

        public RobotMetrics()
        {
            StepsTaken = 0;
            EnergyConsumed = 0;
            TimeElapsed = TimeSpan.Zero;
            DistanceTraveled = 0;
        }

        public void UpdateMetrics(int steps, int energy, TimeSpan time, int distance)
        {
            StepsTaken += steps;
            EnergyConsumed += energy;
            TimeElapsed += time;
            DistanceTraveled += distance;
        }

        public override string ToString()
        {
            return $"{StepsTaken}, {EnergyConsumed}, {TimeElapsed}, {DistanceTraveled}";
        }
    }
}
