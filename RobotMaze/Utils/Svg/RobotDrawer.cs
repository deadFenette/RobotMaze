using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RobotMaze.Utils.Svg
{
    public static class RobotDrawer
    {
        public static Canvas LoadSvgRobot(Brush headColor)
        {
            string svgRobot = $@"
            <Canvas xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
                <Ellipse Width='20' Height='20' Fill='{headColor}' Canvas.Left='15' Canvas.Top='5'/>
                <Rectangle Width='30' Height='30' Fill='Gray' Canvas.Left='10' Canvas.Top='20'/>
                <Rectangle Width='10' Height='20' Fill='Gray' Canvas.Left='0' Canvas.Top='25'/>
                <Rectangle Width='10' Height='20' Fill='Gray' Canvas.Left='40' Canvas.Top='25'/>
            </Canvas>";

            return (Canvas)XamlReader.Parse(svgRobot);
        }

        public static Ellipse LoadSvgPathPoint(Brush fillColor)
        {
            string svgPathPoint = $@"
            <Ellipse xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                     Width='10' Height='10' Fill='{fillColor}'/>";

            return (Ellipse)XamlReader.Parse(svgPathPoint);
        }
    }
}
