using RobotMaze.Metrics;
using System;
using System.Globalization;
using System.Windows.Data;

namespace RobotMaze.Converters
{
    public class RobotMetricsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is RobotMetrics metrics)
            {
                return $"Steps: {metrics.StepsTaken}, Energy: {metrics.EnergyConsumed}, Time: {metrics.TimeElapsed}, Distance: {metrics.DistanceTraveled}";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}