using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Model
{
    public class PlaylistItem
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }

        public string Name { get; set; }

        public int CurrentMedia { get; set; }

        [Ignore]
        public SmartCollection<IMediaItem> Playlist { get; private set; }

        [Ignore]
        public ObservableCollection<IMediaItem> SelectedTracks { get; private set; }

        public void Remove(IMediaItem media)
        {
            Playlist.Remove(media);
        }
    }
}
