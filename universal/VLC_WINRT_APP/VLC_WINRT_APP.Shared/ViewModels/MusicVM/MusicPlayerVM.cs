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
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Commands.Music;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Helpers.MusicLibrary.Deezer;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Services.Interface;
using VLC_WINRT_APP.Services.RunTime;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using WinRTXamlToolkit.Controls.Extensions;

namespace VLC_WINRT_APP.ViewModels.MusicVM
{
    public class MusicPlayerVM : MediaPlaybackViewModel
    {
        #region private props
        private int _currentTrack = 0;
        private bool _canGoPrevious;
        private bool _canGoNext;
        private bool _isRunning;
        private MusicLibraryVM.ArtistItem _currentPlayingArtist;
        private GoToMusicPlayerPage _goToMusicPlayerPage;
        #endregion

        #region private fields
        private ObservableCollection<MusicLibraryVM.TrackItem> _tracksCollection;

        #endregion

        #region public props

        public int Volume
        {
            get { return base._mediaService.GetVolume(); }
            set { _mediaService.SetVolume(value); }
        }

        //public SystemMediaTransportControls MediaControl;

        public MusicLibraryVM.ArtistItem CurrentPlayingArtist
        {
            get { return _currentPlayingArtist; }
            set { SetProperty(ref _currentPlayingArtist, value); }
        }

        public bool CanGoPrevious
        {
            get
            {
                return (CurrentTrack > 0);
            }
        }

        public bool CanGoNext
        {
            get { return (TrackCollection.Count != 1) && (CurrentTrack < TrackCollection.Count - 1); }
        }

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

        public int CurrentTrack
        {
            get { return _currentTrack; }
            set
            {
                SetProperty(ref _currentTrack, value);
                OnPropertyChanged("TrackCollection");
                OnPropertyChanged("CurrentTrack");
                OnPropertyChanged("CurrentTrackItem");
            }
        }

        public MusicLibraryVM.TrackItem CurrentTrackItem
        {
            get
            {
                if (TrackCollection != null && TrackCollection.Any())
                    return TrackCollection[CurrentTrack];
                else return null;
            }
        }

        public GoToMusicPlayerPage GoToMusicPlayerPage
        {
            get { return _goToMusicPlayerPage; }
            set { SetProperty(ref _goToMusicPlayerPage, value); }
        }

        #endregion

        #region public fields
        public ObservableCollection<MusicLibraryVM.TrackItem> TrackCollection
        {
            get { return _tracksCollection; }
            set { SetProperty(ref _tracksCollection, value); }
        }
        #endregion


        public MusicPlayerVM(IMediaService mediaService, VlcService mediaPlayerService)
            : base(mediaService, mediaPlayerService)
        {
            _tracksCollection = new ObservableCollection<MusicLibraryVM.TrackItem>();
            _mediaService.MediaEnded += MediaService_MediaEnded;
            GoToMusicPlayerPage = new GoToMusicPlayerPage();
        }

