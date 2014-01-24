using System;

namespace VLC_WINRT.Model
{
    public class MediaHistory
    {
        public DateTime LastPlayed { get; set; }
        public string Filename { get; set; }
        public string Token { get; set; }
        public double TotalWatchedMilliseconds { get; set; }
        public bool IsAudio { get; set; }
    }
}