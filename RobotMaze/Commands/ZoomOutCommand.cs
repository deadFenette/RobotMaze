using RobotMaze.ViewModels;
using System.Windows.Input;

namespace RobotMaze.Commands
{
    public class ZoomOutCommand : ICommand
    {
        private readonly MazeViewModel viewModel;

        public ZoomOutCommand(MazeViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.ZoomOut();
        }

        public event EventHandler CanExecuteChanged;
    }
}