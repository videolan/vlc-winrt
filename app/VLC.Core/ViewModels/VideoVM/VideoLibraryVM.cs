/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using VLC.Commands.MediaLibrary;
using VLC.Commands.Navigation;
using VLC.Commands.VideoLibrary;
using VLC.Commands.VideoPlayer;
using VLC.Helpers;
using VLC.Model;
using VLC.Model.Video;
using VLC.Utils;
using Windows.UI.Core;

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
        private TvShow _currentShow;
        private List<VideoView> _videoViewCollection;
        #endregion

        #region public fields
        public List<VideoView> VideoViewCollection
        {
            get
            {
                if (_videoViewCollection == null)
                {
                    _videoViewCollection = new List<VideoView>()
                    {
                        VideoView.Videos,
                        VideoView.Shows,
                    };
                    if (DeviceHelper.GetDeviceType() != DeviceTypeEnum.Xbox)
                        _videoViewCollection.Add(VideoView.CameraRoll);
                }
                return _videoViewCollection;
            }
        }

        public ObservableCollection<VideoItem> Videos
        {
            get { return Locator.MediaLibrary.Videos?.ToObservable(); }
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

        public static TVShowClickedCommand TVShowClickedCommand { get; private set; } = new TVShowClickedCommand();
        public PlayVideoCommand OpenVideo { get; private set; } = new PlayVideoCommand();

        public CloseFlyoutAndPlayVideoCommand CloseFlyoutAndPlayVideoCommand { get; private set; } = new CloseFlyoutAndPlayVideoCommand();
        public DeleteFromLibraryCommand DeleteFromLibraryCommand { get; private set; } = new DeleteFromLibraryCommand();
        public ChangeVideoViewCommand ChangeVideoViewCommand { get; private set; } = new ChangeVideoViewCommand();
        #endregion
        #region contructors
        public VideoLibraryVM()
        {
        }
        #endregion

        #region methods
        public void ResetLibrary()
        {
            LoadingStateAllVideos = LoadingStateCamera = LoadingStateShows = LoadingState.NotLoaded;
            CurrentShow = null;
        }

        public void OnNavigatedTo()
        {
            ResetLibrary();
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
        }

        Task InitializeVideos()
        {
            return Task.Run(async () =>
            {
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
                {
                    LoadingStateAllVideos = LoadingState.Loading;
                });

                if (Locator.MediaLibrary.Videos != null)
                    Locator.MediaLibrary.Videos.CollectionChanged += Videos_CollectionChanged;
                Locator.MediaLibrary.LoadVideosFromDatabase();

                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
                {
                    OnPropertyChanged(nameof(Videos));
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