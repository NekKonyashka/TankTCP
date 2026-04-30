using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TankTCP
{
    public class InputManager
    {
        private HashSet<Key> _keys;

        public InputManager()
        {
            _keys = new HashSet<Key>();
        }

        public void OnKeyDown(Key key)
        {
            _keys.Add(key);
        }

        public void OnKeyUp(Key key)
        {
            _keys.Remove(key);
        }

        public bool IsPressed(Key key)
        {
            return _keys.Contains(key);
        }


    }
}
