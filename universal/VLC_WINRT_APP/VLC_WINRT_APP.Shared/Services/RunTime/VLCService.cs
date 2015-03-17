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
using VLC_WINRT_APP.Helpers.MusicLibrary;
using VLC_WINRT_APP.Model;
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
using System.Diagnostics;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using VLC_WINRT_APP.Helpers.MusicPlayer;
using VLC_WINRT_APP.Model.Music;
#if WINDOWS_PHONE_APP
using Windows.Media.Playback;
#endif
using MediaPlayer = libVLCX.MediaPlayer;

namespace VLC_WINRT_APP.Services.RunTime
{
    public sealed class VLCService : IMediaService
    {
        public event EventHandler<MediaState> StatusChanged;
        public event TimeChanged TimeChanged;

        private SystemMediaTransportControls _systemMediaTransportControls;
        public TaskCompletionSource<bool> ContinueIndexing { get; set; }

        public TaskCompletionSource<bool> VLCInstanceReady { get; set; }

        public Instance Instance { get; private set; }
        public MediaPlayer MediaPlayer { get; private set; }
        public bool UseVlcLib { get; set; }

        public VLCService()
        {
            VLCInstanceReady = new TaskCompletionSource<bool>();
            CoreWindow.GetForCurrentThread().Activated += ApplicationState_Activated;
        }

        public void Initialize(object panel)
        {
            var swapchain = panel as SwapChainPanel;
            if (swapchain == null) throw new ArgumentNullException("VLCService needs a SwapChainpanel");
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
            VLCInstanceReady.SetResult(true);
        }


        public void SetMediaTransportControls(SystemMediaTransportControls systemMediaTransportControls)
        {
#if WINDOWS_APP
            ForceMediaTransportControls(systemMediaTransportControls);
#else
            if (BackgroundMediaPlayer.Current != null &&
                BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Playing)
            {

            }
            else
            {
                ForceMediaTransportControls(systemMediaTransportControls);
            }
#endif
        }

        void ForceMediaTransportControls(SystemMediaTransportControls systemMediaTransportControls)
        {
            _systemMediaTransportControls = systemMediaTransportControls;
            _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Closed;
            _systemMediaTransportControls.ButtonPressed += SystemMediaTransportControlsOnButtonPressed;
            _systemMediaTransportControls.IsEnabled = false;
        }

