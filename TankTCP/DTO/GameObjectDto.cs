using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TankTCP
{
    public enum GameObjectType {
        Tank,
        Bullet,
    }

    public class GameObjectDto
    {
        public Point Position { get; set; }
        public double Angle { get; set; }

        public AttachType AttachType { get; set; }
        public GameObjectType Type { get; set; }

        public int Id { get; set; }
    }
}
