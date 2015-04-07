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
using VLC_WinRT.Helpers;
using VLC_WinRT.Helpers.MusicLibrary;
using VLC_WinRT.Model;
using VLC_WinRT.Model.Video;
using VLC_WinRT.Services.Interface;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Views.MusicPages;
using VLC_WinRT.Views.VideoPages;
using libVLCX;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using VLC_WinRT.Helpers.MusicPlayer;
using VLC_WinRT.Model.Music;
using VLC_WinRT.Model.Stream;
using MediaPlayer = libVLCX.MediaPlayer;

namespace VLC_WinRT.Services.RunTime
{
    public sealed class VLCService : IMediaService
    {
        public event EventHandler<MediaState> StatusChanged;
        public event TimeChanged TimeChanged;
        public event EventHandler MediaFailed;
        public event Action<IMediaService> OnStopped;
        public event Action<long> OnLengthChanged;
        public event Action OnEndReached;
        public event Action<int> OnBuffering;

        public TaskCompletionSource<bool> PlayerInstanceReady { get; set; }

        public Instance Instance { get; private set; }
        public MediaPlayer MediaPlayer { get; private set; }

        public VLCService()
        {
            PlayerInstanceReady = new TaskCompletionSource<bool>();
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
            em.OnBuffering += EmOnOnBuffering;
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

        private void EmOnOnBuffering(float param0)
        {
            OnBuffering?.Invoke((int)param0);
        }

        void em_OnLengthChanged(long __param0)
        {
            OnLengthChanged?.Invoke(__param0);
        }

        private void EmOnOnEndReached()
        {
            OnEndReached?.Invoke();
        }

        private void EmOnOnStopped()
        {
            OnStopped?.Invoke(this);
        }

        void em_OnEncounteredError()
        {
            Debug.WriteLine("An error occurred ");
            MediaFailed?.Invoke(this, new EventArgs());
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
            MediaPlayer?.play();
        }

        public void Pause()
        {
            MediaPlayer?.pause();
        }

        public void Stop()
        {
            MediaPlayer?.stop();
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
            MediaPlayer?.setTime(MediaPlayer.time() + 10000);
        }

        public void SkipBack()
        {
            MediaPlayer?.setTime(MediaPlayer.time() - 10000);
        }

        public float GetLength()
        {
            return MediaPlayer?.length() ?? 0;
        }

        public long GetTime()
        {
            return MediaPlayer?.time() ?? 0;
        }

        public void SetTime(long desiredTime)
        {
            MediaPlayer?.setTime(desiredTime);
        }

        public float GetPosition()
        {
            return MediaPlayer?.position() ?? 0.0f;
        }

        public void SetPosition(float desiredPosition)
        {
            MediaPlayer?.setPosition(desiredPosition);
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
            MediaPlayer?.setRate(desiredRate);
        }

        public void Trim()
        {
            Instance?.Trim();
        }

        private void OnPaused()
        {
            StatusChanged(this, MediaState.Paused);
        }

        private void OnPlaying()
        {
            StatusChanged(this, MediaState.Playing);
        }


        public void SetSizeVideoPlayer(uint x, uint y)
        {
            Instance.UpdateSize(x, y);
        }
    }
}
