/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using VLC_WINRT_APP.Database.DataRepository;
using VLC_WINRT_APP.DataRepository;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using SQLite;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Commands.MediaPlayback;
using VLC_WINRT_APP.Commands.Music;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Helpers.MusicLibrary;
using VLC_WINRT_APP.Helpers.MusicLibrary.Deezer;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.Services.Interface;
using VLC_WINRT_APP.Services.RunTime;
using System.Collections.Generic;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Views.MainPages;
using WinRTXamlToolkit.Controls.Extensions;

#if WINDOWS_PHONE_APP
using Windows.Media.Playback;
using VLC_WINRT_APP.BackgroundAudioPlayer.Model;
#endif

namespace VLC_WINRT_APP.ViewModels.MusicVM
{
    public class MusicPlayerVM : MediaPlaybackViewModel
    {
        #region private props
        private TrackCollection _trackCollection;
        private GoToMusicPlayerPage _goToMusicPlayerPage;
        private AlbumItem _currentAlbum;
        private ArtistItem _currentArist;
        private ArtistDataRepository _artistDataRepository = new ArtistDataRepository();
        private AlbumDataRepository _albumDataRepository = new AlbumDataRepository();
        #endregion

        #region private fields
        private ShuffleCommand _shuffle;

        #endregion

        #region public props

        public BackgroundTrackRepository BackgroundTrackRepository { get; set; }

        public AlbumItem CurrentAlbum
        {
            get { return _currentAlbum; }
            set
            {
                SetProperty(ref _currentAlbum, value);
                OnPropertyChanged();
            }
        }

        public ArtistItem CurrentArtist
        {
            get { return _currentArist; }
            set
            {
                SetProperty(ref _currentArist, value);
                OnPropertyChanged();
            }
        }

        public TrackCollection TrackCollection
        {
            get
            {
                _trackCollection = _trackCollection ?? new TrackCollection(true);
                return _trackCollection;
            }
        }

        public async Task SetCurrentArtist()
        {
            if (CurrentTrack == null) return;
            if (CurrentArtist != null && CurrentArtist.Id == CurrentTrack.ArtistId) return;
            CurrentArtist = await _artistDataRepository.LoadArtist(CurrentTrack.ArtistId);
        }

        public async Task SetCurrentAlbum()
        {
            if (CurrentTrack == null) return;
            if (CurrentArtist == null) return;
            if (CurrentAlbum != null && CurrentAlbum.Id == CurrentTrack.AlbumId) return;
            CurrentAlbum = await _albumDataRepository.LoadAlbum(CurrentTrack.AlbumId);
        }

        public TrackItem CurrentTrack
        {
            get
            {
                if (TrackCollection.CurrentTrack == -1
                    || TrackCollection.CurrentTrack == TrackCollection.Playlist.Count)
                    return null;
                return TrackCollection.Playlist[TrackCollection.CurrentTrack];
            }
        }

        public GoToMusicPlayerPage GoToMusicPlayerPage
        {
            get { return _goToMusicPlayerPage; }
            set { SetProperty(ref _goToMusicPlayerPage, value); }
        }

        public ShuffleCommand Shuffle
        {
            get
            {
                _shuffle = _shuffle ?? new ShuffleCommand();
                return _shuffle;
            }
            set { SetProperty(ref _shuffle, value); }
        }

        #endregion

        public MusicPlayerVM(IMediaService mediaService)
            : base(mediaService)
        {
            GoToMusicPlayerPage = new GoToMusicPlayerPage();
            BackgroundTrackRepository = new BackgroundTrackRepository();
        }


