using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TankTCP.GameObjects;

namespace TankTCP
{
    public enum AttachType
    {
        Host,
        Client
    }
    public abstract class GameObject : StandartObject
    {
        protected RotateTransform _rotateTransform;
        public double Angle => _rotateTransform.Angle;
        protected Point _nextPosition;
        public Point NextPosition => _nextPosition;

        private AttachType _attachType;
        public AttachType AttachType => _attachType;

        public GameObject(Point pos,AttachType attachType,double angle) : base(pos)
        {
            _rotateTransform = new RotateTransform();
            _rotateTransform.Angle = angle;

            _object = new Rectangle()
            {
                RenderTransform = _rotateTransform,
                RenderTransformOrigin = new Point(0.5, 0.5),
                Stretch = Stretch.Fill,
                Tag = GameManager.random.Next()
            };

            _attachType = attachType;
        }

        public abstract void Apply(GameObjectDto dto);

        public Point[] GetCorners(double Angle, Point relate)
        {
            var center_pos = GetCenterPos(relate);
            return new[]
            {
                GetRelatedPoint(Angle,center_pos,relate),
                GetRelatedPoint(Angle,center_pos,new Point(relate.X + Width,relate.Y)),
                GetRelatedPoint(Angle, center_pos,new Point(relate.X,relate.Y + Height)),
                GetRelatedPoint(Angle, center_pos,new Point(relate.X + Width,relate.Y + Height))
            };
        }

        public Point GetCenterPos(Point relate)
        {
            return new Point(relate.X + Width / 2, relate.Y + Height / 2);
        }

        public Point GetRelatedPoint(double Angle, Point center_pos, Point current)
        {
            var related_pos = new Point(current.X - center_pos.X, current.Y - center_pos.Y);

            double new_x = related_pos.X * Math.Cos(Angle / 180 * Math.PI) - related_pos.Y * Math.Sin(Angle / 180 * Math.PI) + center_pos.X;
            double new_y = related_pos.X * Math.Sin(Angle / 180 * Math.PI) + related_pos.Y * Math.Cos(Angle / 180 * Math.PI) + center_pos.Y;

            return new Point(new_x, new_y);
        }
    }
}
