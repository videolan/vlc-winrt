using System;

namespace VLC_WinRT.Model.Music
{
    public class MediaProperties
    {
        public string Album { get; set; }
        public string Artist { get; set; }
        public TimeSpan Duration { get; set; }
        public string Title { get; set; }
        public uint Tracknumber { get; set; }
        public uint Year { get; set; }
        public string Genre { get; set; }
    }
}
