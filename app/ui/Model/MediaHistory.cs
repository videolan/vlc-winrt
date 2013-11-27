using System;

namespace VLC_WINRT.Model
{
    public class MediaHistory
    {
        public DateTime LastPlayed { get; set; }
        public string Filename { get; set; }
        public string Token { get; set; }
        public TimeSpan TotalWatched { get; set; }
    }
}