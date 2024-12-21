using RobotMaze.ViewModels;
using System.Windows.Input;

namespace RobotMaze.Commands
{
    public class ZoomInCommand(MazeViewModel viewModel) : ICommand
    {
        private readonly MazeViewModel viewModel = viewModel;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.ZoomIn();
        }

        public event EventHandler CanExecuteChanged;
    }
}