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
using VLC_WINRT_APP.Services.RunTime;

namespace VLC_WINRT_APP.Services.Interface
{
    public interface IMediaService
    {
        bool IsPlaying { get; }
        bool IsBackground { get; }

        Task SetMediaTransportControlsInfo(string artistName, string albumName, string trackName, string albumUri);

        Task SetMediaTransportControlsInfo(string title);
        /// <summary>
        /// Sets the path of the file to played.
        /// </summary>
        /// <param name="fileUri">The path of the file to be played.</param>
        void SetMediaFile(string filePath, bool isAudioMedia = true);

        void Play();
        void Pause();
        void Stop();
        void FastForward();
        void Rewind();
        void SkipAhead();
        void SkipBack();

        float GetPosition();
        float GetLength();
        void SetPosition(float position);

        int GetVolume();

        void SetVolume(int volume);

        void Trim();

        event EventHandler MediaEnded;
        event EventHandler<VlcState> StatusChanged;
    }
}
