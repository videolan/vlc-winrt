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
using System.Linq;
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
                if (Locator.MediaPlaybackViewModel.PlaybackService.CurrentMedia == -1
                    || Locator.MediaPlaybackViewModel.PlaybackService.CurrentMedia == Locator.MediaPlaybackViewModel.PlaybackService.Playlist.Count)
                    return null;
                if (Locator.MediaPlaybackViewModel.PlaybackService.CurrentMedia > Locator.MediaPlaybackViewModel.PlaybackService.Playlist.Count)
                {
                    Locator.MediaPlaybackViewModel.PlaybackService.SetCurrentMediaPosition(0); return null;
                }
                var media = Locator.MediaPlaybackViewModel.CurrentMedia;
                OnPropertyChanged(nameof(CurrentMediaTitle));
                return (media is TrackItem) ? (TrackItem)media : null;
            }
        }

        public string CurrentMediaTitle => (CurrentTrack == null) ? Locator.MediaPlaybackViewModel.CurrentMedia?.Name : CurrentTrack.Name;

        public GoToMusicPlayerPage GoToMusicPlayerPage { get; } = new GoToMusicPlayerPage();

        public ShuffleCommand Shuffle { get; } = new ShuffleCommand();

        public ShareNowPlayingMusicCommand ShareNowPlayingMusicCommand { get; } = new ShareNowPlayingMusicCommand();

        public GoToMusicPlaylistPageCommand GoToMusicPlaylistPageCommand { get; } = new GoToMusicPlaylistPageCommand();
        public AddToPlayingPlaylist AddToPlayingPlaylist { get; } = new AddToPlayingPlaylist();
        public Visibility IsMiniPlayerVisible
        {
            get
            {
                if (Locator.MediaPlaybackViewModel.PlaybackService.IsRunning &&
                    Locator.MediaPlaybackViewModel.PlaybackService.PlayingType == PlayingType.Music &&
                    (Locator.NavigationService.CurrentPage != VLCPage.CurrentPlaylistPage &&
                     Locator.NavigationService.CurrentPage != VLCPage.MusicPlayerPage &&
                     Locator.NavigationService.CurrentPage != VLCPage.VideoPlayerPage &&
                     Locator.NavigationService.CurrentPage != VLCPage.MiniPlayerView))
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }
        #endregion

        public MusicPlayerVM()
        {
            Locator.NavigationService.ViewNavigated += ViewNavigated;
            Locator.MediaPlaybackViewModel.PlaybackService.Playback_MediaSet += Playback_MediaSet;
            Locator.MediaPlaybackViewModel.PlaybackService.Playback_MediaFileNotFound += PlaybackService_Playback_MediaFileNotFound;
        }

        private async void PlaybackService_Playback_MediaFileNotFound(IMediaItem media)
        {
            if (!(media is TrackItem))
                return;

            await Locator.MediaLibrary.RemoveTrackFromCollectionAndDatabase(media as TrackItem);
        }

        private async void Playback_MediaSet(IMediaItem media)
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                OnPropertyChanged(nameof(IsMiniPlayerVisible));
                OnPropertyChanged(nameof(CurrentTrack));
            });

            if (!(media is TrackItem))
            {
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
                {
                    CurrentAlbum = null;
                    CurrentArtist = null;
                });
                return;
            }

            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await SetCurrentArtist();
                await SetCurrentAlbum();
                await UpdatePlayingUI();
                await Scrobble();
#if WINDOWS_PHONE_APP
#else
                await UpdateWindows8UI();
#endif
                if (CurrentArtist != null)
                {
                    CurrentArtist.PlayCount++;
                    await Locator.MediaLibrary.Update(CurrentArtist);
                }
            });
        }

        private void ViewNavigated(object sender, VLCPage p)
        {
            OnPropertyChanged(nameof(IsMiniPlayerVisible));
        }

        public async Task UpdateWindows8UI()
        {
            string artistName = CurrentTrack?.ArtistName ?? Strings.UnknownArtist;
            string albumName = CurrentTrack?.AlbumName ?? Strings.UnknownAlbum;
            string trackName = CurrentTrack?.Name ?? Strings.UnknownTrack;
            var picture = Locator.MusicPlayerVM.CurrentAlbum != null ? Locator.MusicPlayerVM.CurrentAlbum.AlbumCoverFullUri : null;

            await Locator.MediaPlaybackViewModel.SetMediaTransportControlsInfo(artistName, albumName, trackName, picture);

            if (Locator.SettingsVM.NotificationOnNewSong && Locator.MainVM.IsBackground)
            {
                ToastHelper.ToastImageAndText04($"VLC {Strings.Dash} {Strings.NowPlaying}", trackName, albumName, artistName, (Locator.MusicPlayerVM.CurrentAlbum == null) ? null : Locator.MusicPlayerVM.CurrentAlbum.AlbumCoverFullUri ?? null, "newsong", "", "musicplayerview");
            }
        }

        public async Task UpdatePlayingUI()
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                Locator.MediaPlaybackViewModel.PlaybackService.IsRunning = true;
                OnPropertyChanged(nameof(CurrentTrack));
#if WINDOWS_UWP
                TileHelper.UpdateMusicTile();
#else
                UpdateTileHelper.UpdateMediumTileWithMusicInfo();
#endif
            });
        }

        public async Task SetCurrentArtist()
        {
            if (CurrentTrack == null) return;
            if (CurrentArtist != null && CurrentArtist.Id == CurrentTrack.ArtistId) return;
            CurrentArtist = await Locator.MediaLibrary.LoadArtist(CurrentTrack.ArtistId);
        }

        public async Task SetCurrentAlbum()
        {
            if (CurrentTrack == null) return;
            if (CurrentArtist == null) return;
            if (CurrentAlbum != null && CurrentAlbum.Id == CurrentTrack.AlbumId) return;
            CurrentAlbum = await Locator.MediaLibrary.LoadAlbum(CurrentTrack.AlbumId);
        }


        public async Task Scrobble()
        {
            if (!Locator.SettingsVM.LastFmIsConnected) return;
            try
            {
                if (LastFMScrobbler == null)
                {
                    // try to instanciate it
                    LastFMScrobbler = new LastFMScrobbler(App.ApiKeyLastFm, App.ApiSecretLastFm);
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
