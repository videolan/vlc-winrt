/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.UI.Core;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SQLite;
using VLC_WinRT.Commands.Music;
using VLC_WinRT.Common;
using VLC_WinRT.Model.Music;
using System.Collections.Generic;

#if WINDOWS_PHONE_APP
using System.Linq;
using VLC_WinRT.BackgroundHelpers;
using VLC_WinRT.BackgroundAudioPlayer.Model;
using VLC_WinRT.Helpers;
#endif

namespace VLC_WinRT.ViewModels.MusicVM
{
    public class TrackCollection : BindableBase
    {
        private ObservableCollection<TrackItem> _tracksCollection;
        private ObservableCollection<TrackItem> _nonShuffledPlaylist;
        private int _currentTrack;
        private bool _isRunning;
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
                var previous = (CurrentTrack > 0);
                Locator.MediaPlaybackViewModel.SystemMediaTransportControlsBackPossible(previous);
                return previous;
            }
        }

        [Ignore]
        public bool CanGoNext
        {
            get
            {
                var next = (Playlist.Count != 1) && (CurrentTrack < Playlist.Count - 1);
                Locator.MediaPlaybackViewModel.SystemMediaTransportControlsNextPossible(next);
                return next;
            }
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
        public PlayTrackCollCommand PlayTrackCollCommand { get; } = new PlayTrackCollCommand();

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
                NonShuffledPlaylist?.Clear();
                IsShuffled = false;
            });
        }

        public void SetActiveTrackProperty()
        {
            try
            {
                if (Playlist == null || _currentTrack == -1) return;
                foreach (var trackItem in Playlist)
                {
                    if (trackItem == null) continue;
                    trackItem.IsCurrentPlaying = Playlist[_currentTrack].Id == trackItem.Id;
                }
                OnPropertyChanged("CanGoPrevious");
                OnPropertyChanged("CanGoNext");
            }
            catch (ArgumentOutOfRangeException exception)
            {
                _currentTrack = 0;
                SetActiveTrackProperty();
            }
            catch (Exception exception)
            {

            }
        }

        public async Task SetPlaylist(ObservableCollection<TrackItem> playlist)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Playlist = playlist);
#if WINDOWS_PHONE_APP
            var backgroundTracks = BackgroundTaskTools.CreateBackgroundTrackItemList(Locator.MediaPlaybackViewModel.TrackCollection.Playlist.ToList());
            await App.BackgroundAudioHelper.AddToPlaylist(backgroundTracks);
#endif
        }

        public async Task Shuffle()
        {
            if (IsShuffled)
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
            else
            {
                await SetPlaylist(Locator.MediaPlaybackViewModel.TrackCollection.NonShuffledPlaylist);
            }
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

        public async Task Add(TrackItem trackItem, bool isPlayingPlaylist)
        {
            if (Playlist.FirstOrDefault(x => x.Id == trackItem.Id) != null)return;
            Playlist.Add(trackItem);
#if WINDOWS_PHONE_APP
            var backgroundTrack = BackgroundTaskTools.CreateBackgroundTrackItem(trackItem);
            await App.BackgroundAudioHelper.AddToPlaylist(backgroundTrack);
#endif
        }

        public async Task Add(List<TrackItem> trackItems)
        {
            foreach (var track in trackItems)
                Playlist.Add(track);
#if WINDOWS_PHONE_APP
            var backgroundTracks = BackgroundTaskTools.CreateBackgroundTrackItemList(trackItems);
            await App.BackgroundAudioHelper.AddToPlaylist(backgroundTracks);
#endif
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
                var trackItem = await Locator.MusicLibraryVM._trackDataRepository.LoadTrack(trackId);
                Playlist.Add(trackItem);
            }
            IsRunning = true;

            var currentTrack = ApplicationSettingsHelper.ReadSettingsValue(BackgroundAudioConstants.CurrentTrack);
            if (currentTrack != null)
            {
                CurrentTrack = (int) currentTrack;
                if ((int)currentTrack == -1)
                {
                    // Background Audio was terminated
                    // We need to reset the playlist, or set the current track 0.
                    ApplicationSettingsHelper.SaveSettingsValue(BackgroundAudioConstants.CurrentTrack, 0);
                    CurrentTrack = 0;
                }
            }
            await Locator.MusicPlayerVM.UpdateTrackFromMF();
            App.BackgroundAudioHelper.RestorePlaylist();
#endif
            SetActiveTrackProperty();
        }
        #endregion
    }
}