        protected override async void OnEndReached()
        {
            if (TrackCollection.Playlist.Count == 0 ||
                !TrackCollection.CanGoNext)
            {
                // Playlist is finished
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    TrackCollection.IsRunning = false;
                    App.ApplicationFrame.Navigate(typeof(MainPageHome));
                });
            }
            else
            {
                await PlayNext();
            }
        }

        public void Stop()
        {
            _mediaService.Stop();
        }

        public async Task PlayNext()
        {
            if (TrackCollection.CanGoNext)
            {
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    TrackCollection.CurrentTrack++;
                    await Play(false);
                });
            }
            else
            {
                TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            }
        }

        public async Task PlayPrevious()
        {
            if (TrackCollection.CanGoPrevious)
            {
                await DispatchHelper.InvokeAsync(() => TrackCollection.CurrentTrack--);
                await Play(false);
            }
            else
            {
                TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            }
        }

        public async Task Play(bool forceVlcLib)
        {
            _mediaService.UseVlcLib = forceVlcLib;
            Stop();
            if (CurrentTrack == null) return;
            LogHelper.Log("Opening file: " + CurrentTrack.Path);
            await SetActiveMusicInfo(CurrentTrack);

            // Setting the info for windows 8 controls
            var resourceLoader = new ResourceLoader();
            string artistName = CurrentTrack.ArtistName ?? resourceLoader.GetString("UnknownArtist");
            string albumName = CurrentTrack.AlbumName;
            string trackName = CurrentTrack.Name ?? resourceLoader.GetString("UnknownTrack");
            var picture = Locator.MusicPlayerVM.CurrentAlbum != null ? Locator.MusicPlayerVM.CurrentAlbum.Picture : null;

            await base._mediaService.SetMediaTransportControlsInfo(artistName, albumName, trackName, picture);

            var notificationOnNewSong = ApplicationSettingsHelper.ReadSettingsValue("NotificationOnNewSong");
            if (notificationOnNewSong != null && (bool)notificationOnNewSong)
            {
                var notificationOnNewSongForeground = ApplicationSettingsHelper.ReadSettingsValue("NotificationOnNewSongForeground");
                if (base._mediaService.IsBackground || (notificationOnNewSongForeground != null && (bool)notificationOnNewSongForeground))
                {
                    ToastHelper.ToastImageAndText04(trackName, albumName, artistName, Locator.MusicPlayerVM.CurrentAlbum.Picture ?? null);
                }
            }
        }

        private async Task SetActiveMusicInfo(TrackItem track)
        {
            var currentTrackFile = await StorageFile.GetFileFromPathAsync(track.Path);
#if WINDOWS_PHONE_APP
            bool playWithLibVlc = !VLCFileExtensions.MFSupported.Contains(currentTrackFile.FileType.ToLower()) || _mediaService.UseVlcLib;
            if (!playWithLibVlc)
            {
                Debug.Assert(Locator.MusicLibraryVM.ContinueIndexing == null || Locator.MusicLibraryVM.ContinueIndexing.Task.IsCompleted);
                Locator.MusicLibraryVM.ContinueIndexing = new TaskCompletionSource<bool>();
                App.BackgroundAudioHelper.PlayAudio(track.Id);
            }
            else
#endif
            {
#if WINDOWS_PHONE_APP
                _mediaService.UseVlcLib = true;
                ToastHelper.Basic("Can't enable background audio", false, "background");
                if (BackgroundMediaPlayer.Current != null &&
                    BackgroundMediaPlayer.Current.CurrentState != MediaPlayerState.Stopped)
                {
                    BackgroundMediaPlayer.Current.Pause();
                    await App.BackgroundAudioHelper.ResetCollection(ResetType.NormalReset);
                }
#endif
                await base.InitializePlayback(track.Path, true, false, currentTrackFile);
                _mediaService.Play();
                await UpdatePlayingUI();
            }
        }

        public override async Task CleanViewModel()
        {
            await base.CleanViewModel();
#if WINDOWS_PHONE_APP
            if (BackgroundMediaPlayer.Current != null &&
                BackgroundMediaPlayer.Current.CurrentState != MediaPlayerState.Stopped)
            {
                BackgroundMediaPlayer.Current.Pause();
                await App.BackgroundAudioHelper.ResetCollection(ResetType.NormalReset);
            }
#endif
            await TrackCollection.ResetCollection();
            TrackCollection.IsRunning = false;
            GC.Collect();
        }

#if WINDOWS_PHONE_APP
        public async Task UpdateTrackFromMF()
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
             {
                 Locator.MusicPlayerVM.OnLengthChanged(
                     (long)BackgroundMediaPlayer.Current.NaturalDuration.TotalMilliseconds);
                 if (!ApplicationSettingsHelper.Contains(BackgroundAudioConstants.CurrentTrack)) return;
                 int index = (int)ApplicationSettingsHelper.ReadSettingsValue(BackgroundAudioConstants.CurrentTrack);
                 Locator.MusicPlayerVM.TrackCollection.CurrentTrack = index;
                 await SetCurrentArtist();
                 await SetCurrentAlbum();
                 await UpdatePlayingUI();
             });
        }
#endif

        private async Task UpdatePlayingUI()
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                TrackCollection.IsRunning = true;
                TrackCollection.SetActiveTrackProperty();
                OnPropertyChanged("TrackCollection");
                OnPropertyChanged("PlayingType");
                OnPropertyChanged("CurrentTrack");
                UpdateTileHelper.UpdateMediumTileWithMusicInfo();
            });
        }
    }
}
