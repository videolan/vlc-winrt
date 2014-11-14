/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage.AccessCache;
using Windows.Storage.Streams;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Helpers.MusicPlayer;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.Model.Video;
using VLC_WINRT_APP.Services.Interface;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Views.MusicPages;
using VLC_WINRT_APP.Views.VideoPages;

#if WINDOWS_APP
using libVLCX;
using Windows.Media;
#endif

namespace VLC_WINRT_APP.Services.RunTime
{
    public class MediaService : IMediaService
    {
        private readonly IVlcService _vlcService;

        private MediaElement _mediaElement;

        private SystemMediaTransportControls _systemMediaTransportControls;
        public MediaService(IVlcService vlcService)
        {
            _vlcService = vlcService;

            _vlcService.MediaEnded += VlcPlayerService_MediaEnded;
            _vlcService.StatusChanged += VlcPlayerService_StatusChanged;

            CoreWindow.GetForCurrentThread().Activated += ApplicationState_Activated;
        }

        public void SetMediaElement(MediaElement mediaElement)
        {
            _mediaElement = mediaElement;
        }
        public void SetMediaTransportControls(SystemMediaTransportControls systemMediaTransportControls)
        {
            _systemMediaTransportControls = systemMediaTransportControls;
            _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Closed;
            _systemMediaTransportControls.ButtonPressed += SystemMediaTransportControlsOnButtonPressed;
            _systemMediaTransportControls.IsEnabled = false;
        }

