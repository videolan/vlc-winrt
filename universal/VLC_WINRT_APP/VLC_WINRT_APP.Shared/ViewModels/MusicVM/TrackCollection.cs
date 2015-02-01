/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.UI.Core;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SQLite;
using VLC_WINRT_APP.Commands.Music;
using VLC_WINRT_APP.Common;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Helpers.MusicLibrary;
using VLC_WINRT_APP.Helpers.MusicLibrary.Deezer;
using VLC_WINRT_APP.Model.Music;

#if WINDOWS_PHONE_APP
using VLC_WINRT_APP.BackgroundHelpers;
using VLC_WINRT_APP.Database.DataRepository;
using VLC_WINRT_APP.BackgroundAudioPlayer.Model;
#endif

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
            private set
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
        public TrackCollection(bool isRuntimePlaylist)
        {
            if (isRuntimePlaylist)
            {
                RestorePlaylist();
            }
            _tracksCollection = new ObservableCollection<TrackItem>();
            InitializePlaylist();
        }
        public TrackCollection()
        {
            _tracksCollection = new ObservableCollection<TrackItem>();
            InitializePlaylist();
        }
        #endregion

        #region methods
        public void InitializePlaylist()
        {
            Playlist.Clear();
            CurrentTrack = -1;
        }

        public async Task ResetCollection()
        {
#if WINDOWS_PHONE_APP
            await App.BackgroundAudioHelper.ResetCollection(ResetType.NormalReset);
#endif
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Playlist.Clear();
                CurrentTrack = -1;
            });
        }

        public void SetActiveTrackProperty()
        {
            try
            {
                if (Playlist != null)
                    foreach (var trackItem in Playlist)
                    {
                        if (trackItem == null) continue;
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
            catch (ArgumentOutOfRangeException exception)
            {
                _currentTrack = 0;
                SetActiveTrackProperty();
            }
        }

        public async Task SetPlaylist(ObservableCollection<TrackItem> playlist)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Playlist = playlist);
#if WINDOWS_PHONE_APP
            var backgroundTracks = BackgroundTaskTools.CreateBackgroundTrackItemList(Locator.MusicPlayerVM.TrackCollection.Playlist.ToList());
            await App.BackgroundAudioHelper.AddToPlaylist(backgroundTracks);
#endif
        }

        public async Task Shuffle()
        {
            if (IsShuffled)
            {
                await SetPlaylist(Locator.MusicPlayerVM.TrackCollection.NonShuffledPlaylist);
            }
            else
            {
                NonShuffledPlaylist = new ObservableCollection<TrackItem>(Playlist);
                Random r = new Random();
                for (int i = 0; i < Playlist.Count; i++)
                {
                    if (i > CurrentTrack)
                    {
                        int index1 = r.Next(i, Playlist.Count);
                        int index2 = r.Next(i, Playlist.Count);
                        Playlist.Move(index1, index2);
                    }
                }
            }
            IsShuffled = !IsShuffled;

#if WINDOWS_PHONE_APP
            await App.BackgroundAudioHelper.ResetCollection(ResetType.ShuffleReset);
            var backgorundTracks = BackgroundTaskTools.CreateBackgroundTrackItemList(Playlist.ToList());
            await App.BackgroundAudioHelper.AddToPlaylist(backgorundTracks);
#endif
        }

        public void Remove(TrackItem trackItem)
        {
            Playlist.Remove(trackItem);
        }

        public void Add(TrackItem trackItem, bool isPlayingPlaylist)
        {
            Playlist.Add(trackItem);
        }

        public async Task RestorePlaylist()
        {
#if WINDOWS_PHONE_APP
            var playlist = Locator.MusicPlayerVM.BackgroundTrackRepository.LoadPlaylist();
            if (!playlist.Any())
            {
                return;
            }
            var trackIds = playlist.Select(node => node.Id);
            Playlist = new ObservableCollection<TrackItem>();
            foreach (int trackId in trackIds)
            {
                var trackItem = await MusicLibraryVM._trackDataRepository.LoadTrack(trackId);
                Playlist.Add(trackItem);
            }
            IsRunning = true;

            var currentTrack = ApplicationSettingsHelper.ReadSettingsValue(BackgroundAudioConstants.CurrentTrack);
            if (currentTrack != null)
                CurrentTrack = (int)currentTrack;
            await Locator.MusicPlayerVM.UpdateTrackFromMF();
            await App.BackgroundAudioHelper.RestorePlaylist();
#endif
            SetActiveTrackProperty();
        }
        #endregion
    }
}
