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
    public class Tank : GameObject
    {
        public bool InMove = false;
        private double? _lastTimeShooting = null;
        private byte _health = 3;
        private double _reloadTime = 2;
        private double _speed = 1.5;
        private double _rotationSpeed = 1.25;
        public double PrevAngle { get; private set; }

        public Tank(Point pos, AttachType attachType, Brush fill) : base(pos, attachType)
        {
            _width = 60;
            _height = 50;
            _nextPosition = pos;
            PrevAngle = Angle;
            _object.Fill = fill;
            _object.Width = _width;
            _object.Height = _height;
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
            var bullet = new Bullet(bullet_pos, AttachType, Angle);

            return bullet;
        }

        public bool CanShoot(double time)
        {
            if (_lastTimeShooting == null) return true;

            return time - _lastTimeShooting >= _reloadTime;
        }
        public override void Update()
        {
            _position = _nextPosition;

            Canvas.SetLeft(Object, _position.X);
            Canvas.SetTop(Object, _position.Y);
        }

        public void Apply(GameObjectDto dto)
        {
            _nextPosition = dto.Position;
            _rotateTransform.Angle = dto.Angle;
        }

        public void ReturnX()
        {
            _nextPosition.X = _position.X;
        }
        public void MoveBackX(double dif_x)
        {
            _nextPosition.X += dif_x;
        }
        public void MoveBackY(double dif_y)
        {
            _nextPosition.Y += dif_y;
        }

        public void ReturnY()
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
    }
}
