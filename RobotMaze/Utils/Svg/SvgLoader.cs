using System.Windows.Controls;
using System.Windows.Markup;

namespace RobotMaze.Utils.Svg
{
    public static class SvgLoader
    {
        public static Canvas LoadSvgSpikes()
        {
            string svgSpikes = @"
            <Canvas xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
                <Canvas.Effect>
                    <DropShadowEffect BlurRadius='5' Color='Black' Opacity='0.5'/>
                </Canvas.Effect>
                <Rectangle Width='50' Height='50' Fill='#D3D3D3'>
                    <Rectangle.OpacityMask>
                        <DrawingBrush Viewport='0,0,0.2,0.2' ViewportUnits='Absolute' TileMode='Tile'>
                            <DrawingBrush.Drawing>
                                <GeometryDrawing>
                                    <GeometryDrawing.Geometry>
                                        <GeometryGroup>
                                            <RectangleGeometry Rect='0,0,10,10'/>
                                            <EllipseGeometry Center='5,5' RadiusX='2' RadiusY='2'/>
                                        </GeometryGroup>
                                    </GeometryDrawing.Geometry>
                                    <GeometryDrawing.Brush>
                                        <SolidColorBrush Color='Black'/>
                                    </GeometryDrawing.Brush>
                                </GeometryDrawing>
                            </DrawingBrush.Drawing>
                        </DrawingBrush>
                    </Rectangle.OpacityMask>
                </Rectangle>
                <Path Data='M25,0 L35,20 L15,20 Z' Fill='#3A3A3A'/>
                <Path Data='M15,20 L25,40 L35,20 Z' Fill='#3A3A3A'/>
                <Path Data='M20,10 L30,30 L10,30 Z' Fill='#3A3A3A'/>
                <Path Data='M30,30 L40,10 L20,10 Z' Fill='#3A3A3A'/>
                <Path Data='M25,5 L30,25 L20,25 Z' Fill='#2E2E2E'/>
                <Path Data='M20,25 L25,35 L30,25 Z' Fill='#2E2E2E'/>
                <Path Data='M22,15 L28,25 L18,25 Z' Fill='#2E2E2E'/>
                <Path Data='M28,25 L32,15 L22,15 Z' Fill='#2E2E2E'/>
            </Canvas>";

            return (Canvas)XamlReader.Parse(svgSpikes);
        }

        public static Canvas LoadSvgWall()
        {
            string svgWall = @"
            <Canvas xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
                <Rectangle Width='50' Height='50' Fill='#1E1E1E'/>
                <Rectangle Width='46' Height='46' Fill='#2E2E2E' Canvas.Left='2' Canvas.Top='2'/>
                <Path Data='M2,2 L48,2 L48,48 L2,48 Z' Stroke='#101010' StrokeThickness='1' Fill='Transparent'/>
                <Path Data='M6,6 L44,6 L44,44 L6,44 Z' Stroke='#3A3A3A' StrokeThickness='1' Fill='Transparent'/>
                <Ellipse Width='6' Height='6' Fill='#0F0F0F' Canvas.Left='5' Canvas.Top='5'/>
                <Ellipse Width='6' Height='6' Fill='#0F0F0F' Canvas.Left='39' Canvas.Top='5'/>
                <Ellipse Width='6' Height='6' Fill='#0F0F0F' Canvas.Left='5' Canvas.Top='39'/>
                <Ellipse Width='6' Height='6' Fill='#0F0F0F' Canvas.Left='39' Canvas.Top='39'/>
            </Canvas>";

            return (Canvas)XamlReader.Parse(svgWall);
        }

        public static Canvas LoadSvgStart()
        {
            string svgStart = @"
            <Canvas xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
                <Rectangle Width='50' Height='50' Fill='#FFFFFF'/>
                <Path Data='M10,25 L40,25' Stroke='#FFD700' StrokeThickness='4'/>
                <Path Data='M15,20 L20,30' Stroke='#FFD700' StrokeThickness='2'/>
                <Path Data='M25,20 L30,30' Stroke='#FFD700' StrokeThickness='2'/>
                <Path Data='M35,20 L40,30' Stroke='#FFD700' StrokeThickness='2'/>
                <TextBlock Text='Start' FontSize='12' Foreground='Black' Canvas.Left='15' Canvas.Top='35'/>
            </Canvas>";

            return (Canvas)XamlReader.Parse(svgStart);
        }

        public static Canvas LoadSvgGoal()
        {
            string svgGoal = @"
            <Canvas xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
                <Rectangle Width='50' Height='50' Fill='#FFFFFF'/>
                <Ellipse Width='40' Height='40' Fill='#32CD32' Canvas.Left='5' Canvas.Top='5'/>
                <Path Data='M20,15 L30,35 M30,15 L20,35' Stroke='Black' StrokeThickness='2'/>
                <TextBlock Text='Goal' FontSize='12' Foreground='Black' Canvas.Left='15' Canvas.Top='35'/>
            </Canvas>";

            return (Canvas)XamlReader.Parse(svgGoal);
        }
    }
}