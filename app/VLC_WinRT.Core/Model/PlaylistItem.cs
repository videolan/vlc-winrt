using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using VLC.Utils;
using Windows.UI.Xaml;

namespace VLC.Model
{
    public class PlaylistItem : BindableBase
    {
        private SmartCollection<IMediaItem> _playlist = new SmartCollection<IMediaItem>();
        private SmartCollection<IMediaItem> _selectedTracks = new SmartCollection<IMediaItem>();
        
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }

        public string Name { get; set; }

        public int CurrentMedia { get; set; }

        public Visibility IsTracksSelectedVisibility => SelectedTracks.Any() ? Visibility.Visible : Visibility.Collapsed;

        [Ignore]
        public SmartCollection<IMediaItem> Playlist
        {
            get { return _playlist; }
            set { SetProperty(ref _playlist, value); }
        }

        [Ignore]
        public SmartCollection<IMediaItem> SelectedTracks
        {
            get { return _selectedTracks; }
            set { SetProperty(ref _selectedTracks, value); }
        }

        public void Remove(IMediaItem media)
        {
            Playlist.Remove(media);
        }

        public PlaylistItem()
        {
            _selectedTracks.CollectionChanged += _selectedTracks_CollectionChanged;
        }

        private async void _selectedTracks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            await DispatchHelper.InvokeAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                OnPropertyChanged(nameof(IsTracksSelectedVisibility));
            });
        }
    }
}
