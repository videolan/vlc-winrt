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
        //private Player _vlcPlayer;
        private Instance _vlcInstance;
        private MediaPlayer _vlcMP;

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
            if (_vlcMP == null)
                return;
            DoVLCSafeAction(() =>
            {
                _vlcMP.stop();
                UpdateStatus(VlcState.Stopped);
            });
        }

        private void DoVLCSafeAction(Action a)
        {
            if (_vlcInstance == null)
                return;

            a();
        }

        public void Seek(float position)
        {
            DoVLCSafeAction(() => _vlcMP.setPosition(position));
        }

        public void Play()
        {
            DoVLCSafeAction(() =>
            {
                _vlcMP.play();
                UpdateStatus(VlcState.Playing);
            });
        }

        public void Pause()
        {
            DoVLCSafeAction(() =>
            {
                _vlcMP.pause();
                UpdateStatus(VlcState.Paused);
            });
        }

        public async Task Initialize(SwapChainPanel panel)
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

            _vlcInstance = new Instance(param, panel);
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
            DoVLCSafeAction(() => 
            {
                var media = new Media(_vlcInstance, mrl);
                _vlcMP = new MediaPlayer(media);
                var em = _vlcMP.eventManager();
                em.OnEndReached += _vlcPlayer_MediaEnded;
            });
        }

        public void OpenSubtitle(string mrl)
        {
            DoVLCSafeAction(() => { _vlcMP.setSubtitleFile(mrl); });
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

        private void SeekToRelativeTime(TimeSpan relativeTimeSpan)
        {
            double position = GetPosition();
            double length = GetLength();
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
            if (_vlcInstance != null)
            {
                if (CurrentState != VlcState.Stopped)
                {
                    Stop();
                }

                lock (_controlLock)
                {
                    try
                    {
                        _vlcMP = null;
                        _vlcInstance = null;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("We ran into an exception disposing vlc instance");
                        Debug.WriteLine(ex);
                    }
                }
            }
        }

        public float GetPosition()
        {
            if (CurrentState != VlcState.Playing && CurrentState == VlcState.Paused)
                return 0.0f;
            float position = 0.0f;

            DoVLCSafeAction(() => position = _vlcMP.position());
            return position;
        }

        public long GetLength()
        {
            long length = 0;
            DoVLCSafeAction(() => length = _vlcMP.length());
            return length;
        }

        public void SetSizeVideoPlayer(uint x, uint y)
        {
            DoVLCSafeAction(() => _vlcInstance.UpdateSize(x, y));
        }

        public int GetSubtitleCount()
        {
            int subtitleCount = 0;
            DoVLCSafeAction(() => subtitleCount = _vlcMP.spuCount());
            return subtitleCount;
        }

        public int GetAudioTrackCount()
        {
            int audioTracksCount = 0;
            DoVLCSafeAction(() => audioTracksCount = _vlcMP.audioTrackCount());
            return audioTracksCount;
        }

        public int GetSubtitleDescription(IDictionary<int, string> subtitles)
        {
            int subtitleDescription = 0;
            //DoVLCSafeAction(() => subtitleDescription = _vlcMP.spuDescription());
            return subtitleDescription;
        }
        public int GetAudioTrackDescription(IDictionary<int, string> audioTracks)
        {
            int audioTrackDescription = 0;
            //DoVLCSafeAction(()=> audioTrackDescription = _vlcPlayer.GetAudioTracksDescription(audioTracks));
            return audioTrackDescription;
        }

        public void SetSubtitleTrack(int track)
        {
            DoVLCSafeAction(() => _vlcMP.setSpu(track));
        }

        public void SetAudioTrack(int track)
        {
            DoVLCSafeAction(() => _vlcMP.setAudioTrack(track));
        }

        public void SetRate(float rate)
        {
            DoVLCSafeAction(() => _vlcMP.setRate(rate));
        }

        public void SetVolume(int volume)
        {
            DoVLCSafeAction(() => _vlcMP.setVolume(volume));
        }

        public int GetVolume()
        {
            int vol = 0;
            DoVLCSafeAction(() => vol = _vlcMP.volume());
            return vol;
        }

        public void Trim()
        {
            DoVLCSafeAction(() => _vlcInstance.Trim());
        }
    }
}