using System.Collections.Generic;
using System.Linq;
using VLC_WINRT_APP.BackgroundAudioPlayer;
using VLC_WINRT_APP.Model.Music;

namespace VLC_WINRT_APP.BackgroundHelpers
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
