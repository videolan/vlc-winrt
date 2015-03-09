/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using VLC_WINRT_APP.Common;
using Windows.Storage;
using System;
using System.Threading.Tasks;
using VLC_WINRT_APP.Model.Video;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Model;

namespace VLC_WINRT_APP.ViewModels.VideoVM
{
    public class VideoPlayerVM : BindableBase
    {
        #region private props
        private VideoItem _currentVideo;
        #endregion

        #region private fields
        #endregion

        #region public props
        public VideoItem CurrentVideo
        {
            get { return _currentVideo; }
            set { SetProperty(ref _currentVideo, value); }
        }
        #endregion

        #region public fields
        #endregion

        #region constructors
        #endregion

        #region methods
        public async Task SetActiveVideoInfo(VideoItem media, string streamMrl = null, StorageFile file = null)
        {
            if (media == null && string.IsNullOrEmpty(streamMrl))
                return;
            // Pause the music viewmodel
            await Locator.MediaPlaybackViewModel.CleanViewModel();

            // If it's a Stream, set the CurrentVideo to null if there was a video previously played
            if (!string.IsNullOrEmpty(streamMrl))
                CurrentVideo = null;

            OnPropertyChanged("IsRunning");
            Locator.MediaPlaybackViewModel.IsPlaying = true;
            OnPropertyChanged("IsPlaying");

            Locator.MediaPlaybackViewModel.TimeTotal = TimeSpan.Zero;
            LogHelper.Log("PLAYVIDEO: Initializing playback");
            Locator.MediaPlaybackViewModel.PlayingType = PlayingType.Video;
            if (media != null)
            {
                var path = media.Token ?? media.Path;
                await Locator.MediaPlaybackViewModel.InitializePlayback(path, false, false, file);
            }
            else
                await Locator.MediaPlaybackViewModel.InitializePlayback(streamMrl, false, true);
            var em = Locator.MediaPlaybackViewModel._mediaService.MediaPlayer.eventManager();
            em.OnTrackAdded += Locator.MediaPlaybackViewModel.OnTrackAdded;
            em.OnTrackDeleted += Locator.MediaPlaybackViewModel.OnTrackDeleted;
            Locator.MediaPlaybackViewModel._mediaService.Play();
            LogHelper.Log("PLAYVIDEO: Play() method called");

            if (media != null && media.TimeWatched != null && media.TimeWatched != TimeSpan.FromSeconds(0))
                Locator.MediaPlaybackViewModel.Time = (Int64)media.TimeWatched.TotalMilliseconds;

            Locator.MediaPlaybackViewModel.SpeedRate = 100;
            await Locator.MediaPlaybackViewModel._mediaService.SetMediaTransportControlsInfo(CurrentVideo != null ? CurrentVideo.Name : "Video");
            UpdateTileHelper.UpdateMediumTileWithVideoInfo();
        }
        #endregion
    }
}
