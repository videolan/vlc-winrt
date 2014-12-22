/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.Collections.ObjectModel;
using System.Linq;
using SQLite;
using VLC_WINRT_APP.Commands.Music;
using VLC_WINRT_APP.Common;
using VLC_WINRT_APP.Helpers.MusicLibrary;
using VLC_WINRT_APP.Helpers.MusicLibrary.Deezer;
using VLC_WINRT_APP.Model.Music;

namespace VLC_WINRT_APP.ViewModels.MusicVM
{
    public class TrackCollection : BindableBase
    {
        private ObservableCollection<TrackItem> _tracksCollection;
        private ObservableCollection<TrackItem> _nonShuffledPlaylist;
        private int _currentTrack;
        private bool _isRunning;
        private PlayTrackCollCommand _playTrackCollCommand;
        private bool _isShuffled;

        // ui related management
        private ObservableCollection<TrackItem> _selectedTracks;

        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }

        public string Name { get; set; }

        public int CurrentTrack
        {
            get
            {
                return _currentTrack;
            }
            set
            {
                SetProperty(ref _currentTrack, value);
            }
        }

        [Ignore]
        public bool CanGoPrevious
        {
            get
            {
                return (CurrentTrack > 0);
            }
        }

        [Ignore]
        public bool CanGoNext
        {
            get { return (Playlist.Count != 1) && (CurrentTrack < Playlist.Count - 1); }
        }

        [Ignore]
        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
            set
            {
                SetProperty(ref _isRunning, value);
            }
        }

        [Ignore]
        public PlayTrackCollCommand PlayTrackCollCommand
        {
            get { return _playTrackCollCommand ?? (_playTrackCollCommand = new PlayTrackCollCommand()); }
        }

        [Ignore]
        public bool IsShuffled
        {
            get { return _isShuffled; }
            set { SetProperty(ref _isShuffled, value); }
        }

        #region public fields
        [Ignore]
        public ObservableCollection<TrackItem> Playlist
        {
            get { return _tracksCollection; }
            set
            {
                SetProperty(ref _tracksCollection, value);
            }
        }

        [Ignore]
        public ObservableCollection<TrackItem> NonShuffledPlaylist
        {
            get { return _nonShuffledPlaylist; }
            set { SetProperty(ref _nonShuffledPlaylist, value); }
        }

        [Ignore]
        public ObservableCollection<TrackItem> SelectedTracks
        {
            get { return _selectedTracks ?? (_selectedTracks = new ObservableCollection<TrackItem>()); }
            set { SetProperty(ref _selectedTracks, value); }
        }

        #endregion

        #region ctors
        public TrackCollection()
        {
            _tracksCollection = new ObservableCollection<TrackItem>();
            ResetCollection();
        }
        #endregion

        #region methods

        public void ResetCollection()
        {
            Playlist.Clear();
            CurrentTrack = -1;
        }

        public void SetActiveTrackProperty()
        {
            foreach (var trackItem in Playlist)
            {
                if (Playlist[_currentTrack].Id == trackItem.Id)
                {
                    trackItem.IsCurrentPlaying = true;
                }
                else
                {
                    trackItem.IsCurrentPlaying = false;
                }
            }
        }

        #endregion
    }
}
