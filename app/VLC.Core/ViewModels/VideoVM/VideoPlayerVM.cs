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
using System.Threading.Tasks;
using System.Linq;
using VLC.Commands;
using VLC.Helpers;
using VLC.Model.Video;
using VLC.Services.RunTime;
using VLC.Utils;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml;
using libVLCX;
using System.Diagnostics;
using Windows.Graphics.Display;
using Windows.UI.Xaml.Media;
using VLC.Commands.VideoPlayer;
using VLC.Model;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using VLC.MediaMetaFetcher.Fetchers;
using Projection = libVLCX.Projection;

namespace VLC.ViewModels.VideoVM
{
    public class VideoPlayerVM : BindableBase
    {
        #region events
        public event EventHandler<bool> PlayerControlVisibilityChangeRequested;
        #endregion
        #region private props
        private VideoItem _currentVideo;

        private VLCSurfaceZoom currentSurfaceZoom = VLCSurfaceZoom.SURFACE_BEST_FIT;
        private bool isVideoPlayerOptionsPanelVisible;
        private List<VLCSurfaceZoom> zooms;

        private bool _isLoadingSubtitle;
        private string _loadingSubtitleText;
        public bool PlayerControlVisibility { get; private set; } = true;
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

        public ActionCommand ToggleIsVideoPlayerOptionsPanelVisible { get; private set; } = new ActionCommand(() =>
        {
            Locator.NavigationService.Go(VLCPage.VideoPlayerOptionsPanel);
            Locator.VideoPlayerVm.IsVideoPlayerOptionsPanelVisible = false;
        });

        public SurfaceZoomToggleCommand SurfaceZoomToggleCommand { get; private set; } = new SurfaceZoomToggleCommand();

        public InitPiPCommand InitPiPCommand { get; private set; } = new InitPiPCommand();

        public DownloadSubtitleCommand DownloadSubtitleCommand { get; private set; } = new DownloadSubtitleCommand();

        public ShowSubtitlesSettingsCommand ShowSubtitlesSettingsCommand { get; private set; } = new ShowSubtitlesSettingsCommand();
        public ShowAudioTracksSettingsCommand ShowAudioTracksSettingsCommand { get; private set; } = new ShowAudioTracksSettingsCommand();
        public ShowChaptersSettingsCommand ShowChaptersSettingsCommand { get; private set; } = new ShowChaptersSettingsCommand();
        public bool IsLoadingSubtitle { get { return _isLoadingSubtitle; } set { SetProperty(ref _isLoadingSubtitle, value); } }
        public string LoadingSubtitleText { get { return _loadingSubtitleText; } set { SetProperty(ref _loadingSubtitleText, value); } }
        public ActionCommand PlayPauseCommand { get; } = new ActionCommand(() => Locator.MediaPlaybackViewModel.PlaybackService.Pause());
        public ActionCommand GoBackCommand { get; } = new ActionCommand(() => Locator.MediaPlaybackViewModel.GoBack.Execute(null));
        public ActionCommand IncreaseVolumeCommand { get; } = new ActionCommand(() => Locator.MediaPlaybackViewModel.ChangeVolumeCommand.Execute("higher"));
        public ActionCommand DecreaseVolumeCommand { get; } = new ActionCommand(() => Locator.MediaPlaybackViewModel.ChangeVolumeCommand.Execute("lower"));
        public ActionCommand MuteCommand { get; } = new ActionCommand(() => Locator.MediaPlaybackViewModel.ChangeVolumeCommand.Execute("mute"));
        public ActionCommand ToggleFullscreenCommand { get; } = new ActionCommand(AppViewHelper.ToggleFullscreen);
        public ActionCommand ZoomCommand { get; } = new ActionCommand(() => Locator.VideoPlayerVm.ToggleIsVideoPlayerOptionsPanelVisible.Execute(null));
        public ActionCommand IncreaseSpeedCommand { get; } = new ActionCommand(() => Locator.MediaPlaybackViewModel.ChangePlaybackSpeedRateCommand.Execute("faster"));
        public ActionCommand DecreaseSpeedCommand { get; } = new ActionCommand(() => Locator.MediaPlaybackViewModel.ChangePlaybackSpeedRateCommand.Execute("slower"));
        public ActionCommand ResetSpeedCommand { get; } = new ActionCommand(() => Locator.MediaPlaybackViewModel.ChangePlaybackSpeedRateCommand.Execute("reset"));

        #endregion

        #region public fields

        public List<VLCSurfaceZoom> Zooms
        {
            get
            {
                if (zooms == null || !zooms.Any())
                {
                    zooms = new List<VLCSurfaceZoom>
                    {
                        VLCSurfaceZoom.SURFACE_BEST_FIT,
                        VLCSurfaceZoom.SURFACE_FIT_SCREEN,
                        VLCSurfaceZoom.SURFACE_FILL,
                        VLCSurfaceZoom.SURFACE_16_9,
                        VLCSurfaceZoom.SURFACE_4_3,
                        VLCSurfaceZoom.SURFACE_ORIGINAL
                    };
                }
                return zooms;
            }
        }
        #endregion

        #region constructors
        public VideoPlayerVM()
        {
        }

        private void PlaybackService_Playback_MediaFileNotFound(IMediaItem media)
        {
            if (!(media is VideoItem))
                return;

            (media as VideoItem).IsAvailable = false;
            Locator.MediaLibrary.UpdateVideo(media as VideoItem);
        }
        #endregion

