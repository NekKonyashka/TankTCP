using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace TankTCP
{
    public class Bullet : GameObject
    {
        private const double DEFAULT_BULLET_WIDTH = 25;
        private const double DEFAULT_BULLET_HEIGHT = 15;

        private static int counter = 0;
        private double _speed = 1600;
        private int _id;
        public int Id => _id;
        public Bullet(Point pos, AttachType attachType, double angle) : base(pos, attachType, angle)
        {
            _id = counter++;
            _width = ObjectWidthCef * DEFAULT_BULLET_WIDTH;
            _height = ObjectHeightCef * DEFAULT_BULLET_HEIGHT;
            _object.Fill = new ImageBrush(new BitmapImage(new Uri("./res/Bullet.png", UriKind.Relative)));
            _object.Width = _width;
            _object.Height = _height;
        }

        public void Move(double dt)
        {
            double translateX = Math.Cos(Angle / 180 * Math.PI);
            double translateY = Math.Sin(Angle / 180 * Math.PI);

            _position.X -= translateX * _speed * dt;
            _position.Y -= translateY * _speed * dt;
        }

        public void Update(double dt)
        {
            Move(dt);
            Canvas.SetLeft(Object, _position.X);
            Canvas.SetTop(Object, _position.Y);
        }

        public override void Apply(GameObjectDto dto)
        {
            _rotateTransform.Angle = dto.Angle;
            _position = dto.Position;
        }
    }
}
