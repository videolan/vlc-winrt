/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Core;
using SQLite;
using VLC_WinRT.Commands;
using VLC_WinRT.Database;
using VLC_WinRT.Helpers;
using VLC_WinRT.Helpers.VideoLibrary;
using VLC_WinRT.Model;
using VLC_WinRT.Model.Video;
using VLC_WinRT.Utils;
using VLC_WinRT.Commands.VideoPlayer;
using VLC_WinRT.Commands.VideoLibrary;
using System.Collections.Generic;

namespace VLC_WinRT.ViewModels.VideoVM
{
    public class VideoLibraryVM : BindableBase
    {
        public VideoRepository VideoRepository = new VideoRepository();
        #region private fields
        private ObservableCollection<VideoItem> _videos;
        private ObservableCollection<VideoItem> _viewedVideos;
        private ObservableCollection<VideoItem> _cameraRoll;
        private ObservableCollection<TvShow> _shows = new ObservableCollection<TvShow>();
        #endregion

        #region private props
        private VideoView _videoView;
        private LoadingState _loadingState;
        private bool _isBusy = false;
        private bool _hasNoMedia = true;
        private TvShow _currentShow;
        #endregion

        #region public fields
        public List<VideoView> VideoViewCollection { get; set; } = new List<VideoView>()
        {
            VideoView.Videos,
            VideoView.Shows,
            VideoView.CameraRoll
        };

        public ObservableCollection<VideoItem> Videos
        {
            get { return _videos; }
            set { SetProperty(ref _videos, value); }
        }

        public ObservableCollection<VideoItem> ViewedVideos
        {
            get { return _viewedVideos; }
            set
            {
                SetProperty(ref _viewedVideos, value);
            }
        }

        public ObservableCollection<TvShow> Shows
        {
            get { return _shows; }
            set { SetProperty(ref _shows, value); }
        }

        public ObservableCollection<VideoItem> CameraRoll
        {
            get { return _cameraRoll; }
            set { SetProperty(ref _cameraRoll, value); }
        }
        #endregion

        #region public props
        public VideoView VideoView
        {
            get
            {
                var videoView = ApplicationSettingsHelper.ReadSettingsValue(nameof(VideoView), false);
                if (videoView == null)
                {
                    _videoView = VideoView.Videos;
                }
                else
                {
                    _videoView = (VideoView)videoView;
                }
                return _videoView;
            }
            set
            {
                ApplicationSettingsHelper.SaveSettingsValue(nameof(VideoView), (int)value, false);
                SetProperty(ref _videoView, value);
            }
        }

        public TvShow CurrentShow
        {
            get { return _currentShow; }
            set { SetProperty(ref _currentShow, value); }
        }

        public LoadingState LoadingState { get { return _loadingState; } set { SetProperty(ref _loadingState, value); } }
        public bool HasNoMedia
        {
            get { return _hasNoMedia; }
            set { SetProperty(ref _hasNoMedia, value); }
        }

        [Ignore]
        public PlayVideoCommand OpenVideo { get; } = new PlayVideoCommand();

        [Ignore]
        public CloseFlyoutAndPlayVideoCommand CloseFlyoutAndPlayVideoCommand { get; }=new CloseFlyoutAndPlayVideoCommand();

        public PlayNetworkMRLCommand PlayNetworkMRL { get; }=new PlayNetworkMRLCommand();

        public StartVideoIndexingCommand StartVideoIndexingCommand { get; }=new StartVideoIndexingCommand();
        
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }
        #endregion
        #region contructors
        public VideoLibraryVM()
        {
        }

        public void ResetLibrary()
        {
            LoadingState = LoadingState.NotLoaded;
            Videos = new ObservableCollection<VideoItem>();
            ViewedVideos = new ObservableCollection<VideoItem>();
            CameraRoll = new ObservableCollection<VideoItem>();
            Shows = new ObservableCollection<TvShow>();
            CurrentShow = null;
            GC.Collect();
        }

        public void OnNavigatedTo()
        {
            ResetLibrary();
        }

        public void OnNavigatedToAllVideos()
        {
            if (LoadingState == LoadingState.NotLoaded)
            {
                Initialize();
            }
        }

        public void OnNavigatedToCameraRollVideos()
        {
            if (LoadingState == LoadingState.NotLoaded)
            {
                InitializeCameraRollVideos();
            }
        }

        public void OnNavigatedFrom()
        {
            ResetLibrary();
        }

        Task Initialize()
        {
            return Task.Run(async () =>
            {
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () => LoadingState = LoadingState.Loading);
                await VideoLibraryManagement.GetViewedVideos().ConfigureAwait(false);
                await VideoLibraryManagement.GetVideos(VideoRepository).ConfigureAwait(false);
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () =>
                {
                    Locator.MainVM.InformationText = "";
                });
            });
        }

        Task InitializeCameraRollVideos()
        {
            return Task.Run(async () => await VideoLibraryManagement.GetVideosFromCameraRoll(VideoRepository).ConfigureAwait(false));
        }

        #endregion

        #region methods
        public async Task PerformRoutineCheckIfNotBusy()
        {
            // Routine check to add new files if there are new ones
            if (!IsBusy)
            {
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () =>
                {
                    IsBusy = true;
                });
                await Initialize();
                await InitializeCameraRollVideos();
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () =>
                {
                    IsBusy = false;
                });
            }
        }
        #endregion
    }
}
