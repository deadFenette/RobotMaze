using System.Windows;
using System.Windows.Input;
using RobotMaze.ViewModels;

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

        private void MazeCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            viewModel.HandleMouseLeftButtonDown(e);
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
    }
}
