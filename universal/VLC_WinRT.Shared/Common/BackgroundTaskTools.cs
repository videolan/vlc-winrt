using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VLC_WinRT.Model.Music;
using BackgroundTrackItem = VLC_WinRT.Database.Model.BackgroundTrackItem;

namespace VLC_WinRT.Common
{
    public class BackgroundTaskTools
    {
        public static BackgroundTrackItem CreateBackgroundTrackItem(TrackItem track)
        {
            BackgroundTrackItem audiotrack = new BackgroundTrackItem();
            if (track == null) return audiotrack;
            audiotrack.AlbumName = track.AlbumName;
            audiotrack.Path = track.Path;
            audiotrack.ArtistName = track.ArtistName;
            audiotrack.Name = track.Name;
            audiotrack.Thumbnail = track.Thumbnail;
            audiotrack.Duration = track.Duration;
            audiotrack.Id = track.Id;
            return audiotrack;
        }

        public static List<BackgroundTrackItem> CreateBackgroundTrackItemList(List<TrackItem> tracks)
        {
            return tracks.Select(CreateBackgroundTrackItem).ToList();
        }
    }
}
