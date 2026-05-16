using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TankTCP
{
    public class SendedDto
    {
        public List<GameObjectDto> gameObjects { get; set; }
        public double GameTime { get; set; }

        public SendedDto()
        {
            gameObjects = new List<GameObjectDto>();
        }
    }
}
