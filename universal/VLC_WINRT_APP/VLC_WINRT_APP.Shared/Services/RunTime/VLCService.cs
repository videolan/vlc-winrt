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
using VLC_WINRT_APP.Model.Stream;
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
        public event EventHandler MediaFailed;
        public event Action<IMediaService> OnStopped;
        public event Action<long> OnLengthChanged;
        public event Action OnEndReached;
        
        public TaskCompletionSource<bool> PlayerInstanceReady { get; set; }

        public Instance Instance { get; private set; }
        public MediaPlayer MediaPlayer { get; private set; }

        public VLCService()
        {
            PlayerInstanceReady = new TaskCompletionSource<bool>();
            CoreWindow.GetForCurrentThread().Activated += ApplicationState_Activated;
        }

        public void Initialize(object panel)
        {
            var swapchain = panel as SwapChainPanel;
            if (swapchain == null) throw new ArgumentNullException("panel", "VLCService needs a SwapChainpanel");
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
            Instance = new Instance(param, swapchain);
            PlayerInstanceReady.SetResult(true);
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
            await PlayMusicHelper.PlayTrackFromFilePicker(trackItem);
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
            if (token != null)
                videoVm.Token = token;
            Locator.VideoVm.CurrentVideo = videoVm;
            await Locator.MediaPlaybackViewModel.SetMedia(videoVm, false);
        }

        private bool _isAudioMedia;

        public async Task SetMediaFile(IVLCMedia media)
        {
            LogHelper.Log("SetMediaFile: " + media.Path);
            string mrl = null;
            if (media is StreamMedia)
            {
                mrl = media.Path;
            }
            else
            {
                if (media.File != null)
                {
                    mrl = "file://" + GetToken(media.File);
                }
                else
                {
                    mrl = "file://" + await GetToken(media.Path);
                }
            }

            if (Instance == null) return;
            var mediaVLC = new Media(Instance, mrl);
            MediaPlayer = new MediaPlayer(mediaVLC);
            LogHelper.Log("PLAYWITHVLC: MediaPlayer instance created");
            var em = MediaPlayer.eventManager();
            em.OnStopped += EmOnOnStopped;
            em.OnPlaying += OnPlaying;
            em.OnPaused += OnPaused;
            if (TimeChanged != null)
                em.OnTimeChanged += TimeChanged;
            em.OnEndReached += EmOnOnEndReached;
            em.OnEncounteredError += em_OnEncounteredError;
            em.OnLengthChanged += em_OnLengthChanged;
            // todo: is there another way? sure there is.
            _isAudioMedia = media is TrackItem;
        }

        void em_OnLengthChanged(long __param0)
        {
            if (OnLengthChanged != null)
                OnLengthChanged(__param0);
        }

        private void EmOnOnEndReached()
        {
            if (OnEndReached != null)
                OnEndReached();
        }

        private void EmOnOnStopped()
        {
            if (OnStopped != null)
                OnStopped(this);
        }

        void em_OnEncounteredError()
        {
            Debug.WriteLine("An error occurred ");
            if (MediaFailed != null)
            {
                MediaFailed(this, new EventArgs());
            }
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
            return MediaPlayer == null ? 0 : MediaPlayer.length();
        }

        public long GetTime()
        {
            return MediaPlayer == null ? 0 : MediaPlayer.time();
        }

        public void SetTime(long desiredTime)
        {
            if (MediaPlayer == null) return;
            MediaPlayer.setTime(desiredTime);
        }

        public float GetPosition()
        {
            return MediaPlayer == null ? 0.0f : MediaPlayer.position();
        }

        public void SetPosition(float desiredPosition)
        {
            if (MediaPlayer == null) return;
            MediaPlayer.setPosition(desiredPosition);
        }

        public void SetVolume(int volume)
        {
            MediaPlayer.setVolume(volume);
        }


        public int GetVolume()
        {
            return MediaPlayer.volume();
        }

        public void SetSpeedRate(float desiredRate)
        {
            if (MediaPlayer == null) return;
            MediaPlayer.setRate(desiredRate);
        }

        public void Trim()
        {
            if (Instance != null)
                Instance.Trim();
        }

        private void OnPaused()
        {
            StatusChanged(this, MediaState.Paused);
        }

        private void OnPlaying()
        {
            StatusChanged(this, MediaState.Playing);
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
