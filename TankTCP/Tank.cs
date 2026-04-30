using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TankTCP
{
    public class Tank
    {
        public bool InMove = false;
        private double? _lastTimeShooting = null;
        private double _reloadTime = 1;
        private double _speed = 3.5;
        private double _rotationSpeed = 3;
        private Rectangle _object;
        public Rectangle Object => _object;
        private Point _position;
        public Point Position => _position;

        private double _width;
        public double Width => _width;
        private double _height;
        public double Height => _height;

        private RotateTransform _rotateTransform;
        public double Angle => _rotateTransform.Angle;

        public Tank(Point pos,double width,double height)
        {
            _position = pos;
            _width = width;
            _height = height;
            _rotateTransform = new RotateTransform();
            _rotateTransform.Angle = 0;

            _object = new Rectangle()
            {
                Width = width,
                Height = height,
                RenderTransform = _rotateTransform,
                RenderTransformOrigin = new Point(0.5,0.5),
                //Fill = Brushes.Red,
                Fill = new ImageBrush(new BitmapImage(new Uri("./res/RedTank.png", UriKind.Relative))),
                Stretch = Stretch.Fill,
            };

        }

        public void MoveForward()
        {
            double translateX = Math.Cos(Angle / 180 * Math.PI);
            double translateY = Math.Sin(Angle / 180 * Math.PI);

            _position.X -= translateX * _speed;
            _position.Y -= translateY * _speed;
        }

        public void MoveBackward()
        {
            double translateX = Math.Cos(Angle / 180 * Math.PI);
            double translateY = Math.Sin(Angle / 180 * Math.PI);

            _position.X += translateX * _speed;
            _position.Y += translateY * _speed;
        }

        public void RotateLeft()
        {
            _rotateTransform.Angle -= _rotationSpeed;
            GetCorners();
        }
        public void RotateRight()
        {
            _rotateTransform.Angle += _rotationSpeed;
        }
        public Bullet Shoot(double shootTime)
        {
            _lastTimeShooting = shootTime;
            var bullet_pos = new Point(Position.X + Width / 2 - 20 / 2, Position.Y + Height / 2 - 10 / 2);
            var bullet = new Bullet(bullet_pos, Angle, 20, 10);

            return bullet;
        }

        public bool CanShoot(double time)
        {
            if (_lastTimeShooting == null) return true;

            return time - _lastTimeShooting >= _reloadTime;
        }
        public void Update()
        {
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

            double new_x = related_pos.X * Math.Cos(Angle / 180 * Math.PI) - related_pos.Y * Math.Sin(Angle / 180 * Math.PI) + center_pos.X;
            double new_y = related_pos.X * Math.Sin(Angle / 180 * Math.PI) + related_pos.Y * Math.Cos(Angle / 180 * Math.PI) + center_pos.Y;

            return new Point(new_x, new_y);
        }
    }
}
