using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Media;

namespace TankTCP.Managers
{
    public class TankView
    {
        private Grid _grid;
        private Rectangle _tank;
        private Rectangle _reloadBar;

        public TankView(Tank tank)
        {
            _tank = tank.Object;
            _reloadBar = new Rectangle()
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                Width = tank.Width * 0.8,
                Height = 6,
                Fill = Brushes.LightGray,
            };
            _grid = new Grid()
            {
                Width = tank.Width,
                Height = tank.Height + _reloadBar.Height,
            };

            _grid.Children.Add(_tank);
            _grid.Children.Add(_reloadBar);
        }
    }
}
