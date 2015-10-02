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
using System.Linq;
using VLC_WinRT.Commands;
using VLC_WinRT.Helpers;
using VLC_WinRT.Model.Video;
using VLC_WinRT.Services.RunTime;
using VLC_WinRT.Utils;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml;
using libVLCX;
using System.Diagnostics;
using Windows.UI.Xaml.Media;
using VLC_WinRT.Commands.VideoPlayer;

namespace VLC_WinRT.ViewModels.VideoVM
{
    public class VideoPlayerVM : BindableBase
    {
        #region private props
        private VideoItem _currentVideo;

        private VLCSurfaceZoom currentSurfaceZoom = VLCSurfaceZoom.SURFACE_BEST_FIT;
        private bool isVideoPlayerOptionsPanelVisible;
        private bool isVideoPlayerSubtitlesSettingsVisible;
        private bool isVideoPlayerAudioTracksSettingsVisible;
        private bool isVideoPlayerVolumeSettingsVisible;
        #endregion

        #region private fields
        #endregion

        #region public props
        public VideoItem CurrentVideo
        {
            get { return _currentVideo; }
            set { SetProperty(ref _currentVideo, value); }
        }

        public VLCSurfaceZoom CurrentSurfaceZoom
        {
            get
            {
                return currentSurfaceZoom;
            }
            set
            {
                SetProperty(ref currentSurfaceZoom, value);
                ChangeSurfaceZoom(value);
            }
        }

        public bool IsVideoPlayerOptionsPanelVisible
        {
            get { return isVideoPlayerOptionsPanelVisible; }
            set { SetProperty(ref isVideoPlayerOptionsPanelVisible, value); }
        }

        public bool IsVideoPlayerSubtitlesSettingsVisible
        {
            get { return isVideoPlayerSubtitlesSettingsVisible; }
            set { SetProperty(ref isVideoPlayerSubtitlesSettingsVisible, value); }
        }

        public bool IsVideoPlayerAudioTracksSettingsVisible
        {
            get { return isVideoPlayerAudioTracksSettingsVisible; }
            set { SetProperty(ref isVideoPlayerAudioTracksSettingsVisible, value); }
        }

        public bool IsVideoPlayerVolumeSettingsVisible
        {
            get { return isVideoPlayerVolumeSettingsVisible; }
            set { SetProperty(ref isVideoPlayerVolumeSettingsVisible, value); }
        }
        
        public ActionCommand ToggleIsVideoPlayerOptionsPanelVisible { get; } = new ActionCommand(() =>
        {
            Locator.VideoVm.IsVideoPlayerOptionsPanelVisible = !Locator.VideoVm.IsVideoPlayerOptionsPanelVisible;
            Locator.VideoVm.IsVideoPlayerAudioTracksSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerSubtitlesSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerVolumeSettingsVisible = false;
        });


        public ActionCommand ToggleIsVideoPlayerSubtitlesSettingsVisible { get; } = new ActionCommand(() =>
        {
            Locator.VideoVm.IsVideoPlayerAudioTracksSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerOptionsPanelVisible = false;
            Locator.VideoVm.IsVideoPlayerVolumeSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerSubtitlesSettingsVisible = !Locator.VideoVm.IsVideoPlayerSubtitlesSettingsVisible;
        });

        public ActionCommand ToggleIsVideoPlayerAudioTracksSettingsVisible { get; } = new ActionCommand(() =>
        {
            Locator.VideoVm.IsVideoPlayerSubtitlesSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerOptionsPanelVisible = false;
            Locator.VideoVm.IsVideoPlayerVolumeSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerAudioTracksSettingsVisible = !Locator.VideoVm.IsVideoPlayerAudioTracksSettingsVisible;
        });


        public ActionCommand ToggleIsVideoPlayerVolumeSettingsVisible { get; } = new ActionCommand(() =>
        {
            Locator.VideoVm.IsVideoPlayerAudioTracksSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerSubtitlesSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerOptionsPanelVisible = false;
            Locator.VideoVm.IsVideoPlayerVolumeSettingsVisible = !Locator.VideoVm.IsVideoPlayerVolumeSettingsVisible;
        });
        #endregion

        #region public fields
        #endregion

        #region constructors
        #endregion

        #region methods
        public void OnNavigatedTo()
        {
            // If no playback was ever started, ContinueIndexing can be null
            // If we navigate back and forth to the main page, we also don't want to 
            // re-mark the task as completed.
            Locator.MediaPlaybackViewModel.ContinueIndexing = new TaskCompletionSource<bool>();
            Locator.Slideshow.IsPaused = true;
        }

        public void OnNavigatedFrom()
        {
            if (Locator.MediaPlaybackViewModel.ContinueIndexing != null && !Locator.MediaPlaybackViewModel.ContinueIndexing.Task.IsCompleted)
            {
                Locator.MediaPlaybackViewModel.ContinueIndexing.SetResult(true);
            }
            Locator.VideoVm.IsVideoPlayerAudioTracksSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerOptionsPanelVisible = false;
            Locator.VideoVm.IsVideoPlayerSubtitlesSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerVolumeSettingsVisible = false;
            Locator.Slideshow.IsPaused = false;
        }

