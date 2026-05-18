using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TankTCP
{
    public enum SoundAction
    {
        Shoot,
        Destroy,
        Hit
    }
    public class SoundManager
    {
        private MediaPlayer _backgroundPlayer;
        private Dictionary<SoundAction, string> _sounds = new()
        {
            [SoundAction.Shoot] = "./res/shoot.wav",
            [SoundAction.Destroy] = "./res/dead.wav",
            [SoundAction.Hit] = "./res/hit.wav"
        };

        public SoundManager()
        {
            _backgroundPlayer = new MediaPlayer() { Volume = 0.1 };
            _backgroundPlayer.Open(new Uri("./res/BGMusic.mp3", UriKind.Relative));
        }

        public void StartGame()
        {
            _backgroundPlayer.Play();
        }
        public void EndGame()
        {
            _backgroundPlayer.Stop();
        }

        public void DoAction(SoundAction action)
        {
            var _player = new MediaPlayer() { Volume = 0.7 };
            _player.Open(new Uri(_sounds[action],UriKind.Relative));
            _player.Play();
        }
    }
}
