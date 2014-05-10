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
using VLC_WINRT.Utility.Services.RunTime;
using Windows.Storage;


namespace VLC_WINRT.Utility.Services.Interface
{
    public interface IMediaService
    {
        bool IsPlaying { get; }
        bool IsBackground { get; }

        /// <summary>
        /// Navigates to the Audio Player screen with the requested file a parameter.
        /// </summary>
        /// <param name="file">The file to be played.</param>
        Task PlayAudioFile(StorageFile file);
        /// <summary>
        /// Navigates to the Video Player screen with the requested file a parameter.
        /// </summary>
        /// <param name="file">The file to be played.</param>
        Task PlayVideoFile(StorageFile file);

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
        void SetPosition(float position);

        event EventHandler MediaEnded;
        event EventHandler<VlcService.MediaPlayerState> StatusChanged;
    }
}
