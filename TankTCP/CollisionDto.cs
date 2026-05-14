using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace TankTCP
{
    public class CollisionDto
    {
        public double Angle { get; set; }
        public Point CenterPos { get; set; }
        public Rectangle Rectangle { get; set; }

        public CollisionDto(double angle, Point centerPos, Rectangle rectangle)
        {
            Angle = angle;
            CenterPos = centerPos;
            Rectangle = rectangle;
        }

        public void Deconstruct(out double angle,out Point point,out double halfWidth,out double halfHeight)
        {
            angle = Angle;
            point = CenterPos;
            halfWidth = Rectangle.ActualWidth / 2;
            halfHeight = Rectangle.ActualHeight / 2;
        }
    }
}
