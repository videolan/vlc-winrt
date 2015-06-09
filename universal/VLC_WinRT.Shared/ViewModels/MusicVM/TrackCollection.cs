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
using VLC_WinRT.Commands.MusicPlayer;
using VLC_WinRT.Model.Music;
using System.Collections.Generic;
using System.Linq;
using VLC_WinRT.BackgroundHelpers;
using VLC_WinRT.BackgroundAudioPlayer.Model;
using VLC_WinRT.Helpers;
using VLC_WinRT.Model;
using VLC_WinRT.Model.Video;
using VLC_WinRT.Utils;
using System.Diagnostics;

namespace VLC_WinRT.ViewModels.MusicVM
{
    public class TrackCollection : BindableBase
    {
        private ObservableCollection<IVLCMedia> _tracksCollection;
        private ObservableCollection<IVLCMedia> _nonShuffledPlaylist;
        private int _currentTrack;
        private bool _isRunning;
        private bool _isShuffled;
        private bool _repeat;

        // ui related management
        private ObservableCollection<IVLCMedia> _selectedTracks;

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
        public bool Repeat
        {
            get { return _repeat; }
            set { SetProperty(ref _repeat, value); }
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
        public ObservableCollection<IVLCMedia> Playlist
        {
            get { return _tracksCollection; }
            private set { SetProperty(ref _tracksCollection, value); }
        }

        [Ignore]
        public ObservableCollection<IVLCMedia> NonShuffledPlaylist
        {
            get { return _nonShuffledPlaylist; }
            set { SetProperty(ref _nonShuffledPlaylist, value); }
        }

        [Ignore]
        public ObservableCollection<IVLCMedia> SelectedTracks
        {
            get { return _selectedTracks ?? (_selectedTracks = new ObservableCollection<IVLCMedia>()); }
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
            _tracksCollection = new ObservableCollection<IVLCMedia>();
            InitializePlaylist();
        }

        public TrackCollection()
        {
            _tracksCollection = new ObservableCollection<IVLCMedia>();
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
            await App.BackgroundAudioHelper.ResetCollection(ResetType.NormalReset);
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
                if (Playlist == null || !Playlist.Any() || _currentTrack == -1) return;
                foreach (var trackItem in Playlist)
                {
                    if (trackItem == null) continue;
                    trackItem.IsCurrentPlaying = false;
                }
                if (Playlist[_currentTrack] != null)
                {
                    Playlist[_currentTrack].IsCurrentPlaying = true;
                    Debug.WriteLine(Playlist[_currentTrack].Path + " Is the active track");
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

        public async Task SetPlaylist(ObservableCollection<IVLCMedia> playlist)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Playlist = playlist);
            var backgroundTracks = BackgroundTaskTools.CreateBackgroundTrackItemList(Locator.MediaPlaybackViewModel.TrackCollection.Playlist.ToTrackItemPlaylist());
            await App.BackgroundAudioHelper.AddToPlaylist(backgroundTracks);
        }

        public async Task Shuffle()
        {
            if (IsShuffled)
            {
                NonShuffledPlaylist = new ObservableCollection<IVLCMedia>(Playlist);
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
            await App.BackgroundAudioHelper.ResetCollection(ResetType.ShuffleReset);
            var backgorundTracks = BackgroundTaskTools.CreateBackgroundTrackItemList(Playlist.ToTrackItemPlaylist());
            await App.BackgroundAudioHelper.AddToPlaylist(backgorundTracks);
        }

        public void Remove(IVLCMedia media)
        {
            Playlist.Remove(media);
        }

        public async Task Add(VideoItem videoItem, bool isPlayingPlaylist)
        {
            if (Playlist.FirstOrDefault(x => x.Path == videoItem.Path) != null) return;
            Playlist.Add(videoItem);
        }

        public async Task Add(TrackItem trackItem, bool isPlayingPlaylist)
        {
            if (Playlist.FirstOrDefault(x => x.Id == trackItem.Id) != null) return;
            trackItem.Index = (uint)Playlist.Count;
            Playlist.Add(trackItem);
            var backgroundTrack = BackgroundTaskTools.CreateBackgroundTrackItem(trackItem);
            await App.BackgroundAudioHelper.AddToPlaylist(backgroundTrack);
        }

        public async Task Add(List<TrackItem> trackItems)
        {
            foreach (var track in trackItems)
            {
                track.Index = (uint)Playlist.Count;
                Playlist.Add(track);
            }
            var backgroundTracks = BackgroundTaskTools.CreateBackgroundTrackItemList(trackItems);
            await App.BackgroundAudioHelper.AddToPlaylist(backgroundTracks);
        }

        public async Task RestorePlaylist()
        {
            var playlist = Locator.MusicPlayerVM.BackgroundTrackRepository.LoadPlaylist();
            if (!playlist.Any())
            {
                return;
            }
            var trackIds = playlist.Select(node => node.Id);
            Playlist = new ObservableCollection<IVLCMedia>();
            foreach (int trackId in trackIds)
            {
                var trackItem = await Locator.MusicLibraryVM._trackDatabase.LoadTrack(trackId);
                Playlist.Add(trackItem);
            }
            IsRunning = true;

            var currentTrack = ApplicationSettingsHelper.ReadSettingsValue(BackgroundAudioConstants.CurrentTrack);
            if (currentTrack != null)
            {
                CurrentTrack = (int)currentTrack;
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
            SetActiveTrackProperty();
            if (Locator.MusicPlayerVM.CurrentTrack != null)
                await Locator.MediaPlaybackViewModel.SetMedia(Locator.MusicPlayerVM.CurrentTrack, false, false);
        }
        #endregion

        /// <summary>
        /// Only this method should set the CurrentTrack property of TrackCollection.
        /// </summary>
        /// <param name="index"></param>
        public async Task SetCurrentTrackPosition(int index)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => CurrentTrack = index);
        }
    }
}
