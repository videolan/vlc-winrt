using System.Collections.Generic;
using System.Linq;
using VLC_WinRT.BackgroundAudioPlayer.Model;
using VLC_WinRT.Model.Music;

namespace VLC_WinRT.BackgroundHelpers
{
    public static class BackgroundTaskTools
    {
        public static BackgroundTrackItem CreateBackgroundTrackItem(TrackItem track)
        {
            if (track == null) return null;
            BackgroundTrackItem audiotrack = new BackgroundTrackItem(track.Id, track.AlbumId, track.ArtistId, track.ArtistName, track.AlbumName, track.Name, track.Path);
            return audiotrack;
        }

        public static List<BackgroundTrackItem> CreateBackgroundTrackItemList(List<TrackItem> tracks)
        {
            return tracks.Select(CreateBackgroundTrackItem).ToList();
        }
    }
}
