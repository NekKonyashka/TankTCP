using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Media;

namespace TankTCP
{
    public class TankView
    {
        private Grid _grid;
        public Grid Grid => _grid;
        private Tank _tank;
        public Tank Tank => _tank;
        private Rectangle _reloadBar;
        public Rectangle ReloadBar => _reloadBar;

        public TankView(Tank tank)
        {
            _tank = tank;
            _reloadBar = new Rectangle()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                MaxWidth = tank.Width * 0.8,
                Height = 6,
                Fill = Brushes.LightGray,
                Margin = new Thickness(0, 0, 0, 20)
            };
            _reloadBar.Width = _reloadBar.MaxWidth;
            _grid = new Grid()
            {
                Width = tank.Width,
                Height = tank.Height + _reloadBar.Margin.Bottom + _reloadBar.Height,
            };

            _tank.Object.VerticalAlignment = VerticalAlignment.Bottom;
            _tank.Object.HorizontalAlignment = HorizontalAlignment.Center;

            _grid.Children.Add(_tank.Object);
            _grid.Children.Add(_reloadBar);

            _tank.BodyOffsetY = _reloadBar.Height + _reloadBar.Margin.Bottom;
        }

        public void Update(double gameTime)
        {
            _tank.Update();
            ReloadUpdate(gameTime);
            Canvas.SetLeft(_grid,_tank.Position.X);
            Canvas.SetTop(_grid, _tank.Position.Y);
        }

        public void ReloadUpdate(double gt)
        {
            if(_tank._lastTimeShooting != null)
            {
                var lt = _tank._lastTimeShooting.Value;
                if(gt >= lt)
                {
                    _reloadBar.Width = (gt - lt) / _tank.ReloadTime * _reloadBar.MaxWidth;
                }
            }
        }
    }
}
