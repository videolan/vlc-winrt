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
using VLC_WinRT.Commands;
using VLC_WinRT.Helpers;
using VLC_WinRT.Model.Video;
using VLC_WinRT.Utils;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml;

namespace VLC_WinRT.ViewModels.VideoVM
{
    public class VideoPlayerVM : BindableBase
    {
        #region private props
        private VideoItem _currentVideo;

        private bool isVideoPlayerSettingsVisible;
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

        public ActionCommand ToggleIsVideoPlayerSettingsVisible { get; } = new ActionCommand(() =>
        {
            Locator.VideoVm.IsVideoPlayerSettingsVisible = !Locator.VideoVm.IsVideoPlayerSettingsVisible;
            Locator.VideoVm.IsVideoPlayerAudioTracksSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerSubtitlesSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerVolumeSettingsVisible = false;
        });


        public ActionCommand ToggleIsVideoPlayerSubtitlesSettingsVisible { get; } = new ActionCommand(() =>
        {
            Locator.VideoVm.IsVideoPlayerAudioTracksSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerVolumeSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerSubtitlesSettingsVisible = !Locator.VideoVm.IsVideoPlayerSubtitlesSettingsVisible;
        });

        public ActionCommand ToggleIsVideoPlayerAudioTracksSettingsVisible { get; } = new ActionCommand(() =>
        {
            Locator.VideoVm.IsVideoPlayerSubtitlesSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerVolumeSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerAudioTracksSettingsVisible = !Locator.VideoVm.IsVideoPlayerAudioTracksSettingsVisible;
        });


        public ActionCommand ToggleIsVideoPlayerVolumeSettingsVisible { get; } = new ActionCommand(() =>
        {
            Locator.VideoVm.IsVideoPlayerSubtitlesSettingsVisible = false;
            Locator.VideoVm.IsVideoPlayerSettingsVisible = false;
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
            if (AppViewHelper.IsFullScreen())
            {
                App.SplitShell.TitleBarHeight = 0;
            }
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
            var folderPath = System.IO.Path.GetDirectoryName(videoPath);
            var fileNameWithoutExtensions = System.IO.Path.GetFileNameWithoutExtension(videoPath);
            // Since we checked Video Libraries capability and SD Card compatibility, and DLNA discovery
            // I think WinRT will let us create a StorageFolder instance of the parent folder of the file we're playing
            // Unfortunately, if the video is opened via a filepicker AND that the video is in an unusual folder, like C:/randomfolder/
            // This might now work, hence the try catch
            var storageFolderParent = await StorageFolder.GetFolderFromPathAsync(folderPath);
            // Here we need to search for a file with the same name, but with .srt or .ssa (when supported) extension
            StorageFile storageFolderParentSubtitle = null;
            try
            {
                storageFolderParentSubtitle = await storageFolderParent.GetFileAsync(fileNameWithoutExtensions + ".srt");
            }
            catch
            {
                // File doesn't exist
                return false;
            }
            if (storageFolderParentSubtitle != null)
            {
                Locator.MediaPlaybackViewModel.OpenSubtitleCommand.Execute(storageFolderParentSubtitle);
                return true;
            }
            return false;
        }
        #endregion
    }
}
