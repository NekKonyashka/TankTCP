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
        private double _speed = 15;
        public Bullet(Point pos, double angle) : base(pos)
        {
            _width = 20;
            _height = 10;
            _rotateTransform.Angle = angle;
            _object.Fill = new ImageBrush(new BitmapImage(new Uri("./res/Bullet.png", UriKind.Relative)));
            _object.Width = _width;
            _object.Height = _height;
        }

        public void Move()
        {
            double translateX = Math.Cos(Angle / 180 * Math.PI);
            double translateY = Math.Sin(Angle / 180 * Math.PI);

            _position.X -= translateX * _speed;
            _position.Y -= translateY * _speed;
        }

        public override void Update()
        {
            Move();
            Canvas.SetLeft(Object, _position.X);
            Canvas.SetTop(Object, _position.Y);
        }

        public void Apply(double angle)
        {
            _rotateTransform.Angle = angle;
        }
    }
}
