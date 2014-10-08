/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.Collections.ObjectModel;
using VLC_WINRT_APP.Common;
using VLC_WINRT_APP.Model.Music;

namespace VLC_WINRT_APP.ViewModels.MusicVM
{
    public class TrackCollection : BindableBase
    {
        private ObservableCollection<TrackItem> _tracksCollection;
        private int _currentTrack;
        private bool _isRunning;

        public int CurrentTrack
        {
            get { return _currentTrack; }
            set { SetProperty(ref _currentTrack, value); }
        }

        public bool CanGoPrevious
        {
            get
            {
                return (CurrentTrack > 0);
            }
        }

        public bool CanGoNext
        {
            get { return (Playlist.Count != 1) && (CurrentTrack < Playlist.Count - 1); }
        }

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


        #region public fields
        public ObservableCollection<TrackItem> Playlist
        {
            get { return _tracksCollection; }
            set { SetProperty(ref _tracksCollection, value); }
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

        #endregion
    }
}
