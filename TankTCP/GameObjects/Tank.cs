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
        private const double DEFAULT_TANK_WIDTH = 90;
        private const double DEFAULT_TANK_HEIGHT = 70;

        public bool InMove = false;
        public double? _lastTimeShooting = null;
        private byte _health = 3;
        private double _reloadTime = 5;
        public double ReloadTime => _reloadTime;
        private double _speed = 80;
        private double _rotationSpeed = 1.25;
        public double PrevAngle { get; private set; }
        public byte Health => _health;

        public double BodyOffsetY { get; set; }

        public Point BodyPosition => new Point(Position.X, Position.Y + BodyOffsetY);
        public Point NextBodyPosition => new Point(NextPosition.X, NextPosition.Y + BodyOffsetY);

        public Tank(Point pos, AttachType attachType,double angle, Brush fill) : base(pos, attachType,angle)
        {
            _width = ObjectWidthCef * DEFAULT_TANK_WIDTH;
            _height = ObjectHeightCef * DEFAULT_TANK_HEIGHT;
            _nextPosition = pos;
            PrevAngle = Angle;
            _object.Fill = fill;
            _object.Width = _width;
            _object.Height = _height;
        }

        public void MoveForward(double dt)
        {
            PrevAngle = Angle;
            double translateX = Math.Cos(Angle / 180 * Math.PI);
            double translateY = Math.Sin(Angle / 180 * Math.PI);

            _nextPosition.X -= translateX * _speed * dt;
            _nextPosition.Y -= translateY * _speed * dt;
        }

        public void MoveBackward(double dt)
        {
            PrevAngle = Angle;
            double translateX = Math.Cos(Angle / 180 * Math.PI);
            double translateY = Math.Sin(Angle / 180 * Math.PI);

            _nextPosition.X += translateX * _speed * dt;
            _nextPosition.Y += translateY * _speed * dt;
        }

        public void RotateLeft()
        {
            PrevAngle = Angle;
            _rotateTransform.Angle -= _rotationSpeed;
            if (Angle < 0)
            {
                _rotateTransform.Angle = 360;
            }
        }
        public void RotateRight()
        {
            PrevAngle = Angle;
            _rotateTransform.Angle += _rotationSpeed;
            if (Angle > 360)
            {
                _rotateTransform.Angle = 0;
            }
        }
        public Bullet Shoot(double shootTime)
        {
            _lastTimeShooting = shootTime;
            var bullet_pos = new Point(BodyPosition.X + Width / 2 - 20 / 2, BodyPosition.Y + Height / 2 - 10 / 2);
            var bullet = new Bullet(bullet_pos, AttachType, Angle);

            return bullet;
        }

        public bool CanShoot(double time)
        {
            if (_lastTimeShooting == null) return true;

            return time - _lastTimeShooting > _reloadTime;
        }
        public void Update()
        {
            _position = _nextPosition;
        }

        public override void Apply(GameObjectDto dto)
        {
            _nextPosition = dto.Position;
            _rotateTransform.Angle = dto.Angle;
            _health = dto.Health;
            _lastTimeShooting = dto.LastTimeShooting;
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

        public void ResetAngle()
        {
            _rotateTransform.Angle = PrevAngle;
        }

        public void Hit()
        {
            _health--;
        }

        public Point[] GetEndPoints(double Angle, Point relate)
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