        #region methods
        public void OnNavigatedTo()
        {
            // If no playback was ever started, ContinueIndexing can be null
            // If we navigate back and forth to the main page, we also don't want to 
            // re-mark the task as completed.
            PlayerControlVisibility = true;
            Locator.MediaLibrary.ContinueIndexing = new TaskCompletionSource<bool>();
            DeviceHelper.PrivateDisplayCall(true);
            Locator.Slideshow.IsPaused = true;
            if (Locator.SettingsVM.ForceLandscape && DeviceHelper.GetDeviceType() != DeviceTypeEnum.Xbox)
            {
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
            }

            if (Locator.PlaybackService.CurrentPlaybackMedia is VideoItem)
                Task.Run(async () => await UpdateCurrentVideo(Locator.PlaybackService.CurrentPlaybackMedia as VideoItem));

            Locator.MediaPlaybackViewModel.PlaybackService.Playback_MediaSet += PlaybackService_Playback_MediaSet;
            Locator.MediaPlaybackViewModel.PlaybackService.Playback_MediaFileNotFound += PlaybackService_Playback_MediaFileNotFound;
            
        }

        public void OnNavigatedFrom()
        {
            if (Locator.MediaLibrary.ContinueIndexing != null && !Locator.MediaLibrary.ContinueIndexing.Task.IsCompleted)
            {
                Locator.MediaLibrary.ContinueIndexing.TrySetResult(true);
            }
            Locator.VideoPlayerVm.IsVideoPlayerOptionsPanelVisible = false;
            Locator.Slideshow.IsPaused = false;
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.None;
            DeviceHelper.PrivateDisplayCall(false);
            LoadingSubtitleText = string.Empty;

            Locator.MediaPlaybackViewModel.PlaybackService.Playback_MediaSet -= PlaybackService_Playback_MediaSet;
            Locator.MediaPlaybackViewModel.PlaybackService.Playback_MediaFileNotFound -= PlaybackService_Playback_MediaFileNotFound;
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
                var storageFolderParentSubtitle = await storageFolderParent.TryGetItemAsync(fileNameWithoutExtensions + ".srt");
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

        public void ChangeSurfaceZoom(VLCSurfaceZoom desiredZoom)
        {
            var playbackService = Locator.PlaybackService;
            var screenWidth = App.RootPage.SwapChainPanel.ActualWidth;
            var screenHeight = App.RootPage.SwapChainPanel.ActualHeight;
            
            var videoTrack = playbackService.CurrentMedia?.tracks()?.FirstOrDefault(x => x.type() == TrackType.Video);

            if (videoTrack == null)
                return;
            
            switch (desiredZoom)
            {
                case VLCSurfaceZoom.SURFACE_BEST_FIT:
                    playbackService.VideoAspectRatio = string.Empty;
                    playbackService.VideoScale = 0;
                    break;
                case VLCSurfaceZoom.SURFACE_FIT_SCREEN:
                    var videoW = videoTrack.width();
                    var videoH = videoTrack.height();
                    
                    if (videoTrack.sarNum() != videoTrack.sarDen())
                        videoW = videoW * videoTrack.sarNum() / videoTrack.sarDen();

                    var ar = videoW / (float)videoH;
                    var dar = screenWidth / screenHeight;

                    float scale;
                    if (dar >= ar)
                        scale = (float)screenWidth / videoW; /* horizontal */
                    else
                        scale = (float)screenHeight / videoH; /* vertical */

                    playbackService.VideoScale = scale;
                    playbackService.VideoAspectRatio = string.Empty;
                    break;
                case VLCSurfaceZoom.SURFACE_FILL:
                    playbackService.VideoScale = 0;
                    playbackService.VideoAspectRatio = $"{screenWidth}:{screenHeight}";
                    break;
                case VLCSurfaceZoom.SURFACE_16_9:
                    playbackService.VideoAspectRatio = "16:9";
                    playbackService.VideoScale = 0;
                    break;
                case VLCSurfaceZoom.SURFACE_4_3:
                    playbackService.VideoAspectRatio = "4:3";
                    playbackService.VideoScale = 0;
                    break;
                case VLCSurfaceZoom.SURFACE_ORIGINAL:
                    playbackService.VideoAspectRatio = string.Empty;
                    playbackService.VideoScale = 1;
                    break;
            }
        }

        public void RequestChangeControlBarVisibility(bool visibility)
        {
            PlayerControlVisibilityChangeRequested?.Invoke(this, visibility);
        }
        #endregion

        #region events

        private async void PlaybackService_Playback_MediaSet(IMediaItem media)
        {
            await UpdateCurrentVideo(media as VideoItem);
            if (!(media is VideoItem))
                return;
            await Locator.MediaPlaybackViewModel.SetMediaTransportControlsInfo(CurrentVideo.Name);
        }

        private async Task UpdateCurrentVideo(VideoItem video)
        {
            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () =>
            {
                Locator.VideoPlayerVm.CurrentVideo = video;
                if (video != null)
                    AppViewHelper.SetTitleBarTitle(video.Name);
            });
            if (video != null)
            {
                await TryUseSubtitleFromFolder();

                var currentVideoTrackid = Locator.PlaybackService.VideotrackId;
                if (currentVideoTrackid != -1)
                {
                    var currentMediaTrack = Locator.PlaybackService.CurrentMedia?.tracks()?
                        .FirstOrDefault(t => t.id() == currentVideoTrackid);
                    Is3DVideo = currentMediaTrack?.projection() == Projection.Equirectangular;
                }
            }
        }

        public bool Is3DVideo { get; private set; }

        public void OnPlayerControlVisibilityChanged(bool visibility)
        {
            PlayerControlVisibility = visibility;
        }

        #endregion
    }
}
