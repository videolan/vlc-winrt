using System.Text;
using Windows.System;

namespace VLC.Model
{
    public class KeyboardAction
    {
        public VirtualKey Key { get; set; }

        public int KeyCode { get; set; }

        public VirtualKeyModifiers Modifiers { get; set; }

        public VLCAction Action { get; set; }

        private string _keyDes;

        public string KeyDescription => _keyDes ?? (_keyDes = CreateDescription());

        private string CreateDescription()
        {
            var builder = new StringBuilder();
            if (Modifiers != VirtualKeyModifiers.None)
            {
                if ((Modifiers & VirtualKeyModifiers.Shift) == VirtualKeyModifiers.Shift)
                    builder.Append("Shift+");
                if ((Modifiers & VirtualKeyModifiers.Menu) == VirtualKeyModifiers.Menu)
                    builder.Append("Alt+");
                if ((Modifiers & VirtualKeyModifiers.Control) == VirtualKeyModifiers.Control)
                    builder.Append("Ctrl+");
            }
            builder.Append(Key.ToString());
            return builder.ToString();
        }
    }
}
