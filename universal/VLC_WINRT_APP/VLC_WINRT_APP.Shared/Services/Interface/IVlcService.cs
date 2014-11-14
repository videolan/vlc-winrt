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
        float GetPosition();
        long GetLength();
        void SetSizeVideoPlayer(uint x, uint y);
        int GetSubtitleCount();
        int GetAudioTrackCount();
        int GetSubtitleDescription(IDictionary<int, string> subtitles);
        int GetAudioTrackDescription(IDictionary<int, string> audioTracks);
        void SetSubtitleTrack(int track);
        void SetAudioTrack(int track);
        void SetRate(float rate);
        void SetVolume(int volume);
        int GetVolume();
        void Trim();

    }
}
