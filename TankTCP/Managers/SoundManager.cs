using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace TankTCP
{
    public enum SoundAction
    {
        Shoot,
        Destroy,
    }
    public class SoundManager
    {
        private SoundPlayer _player;

        private Dictionary<SoundAction, SoundPlayer> _sounds = new()
        {
            [SoundAction.Shoot] = new SoundPlayer("./res/shoot.wav"),
            [SoundAction.Destroy] = new SoundPlayer("./res/dead.wav")
        };

        public void DoAction(SoundAction action)
        {
            _player = _sounds[action];
            _player.Play();
        }
    }
}
