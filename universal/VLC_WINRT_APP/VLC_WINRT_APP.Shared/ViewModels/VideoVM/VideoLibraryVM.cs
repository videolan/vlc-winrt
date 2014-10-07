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
using Windows.ApplicationModel.Resources;
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
        #endregion

        #region private props
        private LoadingState _loadingState;
        private PlayVideoCommand _openVideo;
        private PickVideoCommand _pickCommand = new PickVideoCommand();
        private PlayNetworkMRLCommand _playNetworkMRL = new PlayNetworkMRLCommand();
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
        public PlayNetworkMRLCommand PlayNetworkMRL
        {
            get { return _playNetworkMRL; }
            set { SetProperty(ref _playNetworkMRL, value); }
        }
        #endregion
        #region contructors


        public VideoLibraryVM()
        {
            LoadingState = LoadingState.NotLoaded;
            OpenVideo = new PlayVideoCommand();
            Videos = new ObservableCollection<VideoVM>();
            ViewedVideos = new ObservableCollection<VideoVM>();
#if WINDOWS_APP
            var resourceLoader = new ResourceLoader();
            Panels.Add(new Panel(resourceLoader.GetString("Videos"), 1, 0.4, App.Current.Resources["VideoPath"].ToString()));
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
        public async void GetViewedVideos()
        {
            var result = await _lastVideosRepository.Load();

            var testCollection = result;
            foreach (VideoVM videoVm in testCollection)
            {
                try
                {
                    await videoVm.InitializeFromFilePath();
                    App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                    {
                        ViewedVideos.Add(videoVm);
                    });
                }
                catch (Exception)
                {
                    // If the video file was deleted, we can't add it to the last viewed files.
                    // We "should" keep the file in the list, and if the user selects it either tell them that the file
                    // is now gone (and let them try and find it again, in case they moved it, so we can keep it in the DB)
                    // but that will require quite a bit of code work to make happen. So for now, we'll catch the error
                    // and not add it to the list.
                    Debug.WriteLine("File not found");
                }
            }
            await GetVideos();
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

                        // Get back to UI thread
                        await DispatchHelper.InvokeAsync(() =>
                        {
                            Videos.Add(mediaVM);
                            if (ViewedVideos.Count < 6 && !ViewedVideos.Contains(mediaVM))
                                ViewedVideos.Add(mediaVM);
                        });
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


        private static async Task<List<StorageFile>> GetMediaFromFolder(StorageFolder folder,
                                                                            CommonFileQuery query)
        {
            IReadOnlyList<StorageFile> files = null;
            try
            {
                files = await folder.GetFilesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("exception listing files");
                Debug.WriteLine(ex.ToString());
            }
            // DLNA folders don't support advanced file listings, us a basic file query
            if (files == null)
            {
                StorageFileQueryResult fileQuery = folder.CreateFileQuery(CommonFileQuery.OrderByName);
                files = await fileQuery.GetFilesAsync();
            }
            List<StorageFile> videoFiles = new List<StorageFile>(files);

            // Verify that the file format is a video
            foreach (StorageFile storageFile in videoFiles)
            {
                if (!VLCFileExtensions.VideoExtensions.Contains(storageFile.FileType.ToLower()))
                {
                    videoFiles.Remove(storageFile);
                }
            }
            return videoFiles;
        }

        public void ExecuteSemanticZoom(SemanticZoom sZ, CollectionViewSource cvs)
        {
            (sZ.ZoomedOutView as ListViewBase).ItemsSource = cvs.View.CollectionGroups;
        }
        #endregion
    }
}
