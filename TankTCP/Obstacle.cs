using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TankTCP
{
    public class Obstacle
    {
        private Rectangle _object;
        public Rectangle Object => _object;
        private Point _position;
        public Point Position => _position;

        private double _width;
        public double Width => _width;
        private double _height;
        public double Height => _height;

        public Obstacle(Point pos,double width,double height)
        {
            _position = pos;
            _width = width;
            _height = height;

            _object = new Rectangle()
            {
                Width = _width,
                Height = _height,
                Fill = Brushes.Gray,
            };
        }

    }
}
