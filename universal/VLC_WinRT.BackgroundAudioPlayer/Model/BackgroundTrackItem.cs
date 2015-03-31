using System;

namespace VLC_WinRT.BackgroundAudioPlayer
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

        public bool IsCurrentPlaying { get; set; }

        public BackgroundTrackItem(int id, int albumId, int artistId, string artistName, string albumName, string name,
            string path)
        {
            Id = id;
            AlbumId = albumId;
            ArtistId = artistId;
            ArtistName = artistName;
            AlbumName = albumName;
            Name = name;
            Path = path;
        }
        public BackgroundTrackItem() { }
    }
}
