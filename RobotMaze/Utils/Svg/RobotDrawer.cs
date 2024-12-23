using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RobotMaze.Utils.Svg
{
    public static class RobotDrawer
    {
        // Метод для загрузки SVG робота
        public static Canvas LoadSvgRobot(Brush headColor, Brush bodyColor, Brush eyeColor, Brush mouthColor, bool addHorns = false, bool addStar = false)
        {
            string horns = addHorns ? @"
                <!-- Horns -->
                <Polygon Points='10,5 15,0 20,5' Fill='Black'/>
                <Polygon Points='30,5 35,0 40,5' Fill='Black'/>" : string.Empty;

            string star = addStar ? @"
                <!-- Star -->
                <Polygon Points='25,35 27,40 32,40 28,42 30,47 25,44 20,47 22,42 18,40 23,40' Fill='Yellow' Opacity='0.5'/>" : string.Empty;

            string svgRobot = $@"
            <Canvas xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
                <!-- Head -->
                <Ellipse Width='30' Height='30' Fill='{headColor}' Canvas.Left='10' Canvas.Top='0'/>
                <!-- Eyes -->
                <Ellipse Width='5' Height='5' Fill='{eyeColor}' Canvas.Left='15' Canvas.Top='10'/>
                <Ellipse Width='5' Height='5' Fill='{eyeColor}' Canvas.Left='25' Canvas.Top='10'/>
                <!-- Mouth -->
                <Rectangle Width='10' Height='2' Fill='{mouthColor}' Canvas.Left='20' Canvas.Top='20'/>
                <!-- Body -->
                <Rectangle Width='40' Height='30' Fill='{bodyColor}' Canvas.Left='5' Canvas.Top='25'/>
                <!-- Arms -->
                <Rectangle Width='8' Height='20' Fill='{bodyColor}' Canvas.Left='0' Canvas.Top='30'/>
                <Rectangle Width='8' Height='20' Fill='{bodyColor}' Canvas.Left='42' Canvas.Top='30'/>
                <!-- Legs -->
                <Rectangle Width='15' Height='20' Fill='{bodyColor}' Canvas.Left='10' Canvas.Top='50'/>
                <Rectangle Width='15' Height='20' Fill='{bodyColor}' Canvas.Left='25' Canvas.Top='50'/>
                {horns}
                {star}
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
