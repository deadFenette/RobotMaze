using System.Windows.Controls;

namespace RobotMaze.Views
{
    public partial class MazeCanvas : UserControl
    {
        public MazeCanvas()
        {
            InitializeComponent();
        }

        // Изменили имя свойства на MazeCanvasElementProperty, чтобы избежать конфликта имен
        public Canvas MazeCanvasElementProperty => MazeCanvasElement;
    }
}
