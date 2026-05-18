using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TankTCP
{
    public enum DtoType 
    {
        None,
        TankDestroy,
    }

    public class SendedDto
    {
        public List<GameObjectDto> gameObjects { get; set; }
        public double GameTime { get; set; }
        public string UserName { get; set; }

        public DtoType DtoType { get; set; }

        public SendedDto()
        {
            gameObjects = new List<GameObjectDto>();
        }
    }
}
