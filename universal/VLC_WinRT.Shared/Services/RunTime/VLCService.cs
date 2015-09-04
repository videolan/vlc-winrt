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
using Windows.Storage.AccessCache;
using VLC_WinRT.Helpers;
using VLC_WinRT.Model;
using VLC_WinRT.Services.Interface;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Diagnostics;
using VLC_WinRT.Model.Music;
using VLC_WinRT.Model.Stream;
using libVLCX;
using MediaPlayer = libVLCX.MediaPlayer;
using VLC_WinRT.ViewModels;

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
            var param = new List<string>
            {
                "-I",
                "dummy",
                "--no-osd",
                "--verbose=3",
                "--no-stats",
                "--avcodec-fast",
                string.Format("--freetype-font={0}\\segoeui.ttf",Windows.ApplicationModel.Package.Current.InstalledLocation.Path)
            };

            // So far, this NEEDS to be called from the main thread
            try
            {
                Instance = new Instance(param, swapchain);
            }
            catch (Exception e)
            {
                ExceptionHelper.CreateMemorizedException("VLC Service : Can't create VLC Instance", e);
                ToastHelper.Basic("Can't start VLC Player");
            }
            if (Instance != null)
            {
                PlayerInstanceReady.SetResult(true);
            }
        }

        public async Task SetMediaFile(IVLCMedia media)
        {
            var mrl_fromType = media.GetMrlAndFromType();
            LogHelper.Log("SetMRL: " + mrl_fromType.Item2);
            await PlayerInstanceReady.Task;
            if (Instance == null) return;
            var mediaVLC = new Media(Instance, mrl_fromType.Item2, mrl_fromType.Item1);
            // Hardware decoding
            mediaVLC.addOption(!Locator.SettingsVM.HardwareAccelerationEnabled ? ":avcodec-hw=none" : ":avcodec-hw=d3d11va");

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
            Debug.WriteLine("VLCService: An error occurred ");
            MediaFailed?.Invoke(this, new EventArgs());
        }

        public void SetAudioDelay(long delay)
        {
            MediaPlayer.setDelay(delay);
        }

        public void SetSpuDelay(long delay)
        {
            MediaPlayer.setSpuDelay(delay);
        }

        public async Task<string> GetToken(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return null;
            if (filePath[0] == '{' && filePath[filePath.Length - 1] == '}') return filePath;
            var file = await StorageFile.GetFileFromPathAsync(filePath);
            return StorageApplicationPermissions.FutureAccessList.Add(file);
        }

        public Media GetMediaFromPath(string filePath)
        {
            if (Instance == null) return null;
            if (string.IsNullOrEmpty(filePath))
                return null;
            return new Media(Instance, filePath, FromType.FromPath);
        }

        public string GetAlbumUrl(Media media)
        {
            if (media == null) return null;
            if (!media.isParsed())
                media.parse();
            if (!media.isParsed())
                return null;
            var url = media.meta(MediaMeta.ArtworkURL);
            if (!string.IsNullOrEmpty(url))
                return url;
            return null;
        }

        public MediaProperties GetVideoProperties(Media media)
        {
            if (media == null) return null;
            if (!media.isParsed())
                media.parse();
            if (!media.isParsed())
                return null;
            var mP = new MediaProperties();
            mP.Title = media.meta(MediaMeta.Title);

            var showName = media.meta(MediaMeta.ShowName);
            if(string.IsNullOrEmpty(showName))
            {
                showName = media.meta(MediaMeta.Artist);
            }
            if (!string.IsNullOrEmpty(showName))
            {
                mP.ShowTitle = showName;
            }

            var episodeString = media.meta(MediaMeta.Episode);
            if(string.IsNullOrEmpty(episodeString))
            {
                episodeString = media.meta(MediaMeta.TrackNumber);
            }
            var episode = 0;
            if(!string.IsNullOrEmpty(episodeString) && int.TryParse(episodeString, out episode))
            {
                mP.Episode = episode;
            }

            var episodesTotal = 0;
            var episodesTotalString = media.meta(MediaMeta.TrackTotal);
            if(!string.IsNullOrEmpty(episodesTotalString) && int.TryParse(episodesTotalString, out episodesTotal))
            {
                mP.Episodes = episodesTotal;
            }
            return mP;
        }

        public MediaProperties GetMusicProperties(Media media)
        {
            if (media == null) return null;
            if (!media.isParsed())
                media.parse();
            if (!media.isParsed())
                return null;
            var mP = new MediaProperties();
            mP.AlbumArtist = media.meta(MediaMeta.AlbumArtist);
            mP.Artist = media.meta(MediaMeta.Artist);
            mP.Album = media.meta(MediaMeta.Album);
            mP.Title = media.meta(MediaMeta.Title);
            var yearString = media.meta(MediaMeta.Date);
            var year = 0;
            if(int.TryParse(yearString, out year))
            {
                mP.Year = year;
            }

            var durationLong = media.duration();
            TimeSpan duration = TimeSpan.FromMilliseconds(durationLong);
            mP.Duration = duration;

            var trackNbString = media.meta(MediaMeta.TrackNumber);
            uint trackNbInt = 0;
            uint.TryParse(trackNbString, out trackNbInt);
            mP.Tracknumber = trackNbInt;

            var discNb = media.meta(MediaMeta.DiscNumber);
            int discNbInt = 1;
            int.TryParse(discNb, out discNbInt);
            mP.DiscNumber = discNbInt;

            var genre = media.meta(MediaMeta.Genre);
            mP.Genre = genre;
            return mP;
        }

        public TimeSpan GetDuration(Media media)
        {
            if (media == null) return TimeSpan.Zero;
            media.parse();
            if (!media.isParsed())
                return TimeSpan.Zero;
            var durationLong = media.duration();
            return TimeSpan.FromMilliseconds(durationLong);
        }

        public void Play()
        {
            MediaPlayer?.play();
        }

        public void Play(int trackId)
        {
            throw new NotImplementedException();
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
            Instance?.UpdateSize(x, y);
        }
    }
}
