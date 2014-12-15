/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

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
using WinRTXamlToolkit.Controls.Extensions;

namespace VLC_WINRT_APP.ViewModels.MusicVM
{
    public class MusicPlayerVM : MediaPlaybackViewModel
    {
        #region private props
        private TrackCollection _trackCollection;
        private GoToMusicPlayerPage _goToMusicPlayerPage;
        #endregion

        #region private fields
        private ShuffleCommand _shuffle;

        #endregion

        #region public props

        public TrackCollection TrackCollection
        {
            get
            {
                _trackCollection = _trackCollection ?? new TrackCollection();
                return _trackCollection;
            }
        }

        public ArtistItem CurrentArtist
        {
            get
            {
                if (CurrentTrack == null) return null;
                ArtistItem artist = Locator.MusicLibraryVM.Artists.FirstOrDefault(x => x.Id == CurrentTrack.ArtistId);
                return artist;
            }
        }

        public AlbumItem CurrentAlbum
        {
            get
            {
                if (CurrentArtist == null) return null;
                if (CurrentArtist.Albums == null || !CurrentArtist.Albums.Any())
                {
                    if (CurrentTrack != null)
                    {
                        return Locator.MusicLibraryVM.Albums.FirstOrDefault(x => x.Id == CurrentTrack.AlbumId);
                    }
                    return null;
                }
                return CurrentArtist.Albums.FirstOrDefault(x => x.Id == CurrentTrack.AlbumId);
            }
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
            _mediaService.MediaEnded += MediaService_MediaEnded;
            GoToMusicPlayerPage = new GoToMusicPlayerPage();
        }


        protected async void MediaService_MediaEnded(object sender, EventArgs e)
        {
            if (TrackCollection.Playlist.Count == 0 ||
                !TrackCollection.CanGoNext)
            {
                // Playlist is finished
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => TrackCollection.IsRunning = false);
            }
            else
            {
                base.Time = 0;
                await PlayNext();
            }
        }

        public void Pause()
        {
            _mediaService.Pause();
        }

        public void Resume()
        {
            _mediaService.Play();
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
                    await Play();
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
                await Play();
            }
            else
            {
                TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            }
        }

        public async Task Play(StorageFile fileFromExplorer = null)
        {
            Stop();
            if (CurrentTrack == null) return;
            var trackItem = CurrentTrack;
            var _ = Task.Run(async () =>
            {
                StorageFile file = null;
                if (fileFromExplorer == null)
                {
                    try
                    {
                        file = await StorageFile.GetFileFromPathAsync(trackItem.Path);
                    }
                    catch(FileNotFoundException exception)
                    {
                        App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
#if WINDOWS_PHONE_APP
                            Locator.MainVM.GoToPanelCommand.Execute(2);
#endif
                            new MessageDialog("File not found. Can't play").ShowAsyncIfPossible();
                            MusicLibraryManagement.RemoveTrackFromCollectionAndDatabase(trackItem);
                        });
                    }
                }
                else
                {
                    file = fileFromExplorer;
                }
                string token = StorageApplicationPermissions.FutureAccessList.Add(file);
                Debug.WriteLine("Opening file: " + file.Path);
                await SetActiveMusicInfo(token, trackItem);
            });
            if (trackItem == null) return;
            // Setting the info for windows 8 controls
            var resourceLoader = new ResourceLoader();
            string artistName = trackItem.ArtistName ?? resourceLoader.GetString("UnknownArtist");
            string albumName = trackItem.AlbumName;
            string trackName = trackItem.Name ?? resourceLoader.GetString("UnknownTrack");
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
            await Task.Delay(250);
        }

        public async Task SetActiveMusicInfo(string token, TrackItem track)
        {
            _fileToken = token;
            _mrl = "file://" + token;
            _mediaService.SetMediaFile(_mrl, isAudioMedia: true);
            _mediaService.Play();

//#if WINDOWS_APP
            UpdateTileHelper.UpdateMediumTileWithMusicInfo();
//#endif

            await App.Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
            {
                TrackCollection.IsRunning = true;
                OnPropertyChanged("TrackCollection");
                OnPropertyChanged("TimeTotal");
                OnPropertyChanged("PlayingType");
                OnPropertyChanged("CurrentTrack");
                OnPropertyChanged("CurrentAlbum");
                OnPropertyChanged("CurrentArtist");
                await Task.Delay(500);
                TimeTotal = TimeSpan.FromMilliseconds(_mediaService.GetLength());
            });
        }

        public override void CleanViewModel()
        {
            base.CleanViewModel();
            TrackCollection.ResetCollection();
            TrackCollection.IsRunning = false;
            GC.Collect();
        }
    }
}
