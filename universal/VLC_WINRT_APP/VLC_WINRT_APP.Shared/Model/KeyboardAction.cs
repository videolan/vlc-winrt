using Windows.System;
using SQLite;

namespace VLC_WINRT_APP.Model
{
    public class KeyboardAction
    {
        [NotNull]
        public VirtualKey MainKey { get; set; }

        public VirtualKey SecondKey { get; set; }

        [NotNull, PrimaryKey]
        public VLCAction Action { get; set; }
    }
}
