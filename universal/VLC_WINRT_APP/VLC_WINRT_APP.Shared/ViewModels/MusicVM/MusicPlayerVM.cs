/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using VLC_WINRT_APP.DataRepository;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Core;
using VLC_WINRT_APP.Commands.MediaPlayback;
using VLC_WINRT_APP.Commands.Music;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.Commands.Social;
using VLC_WINRT_APP.Common;

#if WINDOWS_PHONE_APP
using Windows.Media.Playback;
using VLC_WINRT_APP.BackgroundAudioPlayer.Model;
using VLC_WINRT_APP.Database.DataRepository;
using VLC_WINRT_APP.Model;
#endif

namespace VLC_WINRT_APP.ViewModels.MusicVM
{
    public class MusicPlayerVM : BindableBase
    {
        #region private props
        private GoToMusicPlayerPage _goToMusicPlayerPage;
        private ShareNowPlayingMusicCommand _shareNowPlayingMusicCommand;
        private GoToMusicPlaylistPageCommand _goToMusicPlaylistPageCommand;
        private AlbumItem _currentAlbum;
        private ArtistItem _currentArist;
        private ArtistDataRepository _artistDataRepository = new ArtistDataRepository();
        private AlbumDataRepository _albumDataRepository = new AlbumDataRepository();
        #endregion

        #region private fields
        private ShuffleCommand _shuffle;
        #endregion

        #region public props
#if WINDOWS_PHONE_APP
        public BackgroundTrackRepository BackgroundTrackRepository { get; set; }
#endif

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

        public TrackItem CurrentTrack
        {
            get
            {
                if (Locator.MediaPlaybackViewModel.TrackCollection.CurrentTrack == -1
                    || Locator.MediaPlaybackViewModel.TrackCollection.CurrentTrack == Locator.MediaPlaybackViewModel.TrackCollection.Playlist.Count)
                    return null;
                if (Locator.MediaPlaybackViewModel.TrackCollection.CurrentTrack > Locator.MediaPlaybackViewModel.TrackCollection.Playlist.Count)
                {
                    App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Locator.MediaPlaybackViewModel.TrackCollection.CurrentTrack = 0);
                    return null;
                }
                return Locator.MediaPlaybackViewModel.TrackCollection.Playlist[Locator.MediaPlaybackViewModel.TrackCollection.CurrentTrack];
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

        public ShareNowPlayingMusicCommand ShareNowPlayingMusicCommand
        {
            get
            {
                return _shareNowPlayingMusicCommand ?? (_shareNowPlayingMusicCommand = new ShareNowPlayingMusicCommand());
            }
        }

        public GoToMusicPlaylistPageCommand GoToMusicPlaylistPageCommand
        {
            get
            {
                return _goToMusicPlaylistPageCommand ??
                       (_goToMusicPlaylistPageCommand = new GoToMusicPlaylistPageCommand());
            }
        }
        #endregion

        public MusicPlayerVM()
        {
            GoToMusicPlayerPage = new GoToMusicPlayerPage();
#if WINDOWS_PHONE_APP
            BackgroundTrackRepository = new BackgroundTrackRepository();
#endif
        }



        public async Task UpdateWindows8UI()
        {
            // Setting the info for windows 8 controls
            var resourceLoader = new ResourceLoader();
            string artistName = CurrentTrack.ArtistName ?? resourceLoader.GetString("UnknownArtist");
            string albumName = CurrentTrack.AlbumName;
            string trackName = CurrentTrack.Name ?? resourceLoader.GetString("UnknownTrack");
            var picture = Locator.MusicPlayerVM.CurrentAlbum != null ? Locator.MusicPlayerVM.CurrentAlbum.AlbumCoverUri : null;

            await Locator.MediaPlaybackViewModel.SetMediaTransportControlsInfo(artistName, albumName, trackName, picture);

            var notificationOnNewSong = ApplicationSettingsHelper.ReadSettingsValue("NotificationOnNewSong");
            if (notificationOnNewSong != null && (bool)notificationOnNewSong)
            {
                var notificationOnNewSongForeground = ApplicationSettingsHelper.ReadSettingsValue("NotificationOnNewSongForeground");
                if (Locator.MediaPlaybackViewModel._mediaService.IsBackground || (notificationOnNewSongForeground != null && (bool)notificationOnNewSongForeground))
                {
                    ToastHelper.ToastImageAndText04(trackName, albumName, artistName, (Locator.MusicPlayerVM.CurrentAlbum == null) ? null : Locator.MusicPlayerVM.CurrentAlbum.AlbumCoverUri ?? null);
                }
            }
        }

#if WINDOWS_PHONE_APP
        public async Task UpdateTrackFromMF()
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                Locator.MediaPlaybackViewModel.OnLengthChanged((long)BackgroundMediaPlayer.Current.NaturalDuration.TotalMilliseconds);
                if (!ApplicationSettingsHelper.Contains(BackgroundAudioConstants.CurrentTrack)) return;
                int index = (int)ApplicationSettingsHelper.ReadSettingsValue(BackgroundAudioConstants.CurrentTrack);
                Locator.MediaPlaybackViewModel.TrackCollection.CurrentTrack = index;
                await SetCurrentArtist();
                await SetCurrentAlbum();
                await UpdatePlayingUI();
            });
        }
#endif

        public async Task UpdatePlayingUI()
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Locator.MediaPlaybackViewModel.TrackCollection.IsRunning = true;
                Locator.MediaPlaybackViewModel.TrackCollection.SetActiveTrackProperty();
                OnPropertyChanged("TrackCollection");
                OnPropertyChanged("PlayingType");
                OnPropertyChanged("CurrentTrack");
                UpdateTileHelper.UpdateMediumTileWithMusicInfo();
            });
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
    }
}
