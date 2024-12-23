namespace RobotMaze.Metrics
{
    public class RobotMetrics
    {
        public int StepsTaken { get; set; }
        public int EnergyConsumed { get; set; }
        public TimeSpan TimeElapsed { get; set; }
        public int DistanceTraveled { get; set; }
        public int SpikeCollisions { get; set; }
        public int DirectionChanges { get; set; }
        public TimeSpan AverageTimeBetweenSpikeCollisions { get; set; }

        public RobotMetrics()
        {
            StepsTaken = 0;
            EnergyConsumed = 0;
            TimeElapsed = TimeSpan.Zero;
            DistanceTraveled = 0;
            SpikeCollisions = 0;
            DirectionChanges = 0;
            AverageTimeBetweenSpikeCollisions = TimeSpan.Zero;
        }

        public void UpdateMetrics(int steps, int energy, TimeSpan time, int distance, bool spikeCollision, bool directionChange)
        {
            StepsTaken += steps;
            EnergyConsumed += energy;
            TimeElapsed += time;
            DistanceTraveled += distance;

            if (spikeCollision)
            {
                SpikeCollisions++;
            }

            if (directionChange)
            {
                DirectionChanges++;
            }

            if (SpikeCollisions > 0)
            {
                AverageTimeBetweenSpikeCollisions = TimeElapsed / SpikeCollisions;
            }
        }

        public double CalculateSummaryMetric()
        {
            // Задаём максимальные значения для нормализации
            const int MaxSteps = 100;
            const int MaxEnergy = 100;
            const int MaxSpikeCollisions = 10;
            const int MaxDirectionChanges = 10;
            const double MaxTimeBetweenSpikeCollisions = 10.0; // В секундах

            // Веса для каждой метрики
            const double WeightSteps = 0.4;
            const double WeightEnergy = 0.3;
            const double WeightSpikeCollisions = 0.1;
            const double WeightDirectionChanges = 0.1;
            const double WeightTimeBetweenSpikes = 0.1;

            // Нормализация метрик
            double normalizedSteps = (double)StepsTaken / MaxSteps;
            double normalizedEnergy = (double)EnergyConsumed / MaxEnergy;
            double normalizedSpikeCollisions = (double)SpikeCollisions / MaxSpikeCollisions;
            double normalizedDirectionChanges = (double)DirectionChanges / MaxDirectionChanges;

            double normalizedTimeBetweenSpikes = 0.0;
            if (SpikeCollisions > 0)
            {
                normalizedTimeBetweenSpikes = AverageTimeBetweenSpikeCollisions.TotalSeconds / MaxTimeBetweenSpikeCollisions;
            }

            // Рассчитываем итоговую метрику
            double summaryMetric =
                (WeightSteps * normalizedSteps) +
                (WeightEnergy * normalizedEnergy) +
                (WeightSpikeCollisions * normalizedSpikeCollisions) +
                (WeightDirectionChanges * normalizedDirectionChanges) +
                (WeightTimeBetweenSpikes * normalizedTimeBetweenSpikes);

            return summaryMetric;
        }

        public override string ToString()
        {
            return $"{StepsTaken}, {EnergyConsumed}, {TimeElapsed}, {DistanceTraveled}, {SpikeCollisions}, {DirectionChanges}, {AverageTimeBetweenSpikeCollisions}";
        }
    }
}
