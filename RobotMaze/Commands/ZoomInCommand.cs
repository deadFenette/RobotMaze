using RobotMaze.ViewModels;
using System.Windows.Input;

namespace RobotMaze.Commands
{
    public class ZoomInCommand : ICommand
    {
        private readonly MazeViewModel viewModel;

        public ZoomInCommand(MazeViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

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