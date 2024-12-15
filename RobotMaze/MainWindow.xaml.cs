using System.Windows;
using RobotMaze.ViewModels;
using RobotMaze.Views;

namespace RobotMaze
{
    public partial class MainWindow : Window
    {
        private readonly MazeViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();
            viewModel = new MazeViewModel(MazeCanvas);
            DataContext = viewModel;
        }

        private void MazeCanvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            viewModel.HandleMouseLeftButtonDown(e);
        }

        private void MazeCanvas_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                viewModel.ZoomIn();
            }
            else
            {
                viewModel.ZoomOut();
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.StartButtonClick();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ClearButtonClick();
        }

        private void SetStartButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SetStartButtonClick();
        }

        private void SetGoalButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SetGoalButtonClick();
        }

        private void CreateSpikesButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.CreateSpikesButtonClick();
        }

        private void CreateWallsButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.CreateWallsButtonClick();
        }

        private void GenerateMazeButton_Click(object sender, RoutedEventArgs e)
        {
            int width = int.Parse(MazeWidthTextBox.Text);
            int height = int.Parse(MazeHeightTextBox.Text);
            Console.WriteLine($"Button clicked with width: {width}, height: {height}");
            viewModel.GenerateMaze(width, height);
        }



        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ZoomIn();
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ZoomOut();
        }
    }
}
