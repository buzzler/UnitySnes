using System;
using System.Collections.Generic;

namespace UnitySnes
{
    public class InputMapper
    {
        private Dictionary<string, Tuple<int, short>> _map;
        private Dictionary<int, Tuple<string, string>> _invert;

        public InputMapper()
        {
            _map = new Dictionary<string, Tuple<int, short>>();
            _invert = new Dictionary<int, Tuple<string, string>>();
        }
        
        public Tuple<int, short> GetKey(string key)
        {
            return _map.ContainsKey(key) ? _map[key] : null;
        }

        public Tuple<string, string> GetKey(int snesInput)
        {
            return _invert.ContainsKey(snesInput) ? _invert[snesInput] : null;
        }
        
        public void SetKey(int snesInput, string keyPress, string keyRelease)
        {
            var t1 = new Tuple<int, short>(snesInput, 1);
            if (_map.ContainsKey(keyPress))
                _map[keyPress] = t1;
            else
                _map.Add(keyPress, t1);
            
            t1 = new Tuple<int, short>(snesInput, 0);
            if (_map.ContainsKey(keyRelease))
                _map[keyRelease] = t1;
            else
                _map.Add(keyRelease, t1);

            var t2 = new Tuple<string, string>(keyPress, keyRelease);
            if (_invert.ContainsKey(snesInput))
                _invert[snesInput] = t2;
            else
                _invert.Add(snesInput, t2);
        }

        public void SetKeyAsICade()
        {
            SetKey(SnesInput.Up, "W", "E");
            SetKey(SnesInput.Down, "X", "Z");
            SetKey(SnesInput.Left, "A", "Q");
            SetKey(SnesInput.Right, "D", "C");
            SetKey(SnesInput.Select, "L", "V");
            SetKey(SnesInput.Start, "O", "G");
            SetKey(SnesInput.A, "U", "F");
            SetKey(SnesInput.B, "H", "R");
            SetKey(SnesInput.X, "J", "N");
            SetKey(SnesInput.Y, "Y", "T");
            SetKey(SnesInput.L, "K", "P");
            SetKey(SnesInput.R, "I", "M");
        }
        
        public void SetKeyAsICadeOld()
        {
            SetKey(SnesInput.Up, "W", "E");
            SetKey(SnesInput.Down, "X", "Z");
            SetKey(SnesInput.Left, "A", "Q");
            SetKey(SnesInput.Right, "D", "C");
            SetKey(SnesInput.Select, "Y", "T");
            SetKey(SnesInput.Start, "U", "F");
            SetKey(SnesInput.A, "L", "V");
            SetKey(SnesInput.B, "K", "P");
            SetKey(SnesInput.X, "O", "G");
            SetKey(SnesInput.Y, "I", "M");
            SetKey(SnesInput.L, "H", "R");
            SetKey(SnesInput.R, "J", "N");
        }
    }
}
