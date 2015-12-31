/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using VLC_WinRT.BackgroundAudioPlayer.Model;
using VLC_WinRT.BackgroundHelpers;
using VLC_WinRT.Commands.MediaPlayback;
using VLC_WinRT.Commands.MusicPlayer;
using VLC_WinRT.Helpers;
using VLC_WinRT.Model;
using VLC_WinRT.Model.Music;
using VLC_WinRT.SharedBackground.Helpers.MusicPlayer;
using VLC_WinRT.Utils;
#if WINDOWS_PHONE_APP

#endif

namespace VLC_WinRT.ViewModels.MusicVM
{
    public class MusicPlayerVM : BindableBase
    {
        #region duplicate with Background Audio Task on WP
        LastFMScrobbler LastFMScrobbler;
        #endregion
        #region private props
        private AlbumItem _currentAlbum;
        private ArtistItem _currentArist;
        #endregion

        #region private fields
        #endregion

        #region public props

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
                var media = Locator.MediaPlaybackViewModel.CurrentMedia;
                return (media is TrackItem) ? (TrackItem)media : null;
            }
        }

        public GoToMusicPlayerPage GoToMusicPlayerPage { get; } = new GoToMusicPlayerPage();

        public ShuffleCommand Shuffle { get; } = new ShuffleCommand();

        public ShareNowPlayingMusicCommand ShareNowPlayingMusicCommand { get; } = new ShareNowPlayingMusicCommand();

        public GoToMusicPlaylistPageCommand GoToMusicPlaylistPageCommand { get; } = new GoToMusicPlaylistPageCommand();
        public AddToPlayingPlaylist AddToPlayingPlaylist { get; } = new AddToPlayingPlaylist();
        public Visibility IsMiniPlayerVisible
        {
            get
            {
                OnPropertyChanged(nameof(IsMiniPlayerVisibleHomePage));
                if (Locator.MediaPlaybackViewModel.TrackCollection.IsRunning &&
                    Locator.MediaPlaybackViewModel.PlayingType == PlayingType.Music &&
                    (Locator.NavigationService.CurrentPage != VLCPage.CurrentPlaylistPage &&
                     Locator.NavigationService.CurrentPage != VLCPage.MusicPlayerPage &&
                     Locator.NavigationService.CurrentPage != VLCPage.VideoPlayerPage))
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility IsMiniPlayerVisibleHomePage
        {
            get
            {
                if (Locator.MediaPlaybackViewModel.TrackCollection.IsRunning &&
                    Locator.MediaPlaybackViewModel.PlayingType == PlayingType.Music &&
                    (Locator.NavigationService.CurrentPage != VLCPage.MainPageVideo &&
                     Locator.NavigationService.CurrentPage != VLCPage.MainPageFileExplorer &&
                     Locator.NavigationService.CurrentPage != VLCPage.MainPageMusic &&
                     Locator.NavigationService.CurrentPage != VLCPage.MainPageNetwork))
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }
        #endregion

        public MusicPlayerVM()
        {
            Locator.MediaPlaybackViewModel.TrackCollection.PropertyChanged += MediaPlaybackViewModel_PropertyChanged;
            Locator.NavigationService.ViewNavigated += ViewNavigated;
        }

        private void MediaPlaybackViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TrackCollection.IsRunning))
            {
                OnPropertyChanged(nameof(IsMiniPlayerVisible));
            }
        }

        private void ViewNavigated(object sender, VLCPage p)
        {
            OnPropertyChanged(nameof(IsMiniPlayerVisible));
        }

        public async Task UpdateWindows8UI()
        {
            // Setting the info for windows 8 controls
            string artistName = CurrentTrack?.ArtistName ?? Strings.UnknownArtist;
            string albumName = CurrentTrack?.AlbumName ?? Strings.UnknownAlbum;
            string trackName = CurrentTrack?.Name ?? Strings.UnknownTrack;
            var picture = Locator.MusicPlayerVM.CurrentAlbum != null ? Locator.MusicPlayerVM.CurrentAlbum.AlbumCoverFullUri : null;

            await Locator.MediaPlaybackViewModel.SetMediaTransportControlsInfo(artistName, albumName, trackName, picture);

            var notificationOnNewSong = ApplicationSettingsHelper.ReadSettingsValue("NotificationOnNewSong");
            if (notificationOnNewSong != null && (bool)notificationOnNewSong)
            {
                var notificationOnNewSongForeground = ApplicationSettingsHelper.ReadSettingsValue("NotificationOnNewSongForeground");
                if (Locator.MainVM.IsBackground || (notificationOnNewSongForeground != null && (bool)notificationOnNewSongForeground))
                {
                    ToastHelper.ToastImageAndText04(trackName, albumName, artistName, (Locator.MusicPlayerVM.CurrentAlbum == null) ? null : Locator.MusicPlayerVM.CurrentAlbum.AlbumCoverFullUri ?? null);
                }
            }
        }

        public async Task UpdateTrackFromMF()
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                try
                {
#if WINDOWS_PHONE_APP
                    // TODO : this shouldn't be here
                    var milliseconds = BackgroundAudioHelper.Instance?.NaturalDuration.TotalMilliseconds;
                    if (milliseconds != null && milliseconds.HasValue && double.IsNaN(milliseconds.Value))
                        Locator.MediaPlaybackViewModel.OnLengthChanged((long)milliseconds);
#endif
                    if (!ApplicationSettingsHelper.Contains(BackgroundAudioConstants.CurrentTrack)) return;
                    int index = (int)ApplicationSettingsHelper.ReadSettingsValue(BackgroundAudioConstants.CurrentTrack);
                    Locator.MediaPlaybackViewModel.TrackCollection.CurrentTrack = index;
                    await SetCurrentArtist();
                    await SetCurrentAlbum();
                    await UpdatePlayingUI();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(nameof(MusicPlayerVM) + " " + nameof(UpdateTrackFromMF) + " Exception : " + e);
                }
            });
        }

        public async Task UpdatePlayingUI()
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Locator.MediaPlaybackViewModel.TrackCollection.IsRunning = true;
                Locator.MediaPlaybackViewModel.TrackCollection.SetActiveTrackProperty();
                OnPropertyChanged(nameof(TrackCollection));
                OnPropertyChanged(nameof(PlayingType));
                OnPropertyChanged(nameof(CurrentTrack));
