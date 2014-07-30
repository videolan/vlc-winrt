/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.AccessCache;
using Windows.UI.Popups;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Helpers.MusicLibrary.Deezer;
using VLC_WINRT_APP.Services.Interface;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MusicVM;
using VLC_WINRT_APP.Views.MusicPages;
using VLC_WINRT_APP.Views.VideoPages;
using WinRTXamlToolkit.Controls.Extensions;
#if WINDOWS_APP
using libVLCX;
using Windows.Media;
#endif

namespace VLC_WINRT_APP.Services.RunTime
{
    public class MediaService : IMediaService
    {
        private readonly VlcService _vlcService;

        private MediaElement _mediaElement;

        public MediaService(VlcService vlcService)
        {
            _vlcService = vlcService;

            _vlcService.MediaEnded += VlcPlayerService_MediaEnded;
            _vlcService.StatusChanged += VlcPlayerService_StatusChanged;
            MediaControl.IsPlaying = false;

            CoreWindow.GetForCurrentThread().Activated += ApplicationState_Activated;
        }

        public void SetMediaElement(MediaElement mediaElement)
        {
            _mediaElement = mediaElement;
        }

        /// <summary>
        /// Navigates to the Audio Player screen with the requested file a parameter.
        /// </summary>
        /// <param name="file">The file to be played.</param>
        public static async Task PlayAudioFile(StorageFile file)
        {
            if (App.ApplicationFrame.CurrentSourcePageType != typeof(MusicPlayerPage))
                App.ApplicationFrame.Navigate(typeof(MusicPlayerPage));
            MusicLibraryVM.TrackItem trackItem = new MusicLibraryVM.TrackItem();
            trackItem.Path = file.Path;
            trackItem.AlbumName = "TestAlbum";
            trackItem.ArtistName = "TestArtist";
            if (Locator.MusicPlayerVM.TrackCollection == null)
                Locator.MusicPlayerVM.TrackCollection = new ObservableCollection<MusicLibraryVM.TrackItem>();

            if (trackItem != null && !Locator.MusicPlayerVM.TrackCollection.Contains(trackItem))
            {
                Locator.MusicPlayerVM.ResetCollection();
                Locator.MusicPlayerVM.AddTrack(trackItem);
            }
            else
            {
                Locator.MusicPlayerVM.CurrentTrack =
                    Locator.MusicPlayerVM.TrackCollection.IndexOf(trackItem);
            }
            await Task.Delay(1000);

            await Locator.MusicPlayerVM.Play(file);
        }

        /// <summary>
        /// Navigates to the Video Player screen with the requested file a parameter.
        /// </summary>
        /// <param name="file">The file to be played.</param>
        public static async Task PlayVideoFile(StorageFile file)
        {
            App.ApplicationFrame.Navigate(typeof(VideoPlayerPage));
            ViewModels.VideoVM.VideoVM videoVm = new ViewModels.VideoVM.VideoVM();
            videoVm.Initialize(file);
            if (string.IsNullOrEmpty(videoVm.Token))
            {
                string token = StorageApplicationPermissions.FutureAccessList.Add(videoVm.File);
                videoVm.Token = token;
            }
            Locator.VideoVm.CurrentVideo = videoVm;
            await Task.Delay(1000);
            Locator.VideoVm.SetActiveVideoInfo(videoVm.Token);
        }

        private string _lastMrl;
        private bool _isAudioMedia;

        public void SetMediaFile(string filePath, bool isAudioMedia = true)
        {
            _vlcService.Open(filePath);
            _isAudioMedia = isAudioMedia;
            _lastMrl = filePath;
        }

        public void Play()
        {
            var position = GetPosition();
            if (_vlcService.CurrentState == VlcService.MediaPlayerState.NotPlaying && !string.IsNullOrWhiteSpace(_lastMrl))
            {
                SetPosition(0);
                SetMediaFile(_lastMrl, _isAudioMedia);
            }
            _vlcService.Play();
            RegisterMediaControls();
        }

        public void Pause()
        {
            _vlcService.Pause();
        }

        public void Stop()
        {
#if WINDOWS_PHONE_APP
            //TODO: Remove this piece of .... used for demo
            if (_vlcService.CurrentState == VlcService.MediaPlayerState.Stopped || _vlcService.CurrentState == VlcService.MediaPlayerState.NotPlaying)
                return;
#endif
            _vlcService.Stop();
            UnregisterMediaControls();
        }

