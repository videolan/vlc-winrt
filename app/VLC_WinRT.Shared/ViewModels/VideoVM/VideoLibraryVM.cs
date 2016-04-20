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
using Autofac;
using System.Linq;
using VLC_WinRT.Model.Library;

namespace VLC_WinRT.ViewModels.VideoVM
{
    public class VideoLibraryVM : BindableBase
    {
        MediaLibrary VideoLibrary => Locator.MediaLibrary;
        #region private fields
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
            get { return VideoLibrary.Videos.ToObservable(); }
        }

        public ObservableCollection<VideoItem> ViewedVideos
        {
            get { return VideoLibrary.ViewedVideos.ToObservable(); }
        }

        public ObservableCollection<TvShow> Shows
        {
            get { return VideoLibrary.Shows.ToObservable(); }
        }

        public ObservableCollection<VideoItem> CameraRoll
        {
            get { return VideoLibrary.CameraRoll.ToObservable(); }
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

        
        public PlayVideoCommand OpenVideo { get; } = new PlayVideoCommand();

        public CloseFlyoutAndPlayVideoCommand CloseFlyoutAndPlayVideoCommand { get; } = new CloseFlyoutAndPlayVideoCommand();

        public PlayNetworkMRLCommand PlayNetworkMRL { get; } = new PlayNetworkMRLCommand();

        public StartVideoIndexingCommand StartVideoIndexingCommand { get; } = new StartVideoIndexingCommand();

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
                InitializeVideos();
            }
        }

        public void OnNavigatedToShows()
        {
            if (LoadingState == LoadingState.NotLoaded)
            {
                InitializeShows();
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

        Task InitializeVideos()
        {
            return Task.Run(async () =>
            {
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Locator.MainVM.InformationText = Strings.Loading;
                    LoadingState = LoadingState.Loading;
                });

                if (VideoLibrary.Videos != null)
                    VideoLibrary.Videos.CollectionChanged += Videos_CollectionChanged;
                await VideoLibrary.LoadVideosFromDatabase();
                await VideoLibrary.LoadViewedVideosFromDatabase();

                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
                {
                    OnPropertyChanged(nameof(ViewedVideos));
                    Locator.MainVM.InformationText = String.Empty;
                    LoadingState = LoadingState.Loaded;
                });
            });
        }

        private async void Videos_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                OnPropertyChanged(nameof(Videos));
            });
        }

        Task InitializeShows()
        {
            return Task.Run(async () =>
            {
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Locator.MainVM.InformationText = Strings.Loading;
                    LoadingState = LoadingState.Loading;
                });

                if (VideoLibrary.Shows != null)
                    VideoLibrary.Shows.CollectionChanged += Shows_CollectionChanged;
                await VideoLibrary.LoadShowsFromDatabase();
            });
        }

        private async void Shows_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                OnPropertyChanged(nameof(Shows));
            });
        }

        Task InitializeCameraRollVideos()
        {
            return Task.Run(async () =>
            {
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Locator.MainVM.InformationText = Strings.Loading;
                    LoadingState = LoadingState.Loading;
                });

                if (VideoLibrary.CameraRoll != null)
                    VideoLibrary.CameraRoll.CollectionChanged += CameraRoll_CollectionChanged;
                await VideoLibrary.LoadCameraRollFromDatabase();
            });
        }

        private async void CameraRoll_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                OnPropertyChanged(nameof(CameraRoll));
            });
        }
        #endregion

        #region methods
        #endregion
    }
}