        protected async void MediaService_MediaEnded(object sender, EventArgs e)
        {
            if (TrackCollection.Count == 0 ||
                TrackCollection[CurrentTrack] == TrackCollection.Last() ||
                _mediaService.IsBackground)
            {
                // Playlist is finished
                App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => IsRunning = false);
            }
            else
            {
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

        public void ResetCollection()
        {
            TrackCollection.Clear();
            CurrentTrack = 0;
        }

        public void AddTrack(MusicLibraryVM.TrackItem track)
        {
            TrackCollection.Add(track);
        }

        public void AddTrack(List<MusicLibraryVM.TrackItem> tracks)
        {
            foreach (MusicLibraryVM.TrackItem track in tracks)
                TrackCollection.Add(track);
        }

        public void AddTrack(MusicLibraryVM.ArtistItem artist)
        {
            foreach (MusicLibraryVM.AlbumItem albumItem in artist.Albums)
            {
                foreach (MusicLibraryVM.TrackItem trackItem in albumItem.Tracks)
                {
                    TrackCollection.Add(trackItem);
                }
            }
        }

        public async Task PlayNext()
        {
            if (CanGoNext)
            {
                await DispatchHelper.InvokeAsync(() => CurrentTrack++);
                await Play();
            }
            else
            {
                TileUpdateManager.CreateTileUpdaterForApplication().Clear();
                //await DispatchHelper.InvokeAsync(() => MediaControl.IsPlaying = false);
            }
        }

        public async Task PlayPrevious()
        {
            if (CanGoPrevious)
            {
                await DispatchHelper.InvokeAsync(() => CurrentTrack--);
                await Play();
            }
            else
            {
                TileUpdateManager.CreateTileUpdaterForApplication().Clear();
                //await DispatchHelper.InvokeAsync(() => MediaControl.IsPlaying = false);
                //MediaControl.PreviousTrackPressed -= MediaControl_PreviousTrackPressed;
            }
        }

        async void MediaControl_PreviousTrackPressed(object sender, object e)
        {
            PlayPreviousCommand.Execute("");
        }

        async void MediaControl_NextTrackPressed(object sender, object e)
        {
            PlayNextCommand.Execute("");
        }

        public async Task Play(StorageFile fileFromExplorer = null)
        {
            Stop();
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
            {
                IsRunning = true;
                OnPropertyChanged("PlayingType");
            });
            var trackItem = TrackCollection[CurrentTrack];
            Task.Run(async () =>
            {
                StorageFile file;
                if (fileFromExplorer == null)
                {
                    file = await StorageFile.GetFileFromPathAsync(trackItem.Path);
                }
                else
                {
                    file = fileFromExplorer;
                }
                string token = StorageApplicationPermissions.FutureAccessList.Add(file);
                Debug.WriteLine("Opening file: " + file.Path);
                SetActiveMusicInfo(token, trackItem);
            });

            // Setting the info for windows 8 controls
            var resourceLoader = new ResourceLoader();
            MediaControl.IsPlaying = true;
            MediaControl.ArtistName = trackItem.ArtistName ?? resourceLoader.GetString("UnknownArtist");
            MediaControl.TrackName = trackItem.Name ?? resourceLoader.GetString("UnknownTrack");

            try
            {
                MediaControl.AlbumArt = new Uri(Locator.MusicPlayerVM.CurrentPlayingArtist.CurrentAlbumItem.Picture);
            }
            catch
            {
                // If album cover is from the internet then it's impossible to pass it to the MediaControl
            }

            if (CanGoNext)
                MediaControl.NextTrackPressed += MediaControl_NextTrackPressed;
            else
                MediaControl.NextTrackPressed -= MediaControl_NextTrackPressed;

            if (CanGoPrevious)
                MediaControl.PreviousTrackPressed += MediaControl_PreviousTrackPressed;
            else
                MediaControl.PreviousTrackPressed -= MediaControl_PreviousTrackPressed;
        }

        public async Task PlayFromExplorer(StorageFile file)
        {
            IsRunning = true;
            Stop();

            // Wat? This doesn't make any sense.
            var trackItem = TrackCollection[CurrentTrack];

            string token = StorageApplicationPermissions.FutureAccessList.Add(file);

            Debug.WriteLine("Opening file: " + file.Path);

            SetActiveMusicInfo(token, trackItem);

            // Setting the info for windows 8 controls
            //MediaControl.IsPlaying = true;
            //MediaControl.ArtistName = trackItem.ArtistName;
            //MediaControl.TrackName = trackItem.Name;
            //try
            //{
            //    //MediaControl.AlbumArt = new Uri(Locator.MusicPlayerVM.CurrentPlayingArtist.CurrentAlbumItem.Picture);
            //}
            //catch
            //{
            //    // If album cover is from the internet then it's impossible to pass it to the MediaControl
            //}

            //if (CanGoNext)
            //    MediaControl.NextTrackPressed += MediaControl_NextTrackPressed;
            //else
            //    MediaControl.NextTrackPressed -= MediaControl_NextTrackPressed;

            //if (CanGoPrevious)
            //    MediaControl.PreviousTrackPressed += MediaControl_PreviousTrackPressed;
            //else
            //    MediaControl.PreviousTrackPressed -= MediaControl_PreviousTrackPressed;
        }

        public async void SetActiveMusicInfo(string token, MusicLibraryVM.TrackItem track)
        {
            _fileToken = token;
            _mrl = "file://" + token;
            _mediaService.SetMediaFile(_mrl, isAudioMedia: true);

#if WINDOWS_APP
            UpdateTileHelper.UpdateMediumTileWithMusicInfo();
#endif
            _mediaService.Play();

            App.Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
            {
                ElapsedTime = TimeSpan.Zero;
                OnPropertyChanged("CanGoPrevious");
                OnPropertyChanged("CanGoNext");
                await Task.Delay(500);
                TimeTotal = TimeSpan.FromMilliseconds(_mediaService.GetLength());
                OnPropertyChanged("TimeTotal");
            });
        }

        public override void CleanViewModel()
        {
            base.CleanViewModel();
            TrackCollection.Clear();
            IsRunning = false;
            CurrentPlayingArtist = null;
            GC.Collect();
        }
    }
}
