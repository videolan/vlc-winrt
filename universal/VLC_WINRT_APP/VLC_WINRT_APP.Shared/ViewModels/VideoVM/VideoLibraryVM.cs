/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

#if WINDOWS_PHONE_APP

#endif
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using SQLite;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Commands;
using VLC_WINRT_APP.Commands.Video;
using VLC_WINRT_APP.Common;
using VLC_WINRT_APP.DataRepository;
using VLC_WINRT_APP.Model;
using Panel = VLC_WINRT_APP.Model.Panel;

namespace VLC_WINRT_APP.ViewModels.VideoVM
{
    public class VideoLibraryVM : BindableBase
    {
        #region private fields
#if WINDOWS_APP
        private ObservableCollection<Panel> _panels = new ObservableCollection<Panel>();
#endif
        private ObservableCollection<VideoVM> _videos;
        private ObservableCollection<VideoVM> _viewedVideos;
        private ObservableCollection<VideoVM> _newVideos;
        #endregion

        #region private props
        private LoadingState _loadingState;
        private PlayVideoCommand _openVideo;
        private PickVideoCommand _pickCommand = new PickVideoCommand();
        private bool _hasNoMedia = true;
        public LastVideosRepository _lastVideosRepository = new LastVideosRepository();
        #endregion

        #region public fields
#if WINDOWS_APP
        public ObservableCollection<Panel> Panels
        {
            get { return _panels; }
            set
            {
                SetProperty(ref _panels, value);
            }
        }
#endif

        public ObservableCollection<VideoVM> Videos
        {
            get { return _videos; }
            set { SetProperty(ref _videos, value); }
        }

        public ObservableCollection<VideoVM> ViewedVideos
        {
            get { return _viewedVideos; }
            set
            {
                SetProperty(ref _viewedVideos, value);
            }
        }

        public ObservableCollection<VideoVM> NewVideos
        {
            get { return _newVideos; }
            set { SetProperty(ref _newVideos, value); }
        }
        #endregion

        #region public props
        public LoadingState LoadingState { get { return _loadingState; } set { SetProperty(ref _loadingState, value); } }
        public bool HasNoMedia
        {
            get { return _hasNoMedia; }
            set { SetProperty(ref _hasNoMedia, value); }
        }

        [Ignore]
        public PlayVideoCommand OpenVideo
        {
            get { return _openVideo; }
            set { SetProperty(ref _openVideo, value); }
        }

        public PickVideoCommand PickVideo
        {
            get { return _pickCommand; }
            set { SetProperty(ref _pickCommand, value); }
        }
        #endregion
        #region contructors


        public VideoLibraryVM()
        {
            LoadingState = LoadingState.NotLoaded;
            OpenVideo = new PlayVideoCommand();
            Videos = new ObservableCollection<VideoVM>();
            ViewedVideos = new ObservableCollection<VideoVM>();
            NewVideos = new ObservableCollection<VideoVM>();
#if WINDOWS_APP
            Panels.Add(new Panel("videos", 0, 1, App.Current.Resources["HomePath"].ToString(), true));
            //Panels.Add(new Panel("favorite", 2, 0.4));
#endif
        }
        public void Initialize()
        {
            LoadingState = LoadingState.Loading;
            Task.Run(() => ThreadPool.RunAsync(operation => GetViewedVideos()));
        }
        #endregion

        #region methods
        public void GetViewedVideos()
        {
            App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => NewVideos = new ObservableCollection<VideoVM>());

            _lastVideosRepository.Load().ContinueWith((result) =>
            {
                App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    ViewedVideos = result.Result;
                    foreach (VideoVM videoVm in ViewedVideos)
                    {
                        videoVm.InitializeFromFilePath();
                    }
                });
            });
            GetVideos();
        }

        public async Task GetVideos()
        {
#if WINDOWS_APP
            StorageLibrary videoLibrary = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Videos);
            foreach (StorageFolder storageFolder in videoLibrary.Folders)
#else
            StorageFolder storageFolder = KnownFolders.VideosLibrary;
