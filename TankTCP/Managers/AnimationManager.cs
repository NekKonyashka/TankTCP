using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TankTCP
{
    public class AnimationManager
    {
        private int _frameIndex = 0;
        private DateTime _lastFrame;
        private BitmapImage _sprite;
        private Image _currentFrame;
        private double _frameWidth;
        private double _frameHeight;
        private TaskCompletionSource<bool> _taskCompletionSource;
        public TaskCompletionSource<bool> TaskCompletionSource => _taskCompletionSource;
        
        public event Action<Image,Point> OnAnimationStart;
        public event Action<Image> OnAnimationEnd;

        public AnimationManager()
        {
            _sprite = new BitmapImage(new Uri("./res/ExplosionSprite.png",UriKind.Relative));
            _frameWidth = _sprite.Width / 2;
            _frameHeight = _sprite.Height / 2;

            _currentFrame = new Image()
            {
                Width = 200,
                Height = 200,
                Stretch = Stretch.Fill,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
        }

        public void Start(Point pos)
        {
            CompositionTarget.Rendering += Animation;
            _taskCompletionSource = new TaskCompletionSource<bool>();
            OnAnimationStart?.Invoke(_currentFrame,pos);
        }

        private void Animation(object? sender,EventArgs e)
        {
            if((DateTime.Now - _lastFrame).TotalMilliseconds > 250)
            {
                if(_frameIndex == 4)
                {
                    Stop();
                }

                int x = (int)((_frameIndex % 2 == 0 ? 0 : 1) * _frameWidth);
                int y = (int)((_frameIndex / 2) * _frameHeight);

                CroppedBitmap cropped = new CroppedBitmap(_sprite,
                        new Int32Rect(x, y, (int)_frameWidth, (int)_frameHeight));

                _currentFrame.Source = cropped;

                _lastFrame = DateTime.Now;
                _frameIndex++;
            } 
        }

        private void Stop()
        {
            CompositionTarget.Rendering -= Animation;
            _taskCompletionSource.SetResult(true);
            _frameIndex = 0;
            OnAnimationEnd?.Invoke(_currentFrame);
        }

    }

}