        public void FastForward()
        {
            throw new NotImplementedException();
        }

        public void Rewind()
        {
            throw new NotImplementedException();
        }

        public void SkipAhead()
        {
            _vlcService.SkipAhead();
        }

        public void SkipBack()
        {
            _vlcService.SkipBack();
        }

        public float GetPosition()
        {
            return _vlcService.GetPosition().Result;
        }

        public float GetLength()
        {
            return _vlcService.GetLength().Result;
        }

        public void SetPosition(float position)
        {
            _vlcService.Seek(position);
        }

        public void SetVolume(int volume)
        {
            _vlcService.SetVolume(volume);
        }

        public int GetVolume()
        {
            return _vlcService.GetVolume().Result;
        }

        private async void MediaControl_PausePressed(object sender, object e)
        {
            await DispatchHelper.InvokeAsync(() =>
            {
                Pause();
            });
        }

        private async void MediaControl_PlayPressed(object sender, object e)
        {
            await DispatchHelper.InvokeAsync(() =>
            {
                Play();
            });
        }

        void MediaControl_PlayPauseTogglePressed(object sender, object e)
        {
            if (MediaControl.IsPlaying)
                Pause();
            else
                Play();
        }

        async void VlcPlayerService_MediaEnded(object sender, Player e)
        {
            await DispatchHelper.InvokeAsync(() =>
            {
                MediaControl.IsPlaying = false;
                UnregisterMediaControls();

                var mediaEnded = MediaEnded;
                if (mediaEnded != null)
                {
                    mediaEnded(this, new EventArgs());
                }
            });
        }

        private void VlcPlayerService_StatusChanged(object sender, VlcService.MediaPlayerState state)
        {
            MediaControl.IsPlaying = state == VlcService.MediaPlayerState.Playing;
            var statusChanged = StatusChanged;
            if (statusChanged != null)
            {
                statusChanged(this, state);
            }
        }

        private async void ApplicationState_Activated(object sender, WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == CoreWindowActivationState.Deactivated)
            {
                IsBackground = true;
                if (!IsPlaying)
                    return;

                // If we're playing a video, just pause.
                if (!_isAudioMedia)
                {
                    // TODO: Route Video Player calls through Media Service
                    _vlcService.Pause();

                    Locator.VideoVm._lastVideosRepository.Update(Locator.VideoVm.CurrentVideo);
                    return;
                }

                // Otherwise, set the MediaElement's source to the Audio File in question,
                // and play it with a volume of zero. This allows _vlcService's audio to continue
                // to play in the background. SetSource should have it's source set to a programmatically
                // generated stream of blank noise, just incase the audio file in question isn't support by
                // Windows.
                // TODO: generate blank wave file
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Void.wav"));
                var stream = await file.OpenAsync(FileAccessMode.Read);
                _mediaElement.SetSource(stream, file.ContentType);
                _mediaElement.Play();
                _mediaElement.IsLooping = true;
                _mediaElement.Volume = 0;
            }
            else
            {
                IsBackground = false;

                if (!IsPlaying)
                    return;

                // If we're playing a video, start playing again.
                if (!_isAudioMedia && IsPlaying)
                {
                    // TODO: Route Video Player calls through Media Service
                    _vlcService.Play();
                    return;
                }


                _mediaElement.Stop();
            }
        }

        private bool _isRegistered;

        private void RegisterMediaControls()
        {
            if (_isRegistered)
                return;

            MediaControl.PlayPressed += MediaControl_PlayPressed;
            MediaControl.PausePressed += MediaControl_PausePressed;
            MediaControl.PlayPauseTogglePressed += MediaControl_PlayPauseTogglePressed;
            _isRegistered = true;
        }

        private void UnregisterMediaControls()
        {
            if (!_isRegistered)
                return;

            MediaControl.PlayPressed -= MediaControl_PlayPressed;
            MediaControl.PausePressed -= MediaControl_PausePressed;
            MediaControl.PlayPauseTogglePressed -= MediaControl_PlayPauseTogglePressed;
            _isRegistered = false;
        }

        public bool IsPlaying
        {
            get { return _vlcService.CurrentState == VlcService.MediaPlayerState.Playing; }
        }

        public bool IsBackground { get; private set; }

        public event EventHandler MediaEnded;
        public event EventHandler<VlcService.MediaPlayerState> StatusChanged;

    }
}
