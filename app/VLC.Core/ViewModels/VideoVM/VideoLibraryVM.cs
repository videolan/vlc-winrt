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
using VLC.Commands;
using VLC.Database;
using VLC.Helpers;
using VLC.Helpers.VideoLibrary;
using VLC.Model;
using VLC.Model.Video;
using VLC.Utils;
using VLC.Commands.VideoPlayer;
using VLC.Commands.VideoLibrary;
using System.Collections.Generic;
using Autofac;
using System.Linq;
using VLC.Model.Library;
using Windows.UI.Xaml;
using VLC.Commands.MediaLibrary;
using VLC.Commands.Navigation;

namespace VLC.ViewModels.VideoVM
{
    public class VideoLibraryVM : BindableBase
    {
        #region private fields
        #endregion

        #region private props
        private VideoView _videoView;
        private LoadingState _loadingStateAllVideos;
        private LoadingState _loadingStateCamera;
        private LoadingState _loadingStateShows;
        private bool _isIndexingLibrary = false;
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
            get { return Locator.MediaLibrary.Videos?.ToObservable(); }
        }

        public ObservableCollection<VideoItem> ViewedVideos
        {
            get { return Locator.MediaLibrary.ViewedVideos?.ToObservable(); }
        }

        public ObservableCollection<TvShow> Shows
        {
            get { return Locator.MediaLibrary.Shows?.ToObservable(); }
        }

        public ObservableCollection<VideoItem> CameraRoll
        {
            get { return Locator.MediaLibrary.CameraRoll?.ToObservable(); }
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

        public LoadingState LoadingStateAllVideos { get { return _loadingStateAllVideos; } private set { SetProperty(ref _loadingStateAllVideos, value); } }
        public LoadingState LoadingStateShows { get { return _loadingStateShows; } private set { SetProperty(ref _loadingStateShows, value); } }
        public LoadingState LoadingStateCamera { get { return _loadingStateCamera; } private set { SetProperty(ref _loadingStateCamera, value); } }

        public bool HasNoMedia
        {
            get { return _hasNoMedia; }
            set { SetProperty(ref _hasNoMedia, value); }
        }


        public static TVShowClickedCommand TVShowClickedCommand { get; private set; } = new TVShowClickedCommand();
        public PlayVideoCommand OpenVideo { get; private set; } = new PlayVideoCommand();

        public CloseFlyoutAndPlayVideoCommand CloseFlyoutAndPlayVideoCommand { get; private set; } = new CloseFlyoutAndPlayVideoCommand();
        public DeleteFromLibraryCommand DeleteFromLibraryCommand { get; private set; } = new DeleteFromLibraryCommand();
        public ChangeVideoViewCommand ChangeVideoViewCommand { get; private set; } = new ChangeVideoViewCommand();
        public Visibility IndexingLibraryVisibility
        {
            get { return Locator.MediaLibrary.MediaLibraryIndexingState == LoadingState.Loading ? Visibility.Visible : Visibility.Collapsed; }
        }
        #endregion
        #region contructors
        public VideoLibraryVM()
        {
        }
        #endregion

        #region methods
        private async void MediaLibrary_OnIndexing(LoadingState obj)
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () => OnPropertyChanged(nameof(IndexingLibraryVisibility)));
        }

        public void ResetLibrary()
        {
            LoadingStateAllVideos = LoadingStateCamera = LoadingStateShows = LoadingState.NotLoaded;
            CurrentShow = null;
        }

        public void OnNavigatedTo()
        {
            ResetLibrary();
            Locator.MediaLibrary.OnIndexing += MediaLibrary_OnIndexing;
        }

        public void OnNavigatedToAllVideos()
        {
            if (LoadingStateAllVideos == LoadingState.NotLoaded)
            {
                InitializeVideos();
            }
        }

        public void OnNavigatedToShows()
        {
            if (LoadingStateShows == LoadingState.NotLoaded)
            {
                InitializeShows();
            }
        }

        public void OnNavigatedToCameraRollVideos()
        {
            if (LoadingStateCamera == LoadingState.NotLoaded)
            {
                InitializeCameraRollVideos();
            }
        }

        public Task OnNavigatedFromAllVideos()
        {
            return DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () => LoadingStateAllVideos = LoadingState.NotLoaded);
        }

        public Task OnNavigatedFromShows()
        {
            return DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () => LoadingStateShows = LoadingState.NotLoaded);
        }

        public Task OnNavigatedFromCamera()
        {
            return DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () => LoadingStateCamera = LoadingState.NotLoaded);
        }

        public void OnNavigatedFrom()
        {
            ResetLibrary();
            Locator.MediaLibrary.OnIndexing -= MediaLibrary_OnIndexing;
        }

        Task InitializeVideos()
        {
            return Task.Run(async () =>
            {
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Locator.MainVM.InformationText = Strings.Loading;
                    LoadingStateAllVideos = LoadingState.Loading;
                });

                if (Locator.MediaLibrary.Videos != null)
                    Locator.MediaLibrary.Videos.CollectionChanged += Videos_CollectionChanged;
                Locator.MediaLibrary.LoadVideosFromDatabase();
                await Locator.MediaLibrary.LoadViewedVideosFromDatabase();

                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
                {
                    OnPropertyChanged(nameof(ViewedVideos));
                    OnPropertyChanged(nameof(Videos));
                    Locator.MainVM.InformationText = String.Empty;
                    LoadingStateAllVideos = LoadingState.Loaded;
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
                    LoadingStateShows = LoadingState.Loading;
                });

                if (Locator.MediaLibrary.Shows != null)
                    Locator.MediaLibrary.Shows.CollectionChanged += Shows_CollectionChanged;
                await Locator.MediaLibrary.LoadShowsFromDatabase();
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
                    LoadingStateCamera = LoadingState.Loading;
                });

                if (Locator.MediaLibrary.CameraRoll != null)
                    Locator.MediaLibrary.CameraRoll.CollectionChanged += CameraRoll_CollectionChanged;
                Locator.MediaLibrary.LoadCameraRollFromDatabase();
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
    }
}