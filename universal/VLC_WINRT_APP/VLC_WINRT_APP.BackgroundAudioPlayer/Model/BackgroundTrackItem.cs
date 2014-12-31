using System;

namespace VLC_WINRT_APP.BackgroundAudioPlayer
{
    public sealed class BackgroundTrackItem
    {
        public int Id { get; set; }
        public int AlbumId { get; set; }
        public int ArtistId { get; set; }

        public string ArtistName { get; set; }

        public string AlbumName { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public int Index { get; set; }

        public TimeSpan Duration { get; set; }
        public bool Favorite { get; set; }

        public string Thumbnail { get; set; }

        public int CurrentPosition { get; set; }
    
        public bool IsCurrentPlaying { get; set; }
    }
}
