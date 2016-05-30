using libVLCX;
using System;
using Windows.Storage;

namespace VLC_WinRT.Model
{
    public interface IMediaItem
    {
        int Id { get; set; }
        string Path { get; set; }
        string Name { get; set; }
        TimeSpan Duration { get; set; }

        StorageFile File { get; }
        Media VlcMedia { get; set; }
        string Token { get; set; }
        Tuple<FromType, string> GetMrlAndFromType(bool preferToken = false);
        bool IsCurrentPlaying();
    }
}
