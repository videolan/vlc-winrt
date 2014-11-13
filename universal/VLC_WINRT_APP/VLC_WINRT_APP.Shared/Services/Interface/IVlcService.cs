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
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace VLC_WINRT_APP.Services.Interface
{
    public enum VlcState
    {
        Playing,
        NotPlaying,
        Stopped,
        Paused
    }

    public interface IVlcService
    {
        event EventHandler<VlcState> StatusChanged;
        event EventHandler MediaEnded;

        VlcState CurrentState { get; }

        Task Initialize(SwapChainPanel panel);
        void Stop();
        void Seek(float position);
        void Play();
        void Pause();
        void Open(string mrl);
        void OpenSubtitle(string mrl);
        void SkipAhead();
        void SkipBack();
        void Close();
        Task<float> GetPosition();
        Task<long> GetLength();
        Task SetSizeVideoPlayer(uint x, uint y);
        Task<int> GetSubtitleCount();
        Task<int> GetAudioTrackCount();
        Task<int> GetSubtitleDescription(IDictionary<int, string> subtitles);
        Task<int> GetAudioTrackDescription(IDictionary<int, string> audioTracks);
        Task SetSubtitleTrack(int track);
        Task SetAudioTrack(int track);
        Task SetRate(float rate);
        Task SetVolume(int volume);
        Task<int> GetVolume();
        Task Trim();

    }
}
