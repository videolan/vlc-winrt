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
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using VLC_WINRT_APP.Model.Music;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using libVLCX;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Services.Interface;

namespace VLC_WINRT_APP.Services.RunTime
{
    public class VlcService : IVlcService
    {
        private readonly object _controlLock = new object();

        public VlcState CurrentState { get; private set; }
        private Task _vlcInitializeTask;
        private Player _vlcPlayer;

        public VlcService()
        {
            CurrentState = VlcState.Stopped;
        }

        public event EventHandler<VlcState> StatusChanged;

        public event EventHandler MediaEnded;
        private void UpdateStatus(VlcState status)
        {
            if (CurrentState != status)
            {
                CurrentState = status;
                if (StatusChanged != null)
                {
                    StatusChanged(this, CurrentState);
                }
            }
        }

        public void Stop()
        {
            if (CurrentState == VlcState.Paused)
            {
                Play();
            }
            DoVLCSafeAction(() =>
            {
                _vlcPlayer.Stop();
                UpdateStatus(VlcState.Stopped);
            });
        }

        private async void DoVLCSafeAction(Action a)
        {
            if (_vlcPlayer == null || _vlcInitializeTask == null)
                return;

            await _vlcInitializeTask;
            lock (_controlLock)
            {
                a();
            }
        }

        public void Seek(float position)
        {
            DoVLCSafeAction(() => _vlcPlayer.Seek(position));
        }

        public void Play()
        {
            DoVLCSafeAction(() =>
            {
                _vlcPlayer.Play();
                UpdateStatus(VlcState.Playing);
            });
        }

        public void Pause()
        {
            DoVLCSafeAction(() =>
            {
                _vlcPlayer.Pause();
                UpdateStatus(VlcState.Paused);
            });
        }

        public async Task Initialize(SwapChainPanel panel)
        {
            _vlcPlayer = new Player(panel);
            IAsyncAction init = _vlcPlayer.Initialize();
            if (init != null)
            {
                _vlcInitializeTask = init.AsTask();
            }
            _vlcPlayer.MediaEnded += _vlcPlayer_MediaEnded;
            await _vlcInitializeTask;
        }

        private void _vlcPlayer_MediaEnded()
        {
            UpdateStatus(VlcState.NotPlaying);
            var mediaEnded = MediaEnded;
            if (mediaEnded != null)
            {
                MediaEnded(this, null);
            }
        }

        public void Open(string mrl)
        {
            DoVLCSafeAction(() => { _vlcPlayer.Open(mrl); });
        }

        public void OpenSubtitle(string mrl)
        {
            DoVLCSafeAction(() => { _vlcPlayer.OpenSubtitle(mrl); });
        }

        public void SkipAhead()
        {
            TimeSpan relativeTimeSpan = TimeSpan.FromSeconds(10);
            SeekToRelativeTime(relativeTimeSpan);
        }

        public void SkipBack()
        {
            TimeSpan relativeTimeSpan = TimeSpan.FromSeconds(-10);
            SeekToRelativeTime(relativeTimeSpan);
        }

        private async void SeekToRelativeTime(TimeSpan relativeTimeSpan)
        {
            double position = await GetPosition();
            double length = await GetLength();
            TimeSpan seekTo = TimeSpan.FromMilliseconds(position * length) + relativeTimeSpan;
            double relativePosition = seekTo.TotalMilliseconds / length;
            if (relativePosition < 0.0f)
            {
                relativePosition = 0.0f;
            }
            if (relativePosition > 1.0f)
            {
                relativePosition = 1.0f;
            }
            Seek((float)relativePosition);
        }

        public void Close()
        {
            if (_vlcPlayer != null)
            {
                if (CurrentState != VlcState.Stopped)
                {
                    Stop();
                }

                lock (_controlLock)
                {
                    if (_vlcInitializeTask != null)
                    {
                        _vlcInitializeTask.Wait(20000);
                        _vlcInitializeTask = null;
                        GC.Collect();
                    }
                }

                lock (_controlLock)
                {
                    try
                    {
                        _vlcPlayer.Dispose();
                        _vlcPlayer = null;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("We ran into an exception disposing vlc instance");
                        Debug.WriteLine(ex);
                    }
                }
            }
        }