        public async Task<bool> TryUseSubtitleFromFolder()
        {
            // Trying to get the path of the current video
            var videoPath = "";
            if (CurrentVideo.File != null)
            {
                videoPath = CurrentVideo.File.Path;
            }
            else if (!string.IsNullOrEmpty(CurrentVideo.Path))
            {
                videoPath = CurrentVideo.Path;
            }
            else return false;

            var folderPath = "";
            var fileNameWithoutExtensions = "";
            try
            {
                folderPath = System.IO.Path.GetDirectoryName(videoPath);
                fileNameWithoutExtensions = System.IO.Path.GetFileNameWithoutExtension(videoPath);
            }
            catch
            {
                return false;
            }
            try
            {
                // Since we checked Video Libraries capability and SD Card compatibility, and DLNA discovery
                // I think WinRT will let us create a StorageFolder instance of the parent folder of the file we're playing
                // Unfortunately, if the video is opened via a filepicker AND that the video is in an unusual folder, like C:/randomfolder/
                // This might now work, hence the try catch
                var storageFolderParent = await StorageFolder.GetFolderFromPathAsync(folderPath);
                // Here we need to search for a file with the same name, but with .srt or .ssa (when supported) extension
                StorageFile storageFolderParentSubtitle = null;
                storageFolderParentSubtitle = await storageFolderParent.GetFileAsync(fileNameWithoutExtensions + ".srt");
                if (storageFolderParentSubtitle != null)
                {
                    Locator.MediaPlaybackViewModel.OpenSubtitleCommand.Execute(storageFolderParentSubtitle);
                    return true;
                }
            }
            catch
            {
                // Folder is not accessible cause outside of the sandbox
                // OR
                // File doesn't exist
            }
            return false;
        }

        private void ChangeSurfaceZoom(VLCSurfaceZoom desiredZoom)
        {
            var screenWidth = App.RootPage.SwapChainPanel.ActualWidth;
            var screenHeight = App.RootPage.SwapChainPanel.ActualHeight;
            
            var vlcService = (VLCService)Locator.MediaPlaybackViewModel._mediaService;
            var videoTrack = vlcService.MediaPlayer?.media()?.tracks()?.First(x => x.type() == TrackType.Video);
            var videoHeight = videoTrack.height();
            var videoWidth = videoTrack.width();

            var sarDen = videoTrack.sarDen();
            var sarNum = videoTrack.sarNum();

            double var = 0, displayedVideoWidth;
            if (sarDen == sarNum)
            {
                // Assuming it's 1:1 pixel
                var = (float)videoWidth / videoHeight;
            }
            else
            {
                var = (videoWidth * (double)sarNum / sarDen) / videoHeight;
            }

            var screenar = (float)screenWidth / screenHeight;
            double displayedVideoHeight = 0;
            if (var > screenar)
            {
                displayedVideoHeight = screenWidth * ((float)videoHeight / videoWidth);
                displayedVideoWidth = screenWidth;
            }
            else
            {
                displayedVideoHeight = screenHeight;
                displayedVideoWidth = displayedVideoHeight * var;
            }

            double bandesNoiresVertical = screenHeight - displayedVideoHeight;
            double bandesNoiresHorizontal = screenWidth - displayedVideoWidth;

            var scaleTransform = new ScaleTransform();
            switch (desiredZoom)
            {
                case VLCSurfaceZoom.SURFACE_BEST_FIT:
                    break;
                case VLCSurfaceZoom.SURFACE_FIT_HORIZONTAL:
                    var horizontalScale = displayedVideoWidth / (displayedVideoWidth - bandesNoiresHorizontal);

                    scaleTransform.ScaleX = horizontalScale;
                    scaleTransform.ScaleY = horizontalScale;
                    scaleTransform.CenterX = screenWidth / 2;
                    scaleTransform.CenterY = screenHeight / 2;
                    App.RootPage.SwapChainPanel.RenderTransform = scaleTransform;
                    break;
                case VLCSurfaceZoom.SURFACE_FIT_VERTICAL:
                    var verticalScale = displayedVideoHeight / (displayedVideoHeight - bandesNoiresVertical);

                    scaleTransform.ScaleX = verticalScale;
                    scaleTransform.ScaleY = verticalScale;
                    scaleTransform.CenterX = screenWidth / 2;
                    scaleTransform.CenterY = screenHeight / 2;
                    App.RootPage.SwapChainPanel.RenderTransform = scaleTransform;
                    break;
                case VLCSurfaceZoom.SURFACE_FILL:
                    break;
                case VLCSurfaceZoom.SURFACE_16_9:
                    break;
                case VLCSurfaceZoom.SURFACE_4_3:
                    break;
                case VLCSurfaceZoom.SURFACE_ORIGINAL:
                    break;
                case VLCSurfaceZoom.SURFACE_CUSTOM_ZOOM:
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}
