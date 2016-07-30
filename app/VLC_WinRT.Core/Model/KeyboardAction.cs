using Windows.System;
using SQLite;

namespace VLC_WinRT.Model
{
    public class KeyboardAction
    {
        private string keyDes;

        [NotNull]
        public VirtualKey MainKey { get; set; }

        public VirtualKey SecondKey { get; set; }

        [NotNull, PrimaryKey]
        public VLCAction Action { get; set; }

        [Ignore]
        public string KeyDescription
        {
            get
            {
                if (string.IsNullOrEmpty(keyDes))
                {
                    keyDes= MainKey.ToString();
                    if (SecondKey != VirtualKey.None)
                    {
                        keyDes += " + ";
                        keyDes += SecondKey.ToString();
                    }
                }
                return keyDes;
            }
        }
    }
}
