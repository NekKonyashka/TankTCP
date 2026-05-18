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
        private List<Rectangle> _healthBars;

        public Rectangle ReloadBar => _reloadBar;
        public List<Rectangle> HealthBars => _healthBars;

        public TankView(Tank tank)
        {
            _tank = tank;
            _reloadBar = new Rectangle()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                MaxWidth = tank.Width,
                Height = 6,
                Fill = Brushes.LightGray,
                Margin = new Thickness(0, 12, 0, 0)
            };
            _reloadBar.Width = _reloadBar.MaxWidth;
            _healthBars = new List<Rectangle>(){
                new Rectangle(){
                    Width = _tank.Width * 0.3,
                    Height = 6,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Fill = Brushes.Red,
                },
                new Rectangle(){
                    Width = _tank.Width * 0.3,
                    Height = 6,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Fill = Brushes.Red,
                },
                new Rectangle(){
                    Width = _tank.Width * 0.3,
                    Height = 6,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Fill = Brushes.Red,
                }
            };
            _grid = new Grid()
            {
                Width = tank.Width,
                Height = tank.Height + _reloadBar.Margin.Top + _reloadBar.Height +
                                       _healthBars[0].Height + _tank.Width * 0.1
            };

            _tank.Object.VerticalAlignment = VerticalAlignment.Bottom;
            _tank.Object.HorizontalAlignment = HorizontalAlignment.Center;

            _grid.Children.Add(_tank.Object);
            _grid.Children.Add(_reloadBar);
            foreach (var b in _healthBars)
            {
                _grid.Children.Add(b);
            }

            _tank.BodyOffsetY = _grid.Height - _tank.Height;
        }

        public void Update(double gameTime)
        {
            _tank.Update();
            ReloadUpdate(gameTime);
            SyncHealthBars();
            Canvas.SetLeft(_grid, _tank.Position.X);
            Canvas.SetTop(_grid, _tank.Position.Y);
        }

        private void ReloadUpdate(double gt)
        {
            if (_tank._lastTimeShooting != null)
            {
                var lt = _tank._lastTimeShooting.Value;
                if (gt >= lt)
                {
                    _reloadBar.Width = (gt - lt) / _tank.ReloadTime * _reloadBar.MaxWidth;
                }
            }
        }

        public bool SyncHealthBars()
        {
            if(_healthBars.Count != _tank.Health)
            {
                var bar = _healthBars[_healthBars.Count - 1];
                _grid.Children.Remove(bar);
                _healthBars.Remove(bar);
                return true;
            }
            return false;
        }
    }
}