        public async Task<float> GetPosition()
        {
            float position = 0.0f;

            if (_vlcPlayer == null || _vlcInitializeTask == null)
                return position;

            await _vlcInitializeTask;
            lock (_controlLock)
            {
                {
                    if (CurrentState == VlcState.Playing
                        || CurrentState == VlcState.Paused)
                    {
                        position = _vlcPlayer.GetPosition();
                    }
                }
            }
            return position;
        }

        public async Task<long> GetLength()
        {
            long length = 0;
            if (_vlcPlayer == null || _vlcInitializeTask == null)
                return length;

            await _vlcInitializeTask;
            lock (_controlLock)
            {
                length = _vlcPlayer.GetLength();
            }
            return length;
        }

        public async Task SetSizeVideoPlayer(uint x, uint y)
        {
            if (_vlcPlayer == null || _vlcInitializeTask == null)
                return;
            await _vlcInitializeTask;

            lock (_controlLock)
            {
                _vlcPlayer.UpdateSize(x, y);
            }
        }

        public async Task<int> GetSubtitleCount()
        {
            if (_vlcPlayer == null || _vlcInitializeTask == null)
                return 0;
            await _vlcInitializeTask;
            lock (_controlLock)
            {
                return _vlcPlayer.GetSubtitleCount();
            }
        }

        public async Task<int> GetAudioTrackCount()
        {
            if (_vlcPlayer == null || _vlcInitializeTask == null)
                return 0;
            await _vlcInitializeTask;
            lock (_controlLock)
            {
                return _vlcPlayer.GetAudioTracksCount();
            }
        }

        public async Task<int> GetSubtitleDescription(IDictionary<int, string> subtitles)
        {
            if (_vlcPlayer == null || _vlcInitializeTask == null)
                return 0;
            await _vlcInitializeTask;
            lock (_controlLock)
            {
                return _vlcPlayer.GetSubtitleDescription(subtitles);
            }
        }
        public async Task<int> GetAudioTrackDescription(IDictionary<int, string> audioTracks)
        {
            if (_vlcPlayer == null || _vlcInitializeTask == null)
                return 0;
            await _vlcInitializeTask;
            lock (_controlLock)
            {
                return _vlcPlayer.GetAudioTracksDescription(audioTracks);
            }
        }

        public async Task SetSubtitleTrack(int track)
        {
            if (_vlcPlayer == null || _vlcInitializeTask == null)
                return;
            await _vlcInitializeTask;
            lock (_controlLock)
            {
                _vlcPlayer.SetSubtitleTrack(track);
            }
        }

        public async Task SetAudioTrack(int track)
        {
            if (_vlcPlayer == null || _vlcInitializeTask == null)
                return;
            await _vlcInitializeTask;
            lock (_controlLock)
            {
                _vlcPlayer.SetAudioTrack(track);
            }
        }
        public async Task SetRate(float rate)
        {
            if (_vlcPlayer == null || _vlcInitializeTask == null)
                return;
            await _vlcInitializeTask;
            lock (_controlLock)
            {
                _vlcPlayer.SetRate(rate);
            }
        }

        public async Task SetVolume(int volume)
        {
            if (_vlcPlayer == null || _vlcInitializeTask == null)
                return;
            await _vlcInitializeTask;
            lock (_controlLock)
            {
                _vlcPlayer.SetVolume(volume);
            }
        }

        public async Task<int> GetVolume()
        {
            if (_vlcPlayer == null || _vlcInitializeTask == null)
                return 0;
            await _vlcInitializeTask;
            lock (_controlLock)
            {
                int vol = _vlcPlayer.GetVolume();
                return vol;
            }
        }

        public async Task Trim()
        {
            if (_vlcPlayer == null || _vlcInitializeTask == null)
                return;
            await _vlcInitializeTask;
            lock (_controlLock)
            {
                _vlcPlayer.Trim();
            }
        }
    }
}