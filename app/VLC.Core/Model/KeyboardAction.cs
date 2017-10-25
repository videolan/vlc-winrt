using Windows.System;

namespace VLC.Model
{
    public class KeyboardAction
    {
        public VirtualKey MainKey { get; set; }
        public int KeyCode { get; set; }
        public VirtualKey SecondKey { get; set; }

        public VLCAction Action { get; set; }

        private string _keyDes;

        public string KeyDescription
        {
            get
            {
                if (!string.IsNullOrEmpty(_keyDes)) return _keyDes;
                _keyDes = MainKey.ToString();
                if (SecondKey == VirtualKey.None) return _keyDes;
                _keyDes += " + ";
                _keyDes += SecondKey.ToString();
                return _keyDes;
            }
        }
    }
}