        public async Task SetMediaTransportControlsInfo(string artistName, string albumName, string trackName, string albumUri)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (_systemMediaTransportControls == null) return;
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
                if (_systemMediaTransportControls != null)
                {
                    LogHelper.Log("PLAYVIDEO: Updating SystemMediaTransportControls");
                    SystemMediaTransportControlsDisplayUpdater updater = _systemMediaTransportControls.DisplayUpdater;
                    updater.Type = MediaPlaybackType.Video;

                    //Video metadata
                    updater.VideoProperties.Title = title;
                    //TODO: add full thumbnail suport
                    updater.Thumbnail = null;
                    updater.Update();
                }
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
                    if (Locator.MediaPlaybackViewModel.PlayingType == PlayingType.Music)
                        await Locator.MediaPlaybackViewModel.PlayPrevious();
                    else
                        Locator.MediaPlaybackViewModel.SkipBack.Execute("");
                    break;
                case SystemMediaTransportControlsButton.Next:
                    if (Locator.MediaPlaybackViewModel.PlayingType == PlayingType.Music)
                        await Locator.MediaPlaybackViewModel.PlayNext();
                    else
                        Locator.MediaPlaybackViewModel.SkipAhead.Execute("");
                    break;
            }
        }

        public static async Task OpenFile(StorageFile file)
        {
            if (file == null) return;
            if (VLCFileExtensions.FileTypeHelper(file.FileType) == VLCFileExtensions.VLCFileType.Video)
            {
                var token = StorageApplicationPermissions.FutureAccessList.Add(file);
                await VLCService.PlayVideoFile(file, token);
            }
            else
            {
                await VLCService.PlayAudioFile(file);
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
            var trackItem = await GetInformationsFromMusicFile.GetTrackItemFromFile(file);
            await PlayMusicHelper.PlayTrackFromFilePicker(file, trackItem);
        }

        /// <summary>
        /// Navigates to the Video Player screen with the requested file a parameter.
        /// </summary>
        /// <param name="file">The file to be played.</param>
        /// <param name="token">Token is for files that are NOT in the sandbox, such as files taken from the filepicker from a sd card but not in the Video/Music folder.</param>
        public static async Task PlayVideoFile(StorageFile file, string token = null)
        {
            App.ApplicationFrame.Navigate(typeof(VideoPlayerPage));
            VideoItem videoVm = new VideoItem();
            await videoVm.Initialize(file);
            if (token != null) videoVm.Token = token;
            Locator.VideoVm.CurrentVideo = videoVm;
            await Locator.VideoVm.SetActiveVideoInfo(videoVm, null, file);
        }

        private bool _isAudioMedia;

        public async Task SetMediaFile(string filePath, bool isAudioMedia, bool isStream, StorageFile file = null)
        {
            LogHelper.Log("SetMediaFile: " + filePath);
            string mrl = null;
            if (file != null)
            {
                mrl = "file://" + GetToken(file);
            }
            else if (!isStream)
            {
                mrl = "file://" + await GetToken(filePath);
            }
            else
            {
                mrl = filePath;
            }
            if (Instance == null) return;
            var media = new Media(Instance, mrl);
            MediaPlayer = new MediaPlayer(media);
            LogHelper.Log("PLAYWITHVLC: MediaPlayer instance created");
            var em = MediaPlayer.eventManager();
            em.OnStopped += OnStopped;
            em.OnPlaying += OnPlaying;
            em.OnPaused += OnPaused;
            em.OnTimeChanged += TimeChanged;
            em.OnEndReached += OnEndReached;
            em.OnEncounteredError += em_OnEncounteredError;
            _isAudioMedia = isAudioMedia;
        }

        async void em_OnEncounteredError()
        {
            Debug.WriteLine("An error occurred ");
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                var md = new MessageDialog("Your media cannot be read.", "We're sorry");
                await md.ShowAsync();
            });
        }

        public async Task<string> GetToken(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return null;
            if (filePath[0] == '{' && filePath[filePath.Length - 1] == '}') return filePath;
            var file = await StorageFile.GetFileFromPathAsync(filePath);
            return StorageApplicationPermissions.FutureAccessList.Add(file);
        }

        public string GetToken(StorageFile file)
        {
            return StorageApplicationPermissions.FutureAccessList.Add(file);
        }

        public string GetAlbumUrl(string filePath)
        {
            var media = new Media(Instance, "file:///" + filePath);
            media.parse();
            if (!media.isParsed()) return "";
            var url = media.meta(MediaMeta.ArtworkURL);
            if (!string.IsNullOrEmpty(url))
            {
                return url;
            }
            return "";
        }

        public MediaProperties GetMusicProperties(string filePath)
        {
            var media = new Media(Instance, "file:///" + filePath);
            media.parse();
            if (!media.isParsed()) return null;
            var mP = new MediaProperties();
            mP.Artist = media.meta(MediaMeta.Artist);
            mP.Album = media.meta(MediaMeta.Album);
            mP.Title = media.meta(MediaMeta.Title);
            var dateTimeString = media.meta(MediaMeta.Date);
            DateTime dateTime = new DateTime();
            mP.Year = (uint)(DateTime.TryParse(dateTimeString, out dateTime) ? dateTime.Year : 0);

            var durationLong = media.duration();
            TimeSpan duration = TimeSpan.FromMilliseconds(durationLong);
            mP.Duration = duration;

            var trackNbString = media.meta(MediaMeta.TrackNumber);
            uint trackNbInt = 0;
            uint.TryParse(trackNbString, out trackNbInt);
            mP.Tracknumber = trackNbInt;
            return mP;
        }

        public TimeSpan GetDuration(string filePath)
        {
            var media = new Media(Instance, "file:///" + filePath);
            media.parse();
            if (!media.isParsed()) return TimeSpan.Zero;
            var durationLong = media.duration();
            return TimeSpan.FromMilliseconds(durationLong);
        }

        public void Play()
        {
            if (MediaPlayer == null)
                return; // Should we just assert/crash here?
            MediaPlayer.play();
            if (_systemMediaTransportControls != null)
            {
                _systemMediaTransportControls.IsEnabled = true;
                _systemMediaTransportControls.IsPauseEnabled = true;
                _systemMediaTransportControls.IsPlayEnabled = true;
                _systemMediaTransportControls.IsNextEnabled = Locator.MediaPlaybackViewModel.PlayingType != PlayingType.Music || Locator.MediaPlaybackViewModel.TrackCollection.CanGoNext;
                _systemMediaTransportControls.IsPreviousEnabled = Locator.MediaPlaybackViewModel.PlayingType != PlayingType.Music || Locator.MediaPlaybackViewModel.TrackCollection.CanGoPrevious;
            }
        }

        public void Pause()
        {
            if (MediaPlayer == null)
                return; // Should we just assert/crash here?
            MediaPlayer.pause();
        }

        public void Stop()
        {
            if (MediaPlayer != null)
                MediaPlayer.stop();
        }

        public void SetNullMediaPlayer()
        {
            MediaPlayer = null;
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
            if (MediaPlayer == null)
                return; // Should we just assert/crash here?
            MediaPlayer.setTime(MediaPlayer.time() + 10000);
        }

        public void SkipBack()
        {
            if (MediaPlayer == null)
                return; // Should we just assert/crash here?
            MediaPlayer.setTime(MediaPlayer.time() - 10000);
        }

        public float GetLength()
        {
            return MediaPlayer.length();
        }

        public void SetVolume(int volume)
        {
            MediaPlayer.setVolume(volume);
        }

        public void Trim()
        {
            if (Instance != null)
                Instance.Trim();
        }

        public int GetVolume()
        {
            return MediaPlayer.volume();
        }

        private void OnEndReached()
        {
            if (_systemMediaTransportControls != null)
                _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
        }

        private void OnPaused()
        {
            if (_systemMediaTransportControls != null)
                _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Paused;
            StatusChanged(this, MediaState.Paused);
        }

        private void OnPlaying()
        {
            if (_systemMediaTransportControls != null)
                _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Playing;
            StatusChanged(this, MediaState.Playing);
        }

        private void OnStopped()
        {
            if (_systemMediaTransportControls != null)
                _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
            StatusChanged(this, MediaState.Stopped);
        }

        private void ApplicationState_Activated(object sender, WindowActivatedEventArgs e)
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
                }
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
            }
        }

        public void SetSizeVideoPlayer(uint x, uint y)
        {
            Instance.UpdateSize(x, y);
        }

        public bool IsBackground { get; private set; }
    }
}
