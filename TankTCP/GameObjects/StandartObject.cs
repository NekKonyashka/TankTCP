using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace TankTCP.GameObjects
{
    public abstract class StandartObject
    {
        protected Rectangle _object;
        public Rectangle Object => _object;
        protected Point _position;
        public Point Position => _position;

        protected double _width;
        public double Width => _width;
        protected double _height;
        public double Height => _height;

        public StandartObject(Point pos)
        {
            _position = pos;
        }
    }
}
