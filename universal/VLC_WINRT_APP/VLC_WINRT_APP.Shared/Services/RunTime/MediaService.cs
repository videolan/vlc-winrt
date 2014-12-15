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
using libVLCX;
using System.Collections.Generic;

namespace VLC_WINRT_APP.Services.RunTime
{
    public class MediaService : IMediaService
    {
        private MediaElement _mediaElement;
        public event EventHandler MediaEnded;
        public event EventHandler<MediaState> StatusChanged;

        private SystemMediaTransportControls _systemMediaTransportControls;
        public Instance Instance { get; private set; }
        public MediaPlayer MediaPlayer { get; private set; }
        public MediaService()
        {
            CoreWindow.GetForCurrentThread().Activated += ApplicationState_Activated;
        }

        public void Initialize(SwapChainPanel panel)
        {
            var param = new List<String>()
            {
                "-I",
                "dummy",
                "--no-osd",
                "--verbose=3",
                "--no-stats",
                "--avcodec-fast",
                "--no-avcodec-dr",
                String.Format("--freetype-font={0}\\segoeui.ttf", Windows.ApplicationModel.Package.Current.InstalledLocation.Path)
            };
            // So far, this NEEDS to be called from the main thread
            Instance = new Instance(param, panel);
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

        public async Task SetMediaTransportControlsInfo(string artistName, string albumName, string trackName, string albumUri)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                SystemMediaTransportControlsDisplayUpdater updater = _systemMediaTransportControls.DisplayUpdater;
                updater.Type = MediaPlaybackType.Music;
                // Music metadata.
                updater.MusicProperties.AlbumArtist = artistName;
                updater.MusicProperties.Artist = artistName;
                updater.MusicProperties.Title = trackName;

                // Set the album art thumbnail.
                // RandomAccessStreamReference is defined in Windows.Storage.Streams

                if (albumUri != null && !string.IsNullOrEmpty(albumUri))
                {
                    updater.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(albumUri));
                }

                // Update the system media transport controls.
                updater.Update();
            });
        }

        public async Task SetMediaTransportControlsInfo(string title)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
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
            Locator.MusicPlayerVM.TrackCollection.Playlist.Clear();
            Locator.MusicPlayerVM.TrackCollection.Playlist.Add(trackItem);
            await Task.Delay(1000);
            await PlayMusicHelper.PlayTrack(0);
        }

        /// <summary>
        /// Navigates to the Video Player screen with the requested file a parameter.
        /// </summary>
        /// <param name="file">The file to be played.</param>
        public static async Task PlayVideoFile(StorageFile file)
        {
            App.ApplicationFrame.Navigate(typeof(VideoPlayerPage));
            VideoItem videoVm = new VideoItem();
            await videoVm.Initialize(file);
            if (string.IsNullOrEmpty(videoVm.Token))
            {
                string token = StorageApplicationPermissions.FutureAccessList.Add(videoVm.File);
                videoVm.Token = token;
            }
            Locator.VideoVm.CurrentVideo = videoVm;
            await Task.Delay(1000);
            await Locator.VideoVm.SetActiveVideoInfo(videoVm.Token);
        }

        private bool _isAudioMedia;

        public void SetMediaFile(string filePath, bool isAudioMedia = true)
        {
            var media = new Media(Instance, filePath);
            MediaPlayer = new MediaPlayer(media);
            var em = MediaPlayer.eventManager();
            em.OnStopped += OnStopped;
            em.OnPlaying += OnPlaying;
            em.OnPaused += OnPaused;
            em.OnEndReached += OnEndReached;
            _isAudioMedia = isAudioMedia;
        }

        public void Play()
        {
            if (MediaPlayer == null)
                return; // Should we just assert/crash here?
            MediaPlayer.play();
            _systemMediaTransportControls.IsEnabled = true;
            _systemMediaTransportControls.IsPauseEnabled = true;
            _systemMediaTransportControls.IsPlayEnabled = true;
            _systemMediaTransportControls.IsNextEnabled = Locator.MusicPlayerVM.PlayingType != PlayingType.Music || Locator.MusicPlayerVM.TrackCollection.CanGoNext;
            _systemMediaTransportControls.IsPreviousEnabled = Locator.MusicPlayerVM.PlayingType != PlayingType.Music || Locator.MusicPlayerVM.TrackCollection.CanGoPrevious;
        }

        public void Pause()
        {
            MediaPlayer.pause();
        }

        public void Stop()
        {
            if (MediaPlayer != null)
                MediaPlayer.stop();
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
            MediaPlayer.setTime(MediaPlayer.time() + 10000);
        }

        public void SkipBack()
        {
            MediaPlayer.setTime(MediaPlayer.time() - 10000);
        }

        public float GetPosition()
        {
            if (MediaPlayer != null)
                return MediaPlayer.position();
            return 0.0f;
        }

        public float GetLength()
        {
            return MediaPlayer.length();
        }

        public void SetPosition(float position)
        {
            MediaPlayer.setPosition(position);
        }

        public void SetVolume(int volume)
        {
            MediaPlayer.setVolume(volume);
        }

        public void Trim()
        {
            Instance.Trim();
        }

        public int GetVolume()
        {
            return MediaPlayer.volume();
        }

        private void OnEndReached()
        {
            _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
            MediaEnded(this, null);
        }

        private void OnPaused()
        {
            _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Paused;
            StatusChanged(this, MediaState.Paused);
        }

        private void OnPlaying()
        {
            _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Playing;
            StatusChanged(this, MediaState.Playing);
        }

        private void OnStopped()
        {
            _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
            StatusChanged(this, MediaState.Stopped);
        }

        private async void ApplicationState_Activated(object sender, WindowActivatedEventArgs e)
        {
            if (MediaPlayer == null)
                return;
            if (e.WindowActivationState == CoreWindowActivationState.Deactivated)
            {
                IsBackground = true;
                if (!MediaPlayer.isPlaying())
                    return;

                // If we're playing a video, just pause.
                if (!_isAudioMedia)
                {
                    // TODO: Route Video Player calls through Media Service
                    if (!(bool)ApplicationSettingsHelper.ReadSettingsValue("ContinueVideoPlaybackInBackground"))
                        MediaPlayer.pause();

                    await Locator.VideoVm._lastVideosRepository.Update(Locator.VideoVm.CurrentVideo);
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

                if (!MediaPlayer.isPlaying() && _isAudioMedia)
                    return;

                // If we're playing a video, start playing again.
                if (!_isAudioMedia && MediaPlayer.isPlaying())
                {
                    // TODO: Route Video Player calls through Media Service
                    MediaPlayer.play();
                    return;
                }

                // Stop the MediaElement with fake audio sound
                _mediaElement.Stop();
            }
        }

        public void SetSizeVideoPlayer(uint x, uint y)
        {
            Instance.UpdateSize(x, y);
        }

        public bool IsBackground { get; private set; }
    }
}