#if WINDOWS_UWP
                UpdateTileHelper.UpdateMusicTile();
#else
                UpdateTileHelper.UpdateMediumTileWithMusicInfo();
#endif
            });
        }

        public async Task SetCurrentArtist()
        {
            if (CurrentTrack == null) return;
            if (CurrentArtist != null && CurrentArtist.Id == CurrentTrack.ArtistId) return;
            CurrentArtist = await Locator.MusicLibraryVM.MusicLibrary.LoadArtist(CurrentTrack.ArtistId);
        }

        public async Task SetCurrentAlbum()
        {
            if (CurrentTrack == null) return;
            if (CurrentArtist == null) return;
            if (CurrentAlbum != null && CurrentAlbum.Id == CurrentTrack.AlbumId) return;
            CurrentAlbum = Locator.MusicLibraryVM.MusicLibrary.LoadAlbum(CurrentTrack.AlbumId);
        }


        public async Task Scrobble()
        {
            if (!Locator.SettingsVM.LastFmIsConnected) return;
            try
            {
                if (LastFMScrobbler == null)
                {
                    // try to instanciate it
                    LastFMScrobbler = new LastFMScrobbler("a8eba7d40559e6f3d15e7cca1bfeaa1c", "bd9ad107438d9107296ef799703d478e");
                }

                if (!LastFMScrobbler.IsConnected)
                {
                    var pseudo = Locator.SettingsVM.LastFmUserName;
                    var pd = Locator.SettingsVM.LastFmPassword;
                    var success = await LastFMScrobbler.ConnectOperation(pseudo, pd);
                    if (!success) return;
                }

                if (LastFMScrobbler != null && LastFMScrobbler.IsConnected)
                {
                    if (string.IsNullOrEmpty(Locator.MusicPlayerVM.CurrentTrack.ArtistName) || string.IsNullOrEmpty(Locator.MusicPlayerVM.CurrentTrack.AlbumName) || string.IsNullOrEmpty(Locator.MusicPlayerVM.CurrentTrack.Name)) return;
                    LastFMScrobbler.ScrobbleTrack(Locator.MusicPlayerVM.CurrentTrack.ArtistName,
                                                        Locator.MusicPlayerVM.CurrentTrack.AlbumName,
                                                        Locator.MusicPlayerVM.CurrentTrack.Name);
                }
            }
            catch { }
        }
    }
}