        public void SetMediaTransportControlsInfo(string artistName, string albumName, string trackName, string albumUri)
        {
            App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                SystemMediaTransportControlsDisplayUpdater updater = _systemMediaTransportControls.DisplayUpdater;
                updater.Type = MediaPlaybackType.Music;
                // Music metadata.
                updater.MusicProperties.AlbumArtist = artistName;
                updater.MusicProperties.Artist = artistName;
                updater.MusicProperties.Title = trackName;

                // Set the album art thumbnail.
                // RandomAccessStreamReference is defined in Windows.Storage.Streams
                if (albumUri == "/Assets/GreyPylon/280x156.jpg")
                {
                }
                else
                {
                    if (albumUri != null && !string.IsNullOrEmpty(albumUri))
                    {
                        StorageFile albumCover = await StorageFile.GetFileFromPathAsync(albumUri);
                        updater.Thumbnail = RandomAccessStreamReference.CreateFromFile(albumCover);
                    }
                }

                // Update the system media transport controls.
                updater.Update();
            });
        }

        public void SetMediaTransportControlsInfo(string title)
        {
            App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SystemMediaTransportControlsDisplayUpdater updater = _systemMediaTransportControls.DisplayUpdater;
                updater.Type = MediaPlaybackType.Video;

                //Video metadata
                updater.VideoProperties.Title = title;
                //TODO: add full thumbnail suport
                updater.Thumbnail = null;
                updater.Update();
            });
        }

        private async void SystemMediaTransportControlsOnButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Pause:
                    Pause();
                    break;
                case SystemMediaTransportControlsButton.Play:
                    Play();
                    break;
                case SystemMediaTransportControlsButton.Stop:
                    Stop();
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    if (Locator.MusicPlayerVM.PlayingType == PlayingType.Music)
                        await Locator.MusicPlayerVM.PlayPrevious();
                    else
                        Locator.VideoVm.SkipBack.Execute("");
                    break;
                case SystemMediaTransportControlsButton.Next:
                    if (Locator.MusicPlayerVM.PlayingType == PlayingType.Music)
                        await Locator.MusicPlayerVM.PlayNext();
                    else
                        Locator.VideoVm.SkipAhead.Execute("");
                    break;
            }
        }

        /// <summary>
        /// Navigates to the Audio Player screen with the requested file a parameter.
        /// </summary>
        /// <param name="file">The file to be played.</param>
        public static async Task PlayAudioFile(StorageFile file)
        {
            if (App.ApplicationFrame.CurrentSourcePageType != typeof(MusicPlayerPage))
                App.ApplicationFrame.Navigate(typeof(MusicPlayerPage));
            var trackItem = new TrackItem();
            trackItem.Path = file.Path;
            trackItem.AlbumName = "Album";
            trackItem.ArtistName = "Artist";
            await Task.Delay(1000);
            await PlayMusicHelper.PlayTrack(trackItem.Id);
        }

        /// <summary>
        /// Navigates to the Video Player screen with the requested file a parameter.
        /// </summary>
        /// <param name="file">The file to be played.</param>
        public static async Task PlayVideoFile(StorageFile file)
        {
            App.ApplicationFrame.Navigate(typeof(VideoPlayerPage));
            VideoItem videoVm = new VideoItem();
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
            if (_vlcService.CurrentState == VlcState.NotPlaying && !string.IsNullOrWhiteSpace(_lastMrl))
            {
                SetPosition(0);
                SetMediaFile(_lastMrl, _isAudioMedia);
            }
            _vlcService.Play();
            _systemMediaTransportControls.IsEnabled = true;
            _systemMediaTransportControls.IsPauseEnabled = true;
            _systemMediaTransportControls.IsPlayEnabled = true;
            _systemMediaTransportControls.IsNextEnabled = Locator.MusicPlayerVM.PlayingType != PlayingType.Music || Locator.MusicPlayerVM.TrackCollection.CanGoNext;
            _systemMediaTransportControls.IsPreviousEnabled = Locator.MusicPlayerVM.PlayingType != PlayingType.Music || Locator.MusicPlayerVM.TrackCollection.CanGoPrevious;
        }

        public void Pause()
        {
            _vlcService.Pause();
        }

        public void Stop()
        {
            _vlcService.Stop();
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

        public void Trim()
        {
            _vlcService.Trim();
        }

        public int GetVolume()
        {
            return _vlcService.GetVolume().Result;
        }

        async void VlcPlayerService_MediaEnded(object sender, object e)
        {
            await DispatchHelper.InvokeAsync(() =>
            {
                _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
                var mediaEnded = MediaEnded;
                if (mediaEnded != null)
                {
                    mediaEnded(this, new EventArgs());
                }
            });
        }

        private void VlcPlayerService_StatusChanged(object sender, VlcState state)
        {
            try
            {
                switch (state)
                {
                    case VlcState.NotPlaying:
                        _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Closed;
                        break;
                    case VlcState.Paused:
                        _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Paused;
                        break;
                    case VlcState.Playing:
                        _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                        break;
                    case VlcState.Stopped:
                        _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
                        break;
                }
                var statusChanged = StatusChanged;
                if (statusChanged != null)
                {
                    statusChanged(this, state);
                }
            }
            catch { }
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
                    if (!(bool)ApplicationSettingsHelper.ReadSettingsValue("ContinueVideoPlaybackInBackground"))
                        _vlcService.Pause();

                    Locator.VideoVm._lastVideosRepository.Update(Locator.VideoVm.CurrentVideo);
                }

                // Otherwise, set the MediaElement's source to the Audio File in question,
                // and play it with a volume of zero. This allows _vlcService's audio to continue
                // to play in the background. SetSource should have it's source set to a programmatically
                // generated stream of blank noise, just incase the audio file in question isn't support by
                // Windows.
                // TODO: generate blank wave file
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///audio.wav"));
                var stream = await file.OpenAsync(FileAccessMode.Read);
                _mediaElement.SetSource(stream, file.ContentType);
                _mediaElement.Play();
                _mediaElement.IsLooping = true;
                _mediaElement.Volume = 0;
            }
            else
            {
                IsBackground = false;

                if (!IsPlaying && _isAudioMedia)
                    return;

                // If we're playing a video, start playing again.
                if (!_isAudioMedia && IsPlayingOrPaused)
                {
                    // TODO: Route Video Player calls through Media Service
                    _vlcService.Play();
                    return;
                }

                // Stop the MediaElement with fake audio sound
                _mediaElement.Stop();
            }
        }

        public bool IsPlaying
        {
            get { return _vlcService.CurrentState == VlcState.Playing; }
        }

        public bool IsPlayingOrPaused
        {
            get
            {
                return _vlcService.CurrentState == VlcState.Playing ||
                       _vlcService.CurrentState == VlcState.Paused;
            }
        }
        public bool IsBackground { get; private set; }

        public event EventHandler MediaEnded;
        public event EventHandler<VlcState> StatusChanged;

    }
}
