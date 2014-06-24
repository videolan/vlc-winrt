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
using VLC_WINRT.Common;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Services.Interface;
using VLC_WINRT_APP.Services.RunTime;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace VLC_WINRT_APP.ViewModels.MusicVM
{
    public class MusicPlayerVM : MediaPlaybackViewModel
    {
        private ObservableCollection<MusicLibraryVM.TrackItem> _tracksCollection;
        private int _currentTrack = 0;
        private bool _canGoPrevious;
        private bool _canGoNext;
        private bool _isPlaying;
        private bool _isRunning;
        private MusicLibraryVM.ArtistItem _currentPlayingArtist;
        public SystemMediaTransportControls MediaControl;

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

        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                if (value != _isPlaying)
                {
                    if (value)
                    {
                        OnPlaybackStarting();
                    }
                    else
                    {
                        OnPlaybackStopped();
                    }
                    SetProperty(ref _isPlaying, value);
                }
            }
        }
        public int CurrentTrack
        {
            get { return _currentTrack; }
            set { SetProperty(ref _currentTrack, value); }
        }


        public ObservableCollection<MusicLibraryVM.TrackItem> TrackCollection
        {
            get { return _tracksCollection; }
            set { SetProperty(ref _tracksCollection, value); }
        }


        public MusicPlayerVM(IMediaService mediaService, VlcService mediaPlayerService)
            : base(mediaService, mediaPlayerService)
        {
            _tracksCollection = new ObservableCollection<MusicLibraryVM.TrackItem>();
            _mediaService.MediaEnded += MediaService_MediaEnded;
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
            await PlayPrevious();
        }

        async void MediaControl_NextTrackPressed(object sender, object e)
        {
            await PlayNext();
        }

        public async Task Play()
        {
            IsRunning = true;
            Stop();
            var trackItem = TrackCollection[CurrentTrack];

            var file = await StorageFile.GetFileFromPathAsync(trackItem.Path);
            string token = StorageApplicationPermissions.FutureAccessList.Add(file);

            Debug.WriteLine("Opening file: " + file.Path);

            SetActiveMusicInfo(token, trackItem);

            // Setting the info for windows 8 controls
            var resourceLoader = new ResourceLoader();
            //MediaControl.IsPlaying = true;
            //MediaControl.ArtistName = trackItem.ArtistName ?? resourceLoader.GetString("UnknownArtist");
            //MediaControl.TrackName = trackItem.Name ?? resourceLoader.GetString("UnknownTrack");
            _timeTotal = TimeSpan.Zero;
            _elapsedTime = TimeSpan.Zero;
            try
            {
                //MediaControl.AlbumArt = new Uri(Locator.MusicPlayerVM.CurrentPlayingArtist.CurrentAlbumItem.Picture);
            }
            catch
            {
                // If album cover is from the internet then it's impossible to pass it to the MediaControl
            }

            //if (CanGoNext)
            //    MediaControl.NextTrackPressed += MediaControl_NextTrackPressed;
            //else
            //    MediaControl.NextTrackPressed -= MediaControl_NextTrackPressed;

            //if (CanGoPrevious)
                //MediaControl.PreviousTrackPressed += MediaControl_PreviousTrackPressed;
            //else
                //MediaControl.PreviousTrackPressed -= MediaControl_PreviousTrackPressed;
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

        public void SetActiveMusicInfo(string token, MusicLibraryVM.TrackItem track)
        {
            _fileToken = token;
            _mrl = "file://" + token;
            Title = track.Name;
            CurrentPlayingArtist = Locator.MusicLibraryVM.Artist.FirstOrDefault(x => x.Name == track.ArtistName);
            if (CurrentPlayingArtist != null)
                CurrentPlayingArtist.CurrentAlbumIndex = CurrentPlayingArtist.Albums.IndexOf(CurrentPlayingArtist.Albums.FirstOrDefault(x => x.Name == track.AlbumName));
            _mediaService.SetMediaFile(_mrl, isAudioMedia: true);
            OnPropertyChanged("TimeTotal");
#if NETFX_CORE
            UpdateTileHelper.UpdateMediumTileWithMusicInfo();
#endif
            _mediaService.Play();
        }

        public override void CleanViewModel()
        {
            base.CleanViewModel();
            TrackCollection.Clear();
            IsRunning = false;
            CurrentPlayingArtist = null;
        }
    }
}
