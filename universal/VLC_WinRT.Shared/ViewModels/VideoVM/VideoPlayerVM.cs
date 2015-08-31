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

namespace VLC_WinRT.ViewModels.VideoVM
{
    public class VideoPlayerVM : BindableBase
    {
        #region private props
        private VideoItem _currentVideo;

        private VLCSurfaceZoom currentSurfaceZoom = VLCSurfaceZoom.SURFACE_BEST_FIT;
        private bool isVideoPlayerSettingsVisible;
        private bool isVideoPlayerSubtitlesSettingsVisible;
        private bool isVideoPlayerAudioTracksSettingsVisible;
        private bool isVideoPlayerVolumeSettingsVisible;
        private bool isVideoPlayerChaptersListVisible;
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
                ChangeSurfaceZoom(value);
                SetProperty(ref currentSurfaceZoom, value);
            }
        }

        public bool IsVideoPlayerSettingsVisible
        {
            get { return isVideoPlayerSettingsVisible; }
            set { SetProperty(ref isVideoPlayerSettingsVisible, value); }
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

        public bool IsVideoPlayerChaptersListVisible
        {
            get { return isVideoPlayerChaptersListVisible; }
            set { SetProperty(ref isVideoPlayerChaptersListVisible, value); }
        }

        public ActionCommand ToggleIsVideoPlayerSettingsVisible { get; } = new ActionCommand(() =>
        {
            Locator.VideoVm.IsVideoPlayerSettingsVisible = !Locator.VideoVm.IsVideoPlayerSettingsVisible;
            Locator.VideoVm.IsVideoPlayerAudioTracksSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerSubtitlesSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerVolumeSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerChaptersListVisible = false;
        });


        public ActionCommand ToggleIsVideoPlayerSubtitlesSettingsVisible { get; } = new ActionCommand(() =>
        {
            Locator.VideoVm.IsVideoPlayerAudioTracksSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerVolumeSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerChaptersListVisible = false;
            Locator.VideoVm.IsVideoPlayerSubtitlesSettingsVisible = !Locator.VideoVm.IsVideoPlayerSubtitlesSettingsVisible;
        });

        public ActionCommand ToggleIsVideoPlayerAudioTracksSettingsVisible { get; } = new ActionCommand(() =>
        {
            Locator.VideoVm.IsVideoPlayerSubtitlesSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerVolumeSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerChaptersListVisible = false;
            Locator.VideoVm.IsVideoPlayerAudioTracksSettingsVisible = !Locator.VideoVm.IsVideoPlayerAudioTracksSettingsVisible;
        });


        public ActionCommand ToggleIsVideoPlayerVolumeSettingsVisible { get; } = new ActionCommand(() =>
        {
            Locator.VideoVm.IsVideoPlayerAudioTracksSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerSubtitlesSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerChaptersListVisible = false;
            Locator.VideoVm.IsVideoPlayerVolumeSettingsVisible = !Locator.VideoVm.IsVideoPlayerVolumeSettingsVisible;
        });

        public ActionCommand ToggleIsVideoPlayerChaptersListVisible { get; } = new ActionCommand(() =>
        {
            Locator.VideoVm.IsVideoPlayerAudioTracksSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerSubtitlesSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerVolumeSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerChaptersListVisible = !Locator.VideoVm.IsVideoPlayerChaptersListVisible;
            Locator.MediaPlaybackViewModel.UpdateCurrentChapter();
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
            App.SplitShell.TitleBarHeight = 0;
        }

        public void OnNavigatedFrom()
        {
            if (Locator.MediaPlaybackViewModel.ContinueIndexing != null && !Locator.MediaPlaybackViewModel.ContinueIndexing.Task.IsCompleted)
            {
                Locator.MediaPlaybackViewModel.ContinueIndexing.SetResult(true);
            }
            Locator.VideoVm.IsVideoPlayerAudioTracksSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerSubtitlesSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerVolumeSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerChaptersListVisible = false;
            Locator.Slideshow.IsPaused = false;
            App.SplitShell.TitleBarHeight = AppViewHelper.TitleBarHeight;
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
            var screenWidth = Window.Current.Bounds.Width;
            var screenHeight = Window.Current.Bounds.Height;

            var dw = screenWidth;

            var vlcService = (VLCService)Locator.MediaPlaybackViewModel._mediaService;
            var videoTrack = vlcService.MediaPlayer?.media()?.tracks()?.First(x => x.type() == TrackType.Video);
            var videoHeight = videoTrack.height();
            var videoWidth = videoTrack.width();

            var sarDen = videoTrack.sarDen();
            var sarNum = videoTrack.sarNum();

            double var=0, displayedVideoWidth;
            if(sarDen == sarNum)
            {
                // Assuming it's 1:1 pixel
                var = (float)videoHeight / videoWidth;
            }
            else
            {
                
            }

            switch (desiredZoom)
            {
                case VLCSurfaceZoom.SURFACE_BEST_FIT:
                    break;
                case VLCSurfaceZoom.SURFACE_FIT_HORIZONTAL:

                    break;
                case VLCSurfaceZoom.SURFACE_FIT_VERTICAL:
                    double displayedVideoHeight = screenWidth * var;
                    double bandesNoires = screenHeight - displayedVideoHeight;
                    var dar = screenWidth / displayedVideoHeight;
                    //App.RootPage.SwapChainPanel.Height = App.RootPage.SwapChainPanel.ActualHeight * dar;
                    //App.RootPage.SwapChainPanel.Width = App.RootPage.SwapChainPanel.ActualWidth * dar;
                    Debug.WriteLine(displayedVideoHeight);
                    break;
                case VLCSurfaceZoom.SURFACE_FILL:
                    break;
                case VLCSurfaceZoom.SURFACE_16_9:
                    break;
                case VLCSurfaceZoom.SURFACE_4_3:
                    break;
                case VLCSurfaceZoom.SURFACE_ORIGINAL:
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}
