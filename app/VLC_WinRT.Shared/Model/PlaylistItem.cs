using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Model
{
    public class PlaylistItem : BindableBase
    {
        private SmartCollection<IMediaItem> _playlist = new SmartCollection<IMediaItem>();
        
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }

        public string Name { get; set; }

        public int CurrentMedia { get; set; }

        [Ignore]
        public SmartCollection<IMediaItem> Playlist
        {
            get { return _playlist; }
            set { SetProperty(ref _playlist, value); }
        }

        [Ignore]
        public SmartCollection<IMediaItem> SelectedTracks { get; private set; }

        public void Remove(IMediaItem media)
        {
            Playlist.Remove(media);
        }
    }
}
