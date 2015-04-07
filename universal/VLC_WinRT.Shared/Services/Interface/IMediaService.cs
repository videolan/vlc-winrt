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
using VLC_WinRT.Model;
using libVLCX;

namespace VLC_WinRT.Services.Interface
{
    public interface IMediaService
    {
        /// <summary>
        /// Initialize passes either a SwapChainPanel for VLCService
        /// or the MediaElement itself from the XAML when using MediaFoundation
        /// </summary>
        /// <param name="panel"></param>
        void Initialize(object panel);

        /// <summary>
        /// Sets the path of the file to played.
        /// </summary>
        /// <param name="fileUri">The path of the file to be played.</param>
        Task SetMediaFile(IVLCMedia media);

        void Play();
        void Play(int trackId);
        void Pause();

        void Stop();
        void SetNullMediaPlayer();
        void FastForward();
        void Rewind();
        void SkipAhead();
        void SkipBack();

        float GetLength();

        long GetTime();
        void SetTime(long desiredTime);

        float GetPosition();
        void SetPosition(float desiredPosition);
        int GetVolume();

        void SetVolume(int volume);
        void SetSpeedRate(float desiredRate);
        void SetSizeVideoPlayer(uint x, uint y);

        event EventHandler<libVLCX.MediaState> StatusChanged;
        event TimeChanged TimeChanged; 
        event EventHandler MediaFailed;
        event Action<IMediaService> OnStopped;
        event Action<long> OnLengthChanged;
        event Action OnEndReached;
        event Action<int> OnBuffering;

        TaskCompletionSource<bool> PlayerInstanceReady { get; set; } 
    }
}
