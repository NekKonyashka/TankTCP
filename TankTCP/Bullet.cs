using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace TankTCP
{
    public class Bullet
    {
        private double _speed = 10;
        private double _angle;
        private Rectangle _object;
        public Rectangle Object => _object;
        private double _width;
        public double Width => _width;
        private double _height;
        public double Height => _height;

        private Point _position;
        public Point Position => _position;

        private RotateTransform _rotateTransform;

        public Bullet(Point pos,double angle,double width,double height)
        {
            _position = pos;
            _angle = angle;
            _width = width;
            _height = height;

            _rotateTransform = new RotateTransform();
            _rotateTransform.Angle = angle;

            _object = new Rectangle()
            {
                Width = _width,
                Height = _height,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = _rotateTransform,
                Fill = Brushes.DarkOrange,
            };
        }

        public void Move()
        {
            double translateX = Math.Cos(_angle / 180 * Math.PI);
            double translateY = Math.Sin(_angle / 180 * Math.PI);

            _position.X -= translateX * _speed;
            _position.Y -= translateY * _speed;
        }

        public void Update()
        {
            Move();

            Canvas.SetLeft(Object, _position.X);
            Canvas.SetTop(Object, _position.Y);
        }

        public Point[] GetCorners()
        {
            return new[]
            {
                GetRelatedPoint(Position),
                GetRelatedPoint(new Point(Position.X + Width,Position.Y)),
                GetRelatedPoint(new Point(Position.X,Position.Y + Height)),
                GetRelatedPoint(new Point(Position.X + Width,Position.Y + Height))
            };
        }

        private Point GetRelatedPoint(Point current)
        {
            var center_pos = new Point(Position.X + Width / 2, Position.Y + Height / 2);
            var related_pos = new Point(current.X - center_pos.X, current.Y - center_pos.Y);

            double new_x = related_pos.X * Math.Cos(_angle / 180 * Math.PI) - related_pos.Y * Math.Sin(_angle / 180 * Math.PI) + center_pos.X;
            double new_y = related_pos.X * Math.Sin(_angle / 180 * Math.PI) + related_pos.Y * Math.Cos(_angle / 180 * Math.PI) + center_pos.Y;

            return new Point(new_x, new_y);
        }
    }
}
