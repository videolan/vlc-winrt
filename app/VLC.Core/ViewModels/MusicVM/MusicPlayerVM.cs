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
using VLC.Commands.MediaPlayback;
using VLC.Commands.MusicPlayer;
using VLC.Helpers;
using VLC.Model;
using VLC.Model.Music;
using VLC.Utils;
using System.Linq;
using VLC.Helpers.MusicPlayer;

namespace VLC.ViewModels.MusicVM
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
            get { return Locator.PlaybackService.CurrentPlaybackMedia as TrackItem; }
        }

        public string CurrentMediaTitle => Locator.PlaybackService.CurrentPlaybackMedia?.Name;

        public GoToMusicPlayerPage GoToMusicPlayerPage { get; } = new GoToMusicPlayerPage();

        public ShareNowPlayingMusicCommand ShareNowPlayingMusicCommand { get; } = new ShareNowPlayingMusicCommand();

        public GoToMusicPlaylistPageCommand GoToMusicPlaylistPageCommand { get; } = new GoToMusicPlaylistPageCommand();
        public AddToPlayingPlaylist AddToPlayingPlaylist { get; } = new AddToPlayingPlaylist();

        public bool DesktopMode { get { return !Locator.SettingsVM.MediaCenterMode; } }
        #endregion

        public MusicPlayerVM()
        {
            Locator.MediaPlaybackViewModel.PlaybackService.Playback_MediaSet += Playback_MediaSet;
            Locator.MediaPlaybackViewModel.PlaybackService.Playback_MediaFileNotFound += PlaybackService_Playback_MediaFileNotFound;
        }

        private void PlaybackService_Playback_MediaFileNotFound(IMediaItem media)
        {
            if (!(media is TrackItem))
                return;

            (media as TrackItem).IsAvailable = false;
            Locator.MediaLibrary.Update(media as TrackItem);
        }

        private async void Playback_MediaSet(IMediaItem media)
        {
            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, async () =>
            {
                OnPropertyChanged(nameof(CurrentMediaTitle));
                OnPropertyChanged(nameof(CurrentTrack));

                SetCurrentArtist();
                SetCurrentAlbum();
                UpdatePlayingUI();
                await Scrobble();
                await UpdateWindows8UI();
                if (CurrentArtist != null)
                {
                    CurrentArtist.PlayCount++;
                    Locator.MediaLibrary.Update(CurrentArtist);
                }
            });
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

        private void UpdatePlayingUI()
        {
            OnPropertyChanged(nameof(CurrentTrack));
            //TileHelper.UpdateMusicTile();
        }

        public void SetCurrentArtist()
        {
            if (CurrentTrack == null) return;
            if (CurrentArtist != null && CurrentArtist.Id == CurrentTrack.ArtistId) return;
            CurrentArtist = Locator.MediaLibrary.LoadArtist(CurrentTrack.ArtistId);
        }

        public void SetCurrentAlbum()
        {
            if (CurrentTrack == null) return;
            if (CurrentArtist == null) return;
            if (CurrentAlbum != null && CurrentAlbum.Id == CurrentTrack.AlbumId) return;
            CurrentAlbum = Locator.MediaLibrary.LoadAlbum(CurrentTrack.AlbumId);
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
