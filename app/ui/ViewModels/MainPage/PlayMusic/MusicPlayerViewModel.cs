/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Autofac;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Media;
using Windows.Storage;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using VLC_WINRT.Utility.Commands;
using VLC_WINRT.Utility.Commands.MusicPlayer;
using VLC_WINRT.Utility.Helpers;
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT.Utility.Services.Interface;
using VLC_WINRT.Common;
using Windows.UI.Xaml.Navigation;

namespace VLC_WINRT.ViewModels.MainPage.PlayMusic
{
    public class MusicPlayerViewModel : MediaPlaybackViewModel
    {
        private MusicLibraryViewModel.ArtistItemViewModel _artist;
        private TrackCollectionViewModel _trackCollection;

        public MusicPlayerViewModel(HistoryService historyService, IMediaService mediaService, VlcService mediaPlayerService)
            : base(historyService, mediaService, mediaPlayerService)
        {
            _trackCollection = new TrackCollectionViewModel();
            _mediaService.MediaEnded += MediaService_MediaEnded;
        }

        protected async void MediaService_MediaEnded(object sender, EventArgs e)
        {
            if (TrackCollection.TrackCollection.Count == 0 ||
                TrackCollection.TrackCollection[TrackCollection.CurrentTrack] == TrackCollection.TrackCollection.Last())
            {
                // Playlist is finished
                await DispatchHelper.InvokeAsync(() => TrackCollection.IsRunning = false);
            }
            else
            {
                await PlayNext();
            }
        }

        public TrackCollectionViewModel TrackCollection
        {
            get
            {
                return _trackCollection;
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
            TrackCollection.IsNextPossible();
            TrackCollection.IsPreviousPossible();
            if (TrackCollection.CanGoNext)
            {
                await DispatchHelper.InvokeAsync(() => TrackCollection.CurrentTrack++);
                await Play();
            }
            else
            {
                TileUpdateManager.CreateTileUpdaterForApplication().Clear();
                await DispatchHelper.InvokeAsync(() => MediaControl.IsPlaying = false);
            }
        }

        public async Task PlayPrevious()
        {
            TrackCollection.IsNextPossible();
            TrackCollection.IsPreviousPossible();
            if (TrackCollection.CanGoPrevious)
            {
                await DispatchHelper.InvokeAsync(() => TrackCollection.CurrentTrack--);
                await Play();
            }
            else
            {
                TileUpdateManager.CreateTileUpdaterForApplication().Clear();
                await DispatchHelper.InvokeAsync(() => MediaControl.IsPlaying = false);
                MediaControl.PreviousTrackPressed -= MediaControl_PreviousTrackPressed;
            }
        }

        async void MediaControl_PreviousTrackPressed(object sender, object e)
        {
            await PlayPrevious();
        }

        async void MediaControl_NextTrackPressed(object sender, object e)
        {
            await PlayNext();
        }

        public async Task Play()
        {
            TrackCollection.IsRunning = true;
            Stop();
            var trackItem = TrackCollection.TrackCollection[TrackCollection.CurrentTrack];

            var file = await StorageFile.GetFileFromPathAsync(trackItem.Path);
            var history = new HistoryService();
            await history.RestoreHistory();
            //string token = history.Add(file, true);
            string token = await history.Add(file);

            Debug.WriteLine("Opening file: " + file.Path);

            SetActiveMusicInfo(token, trackItem);

            // Setting the info for windows 8 controls
            var resourceLoader = new ResourceLoader();
            MediaControl.IsPlaying = true;
            MediaControl.ArtistName = trackItem.ArtistName ?? resourceLoader.GetString("UnknownArtist");
            MediaControl.TrackName = trackItem.Name ?? resourceLoader.GetString("UnknownTrack");
            _timeTotal = TimeSpan.Zero;
            _elapsedTime = TimeSpan.Zero;
            try
            {
                MediaControl.AlbumArt = new Uri(Locator.MusicPlayerVM.Artist.CurrentAlbumItem.Picture);
            }
            catch
            {
                // If album cover is from the internet then it's impossible to pass it to the MediaControl
            }

            TrackCollection.IsNextPossible();
            TrackCollection.IsPreviousPossible();

            if (TrackCollection.CanGoNext)
                MediaControl.NextTrackPressed += MediaControl_NextTrackPressed;
            else
                MediaControl.NextTrackPressed -= MediaControl_NextTrackPressed;

            if (TrackCollection.CanGoPrevious)
                MediaControl.PreviousTrackPressed += MediaControl_PreviousTrackPressed;
            else
                MediaControl.PreviousTrackPressed -= MediaControl_PreviousTrackPressed;
        }

        public async Task PlayFromExplorer(StorageFile file)
        {
            TrackCollection.IsRunning = true;
            Stop();

            // Wat? This doesn't make any sense.
            var trackItem = TrackCollection.TrackCollection[TrackCollection.CurrentTrack];

            var history = new HistoryService();
            await history.RestoreHistory();
            string token = await history.Add(file);

            Debug.WriteLine("Opening file: " + file.Path);

            SetActiveMusicInfo(token, trackItem);

            // Setting the info for windows 8 controls
            MediaControl.IsPlaying = true;
            MediaControl.ArtistName = trackItem.ArtistName;
            MediaControl.TrackName = trackItem.Name;
            try
            {
                MediaControl.AlbumArt = new Uri(Locator.MusicPlayerVM.Artist.CurrentAlbumItem.Picture);
            }
            catch
            {
                // If album cover is from the internet then it's impossible to pass it to the MediaControl
            }

            TrackCollection.IsNextPossible();
            TrackCollection.IsPreviousPossible();

            if (TrackCollection.CanGoNext)
                MediaControl.NextTrackPressed += MediaControl_NextTrackPressed;
            else
                MediaControl.NextTrackPressed -= MediaControl_NextTrackPressed;

            if (TrackCollection.CanGoPrevious)
                MediaControl.PreviousTrackPressed += MediaControl_PreviousTrackPressed;
            else
                MediaControl.PreviousTrackPressed -= MediaControl_PreviousTrackPressed;
        }

        public MusicLibraryViewModel.ArtistItemViewModel Artist
        {
            get { return _artist; }
            private set { SetProperty(ref _artist, value); }
        }

        public void SetActiveMusicInfo(string token, MusicLibraryViewModel.TrackItem track)
        {
            _fileToken = token;
            _mrl = "file://" + token;
            Title = track.Name;
            Artist = Locator.MusicLibraryVM.Artist.FirstOrDefault(x => x.Name == track.ArtistName);
            if (Artist != null)
                Artist.CurrentAlbumIndex = _artist.Albums.IndexOf(_artist.Albums.FirstOrDefault(x => x.Name == track.AlbumName));
            _mediaService.SetPath(_mrl);
            OnPropertyChanged("TimeTotal");
            UpdateTileHelper.UpdateMediumTileWithMusicInfo();
            _mediaService.Play();
        }

        public override void CleanViewModel()
        {
            base.CleanViewModel();
            TrackCollection.TrackCollection.Clear();
            TrackCollection.IsRunning = false;
            Artist = null;
        }
    }
}
