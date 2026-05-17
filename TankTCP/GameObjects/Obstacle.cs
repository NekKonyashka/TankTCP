using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TankTCP.GameObjects;

namespace TankTCP
{
    public class Obstacle : StandartObject
    {
        private const double DEFAULT_OBSTACLE_WIDTH = 125;
        private const double DEFAULT_OBSTACLE_HEIGHT = 125;
        public Obstacle(Point pos) : base(pos)
        {
            _width = DEFAULT_OBSTACLE_WIDTH * ObjectWidthCef;
            _height = DEFAULT_OBSTACLE_HEIGHT * ObjectHeightCef;

            _object = new Rectangle()
            {
                Width = _width,
                Height = _height,
                Fill = new ImageBrush(new BitmapImage(new Uri("./res/KAMEN.png", UriKind.Relative))),
                Stretch = Stretch.Fill,
                
            };
        }

    }
}
