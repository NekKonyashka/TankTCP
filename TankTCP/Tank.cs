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
        private Point _nextPosition;
        public Point NextPosition => _nextPosition;

        private double _width;
        public double Width => _width;
        private double _height;
        public double Height => _height;

        private RotateTransform _rotateTransform;
        public double Angle => _rotateTransform.Angle;
        public double PrevAngle { get; private set; }

        public Tank(Point pos,double width,double height)
        {
            _position = pos;
            _nextPosition = pos;
            _width = width;
            _height = height;
            _rotateTransform = new RotateTransform();
            _rotateTransform.Angle = 0;
            PrevAngle = 0;

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
            PrevAngle = Angle;
            double translateX = Math.Cos(Angle / 180 * Math.PI);
            double translateY = Math.Sin(Angle / 180 * Math.PI);

            _nextPosition.X -= translateX * _speed;
            _nextPosition.Y -= translateY * _speed;
        }

        public void MoveBackward()
        {
            PrevAngle = Angle;
            double translateX = Math.Cos(Angle / 180 * Math.PI);
            double translateY = Math.Sin(Angle / 180 * Math.PI);

            _nextPosition.X += translateX * _speed;
            _nextPosition.Y += translateY * _speed;
        }

        public void RotateLeft()
        {
            PrevAngle = Angle;
            _rotateTransform.Angle -= _rotationSpeed;
            if(Angle < 0)
            {
                _rotateTransform.Angle = 360;
            }
        }
        public void RotateRight()
        {
            PrevAngle = Angle;
            _rotateTransform.Angle += _rotationSpeed;
            if(Angle > 360)
            {
                _rotateTransform.Angle = 0;
            }
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
            _position = _nextPosition;

            Canvas.SetLeft(Object, _position.X);
            Canvas.SetTop(Object, _position.Y);
        }

        public void ReturnX(double dif_x)
        {
            _nextPosition.X = _position.X;
        }

        public void ReturnY(double dif_y)
        {
            _nextPosition.Y = _position.Y;
        }

        public Point[] GetEndPoints(double Angle,Point relate)
        {
            var center_pos = new Point(relate.X + Width / 2, relate.Y + Height / 2);

            double shift_y = Height / 2 * Math.Cos(Angle / 180 * Math.PI);
            double shift_x = Height / 2 * Math.Sin(Angle / 180 * Math.PI);
            var end_pos_forward = GetRelatedPoint(Angle, center_pos, new Point(relate.X, relate.Y));
            var end_pos_backward = GetRelatedPoint(Angle, center_pos, new Point(relate.X + Width, relate.Y + Height));

            return new[]
            {
                new Point(end_pos_forward.X - shift_x,end_pos_forward.Y + shift_y),
                new Point(end_pos_backward.X + shift_x,end_pos_backward.Y - shift_y)
            };
        }

        public Point[] GetCorners(double Angle, Point relate)
        {
            var center_pos = new Point(relate.X + Width / 2, relate.Y + Height / 2);
            return new[]
            {
                GetRelatedPoint(Angle,center_pos,relate),
                GetRelatedPoint(Angle,center_pos,new Point(relate.X + Width,relate.Y)),
                GetRelatedPoint(Angle, center_pos,new Point(relate.X,relate.Y + Height)),
                GetRelatedPoint(Angle, center_pos,new Point(relate.X + Width,relate.Y + Height))
            };
        }

        private Point GetRelatedPoint(double Angle,Point center_pos,Point current)
        {
            var related_pos = new Point(current.X - center_pos.X, current.Y - center_pos.Y);

            double new_x = related_pos.X * Math.Cos(Angle / 180 * Math.PI) - related_pos.Y * Math.Sin(Angle / 180 * Math.PI) + center_pos.X;
            double new_y = related_pos.X * Math.Sin(Angle / 180 * Math.PI) + related_pos.Y * Math.Cos(Angle / 180 * Math.PI) + center_pos.Y;

            return new Point(new_x, new_y);
        }
    }
}