#endif
            {
                try
                {
                    IReadOnlyList<StorageFile> files =
                        await GetMediaFromFolder(storageFolder, CommonFileQuery.OrderByName);

                    //int j = 0;
                    foreach (StorageFile storageFile in files)
                    {
                        var mediaVM = new VideoVM();
                        mediaVM.Initialize(storageFile);

                        if (string.IsNullOrEmpty(mediaVM.Title))
                            continue;

                        VideoVM searchVideo = ViewedVideos.FirstOrDefault(x => x.Title == mediaVM.Title);
                        if (searchVideo != null)
                        {
                            mediaVM.TimeWatched = searchVideo.TimeWatched;
                        }
                        else
                        {
                            DispatchHelper.Invoke(() => NewVideos.Add(mediaVM));
                        }

                        // Get back to UI thread
                        await DispatchHelper.InvokeAsync(() => Videos.Add(mediaVM));
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine("An error occured while indexing a video folder");
                }
            }
            if (Videos.Count > 0)
            {
                DispatchHelper.InvokeAsync(() => HasNoMedia = false);
            }
            else
            {
                DispatchHelper.InvokeAsync(() => HasNoMedia = true);
            }
            App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {

                //if (NewVideos.Any())
                //{
#if WINDOWS_APP
                    //Panels.Add(new Panel("new", 1, 0.4, App.Current.Resources["HomePath"].ToString()));
#endif
                //}
                LoadingState = LoadingState.Loaded;
            });

        }


        private static async Task<IReadOnlyList<StorageFile>> GetMediaFromFolder(StorageFolder folder,
                                                                            CommonFileQuery query)
        {
            IReadOnlyList<StorageFile> files = null;
            StorageFileQueryResult fileQuery;
#if WINDOWS_APP
            var queryOptions = new QueryOptions(query,
                                               new List<string>
                                               {
                                                   ".3g2",
                                                   ".3gp",
                                                   ".3gp2",
                                                   ".3gpp",
                                                   ".amv",
                                                   ".asf",
                                                   ".avi",
                                                   ".divx",
                                                   ".drc",
                                                   ".dv",
                                                   ".f4v",
                                                   ".flv",
                                                   ".gvi",
                                                   ".gxf",
                                                   ".ismv",
                                                   ".iso",
                                                   ".m1v",
                                                   ".m2v",
                                                   ".m2t",
                                                   ".m2ts",
                                                   ".m3u8",
                                                   ".mkv",
                                                   ".mov",
                                                   ".mp2",
                                                   ".mp2v",
                                                   ".mp4",
                                                   ".mp4v",
                                                   ".mpe",
                                                   ".mpeg",
                                                   ".mpeg1",
                                                   ".mpeg2",
                                                   ".mpeg4",
                                                   ".mpg",
                                                   ".mpv2",
                                                   ".mts",
                                                   ".mtv",
                                                   ".mxf",
                                                   ".mxg",
                                                   ".nsv",
                                                   ".nut",
                                                   ".nuv",
                                                   ".ogm",
                                                   ".ogv",
                                                   ".ogx",
                                                   ".ps",
                                                   ".rec",
                                                   ".rm",
                                                   ".rmvb",
                                                   ".tob",
                                                   ".ts",
                                                   ".tts",
                                                   ".vob",
                                                   ".vro",
                                                   ".webm",
                                                   ".wm",
                                                   ".wmv",
                                                   ".wtv",
                                                   ".xesc",
                                               });
#endif
            try
            {
#if WINDOWS_APP
                fileQuery = folder.CreateFileQueryWithOptions(queryOptions);
                files = await fileQuery.GetFilesAsync();
#else

                files = await folder.GetFilesAsync();
#endif
            }
            catch (Exception ex)
            {
                Debug.WriteLine("exception listing files");
                Debug.WriteLine(ex.ToString());
            }
            // DLNA folders don't support advanced file listings, us a basic file query
            if (files == null)
            {
                fileQuery = folder.CreateFileQuery(CommonFileQuery.OrderByName);
                files = await fileQuery.GetFilesAsync();
            }

            return files;
        }

        public void ExecuteSemanticZoom(SemanticZoom sZ, CollectionViewSource cvs)
        {
            (sZ.ZoomedOutView as ListViewBase).ItemsSource = cvs.View.CollectionGroups;
        }

        #endregion
    }
}
