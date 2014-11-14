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
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Model.Video;
using Panel = VLC_WINRT_APP.Model.Panel;

namespace VLC_WINRT_APP.ViewModels.VideoVM
{
    public class VideoLibraryVM : BindableBase
    {
        #region private fields
#if WINDOWS_APP
        private ObservableCollection<Panel> _panels = new ObservableCollection<Panel>();
#endif
        private ObservableCollection<VideoItem> _videos;
        private ObservableCollection<VideoItem> _viewedVideos;
        #endregion

        #region private props
        private LoadingState _loadingState;
        private PlayVideoCommand _openVideo;
        private PickVideoCommand _pickCommand = new PickVideoCommand();
        private PlayNetworkMRLCommand _playNetworkMRL = new PlayNetworkMRLCommand();
        private bool _hasNoMedia = true;
        public LastVideosRepository _lastVideosRepository = new LastVideosRepository();
        private ObservableCollection<TvShow> _shows = new ObservableCollection<TvShow>();
        private TvShow _currentShow;

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

        #endregion

        #region public props

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
            Videos = new ObservableCollection<VideoItem>();
            ViewedVideos = new ObservableCollection<VideoItem>();
#if WINDOWS_APP
            var resourceLoader = new ResourceLoader();
            Panels.Add(new Panel(resourceLoader.GetString("Videos"), 0, 1, App.Current.Resources["VideoPath"].ToString(), true));
            //Panels.Add(new Panel("favorite", 2, 0.4));
#endif
        }
        public Task Initialize()
        {
            LoadingState = LoadingState.Loading;
            return GetViewedVideos();
        }
        #endregion

        #region methods
        public async Task GetViewedVideos()
        {
            var result = await _lastVideosRepository.Load();

            foreach (VideoItem videoVm in result)
            {
                try
                {
                    StorageFile file = await StorageFile.GetFileFromPathAsync(videoVm.FilePath);
                    videoVm.File = file;
                    await videoVm.GenerateThumbnail();
                    await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
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
            var resourceLoader = new ResourceLoader();
#if WINDOWS_APP
            StorageLibrary videoLibrary = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Videos);
            foreach (StorageFolder storageFolder in videoLibrary.Folders)
#else
            StorageFolder storageFolder = KnownFolders.VideosLibrary;
#endif
            {
                try
                {
                    IReadOnlyList<StorageFile> files = await GetMediaFromFolder(storageFolder);

                    foreach (StorageFile storageFile in files)
                    {
                        // Analyse to see if it's a tv show
                        // if the file is from a tv show, we push it to this tvshow item
                        Dictionary<string, string> showInfoDictionary = TitleDecrapifier.tvShowEpisodeInfoFromString(storageFile.DisplayName);
                        bool isTvShow = showInfoDictionary != null && showInfoDictionary.Count > 0;

                        VideoItem mediaVM = !isTvShow ? new VideoItem() : new VideoItem(showInfoDictionary["season"], showInfoDictionary["episode"]);
                        await mediaVM.Initialize(storageFile);
                        if (string.IsNullOrEmpty(mediaVM.Title))
                            continue;
                        VideoItem searchVideo = ViewedVideos.FirstOrDefault(x => x.Title == mediaVM.Title);
                        if (searchVideo != null)
                        {
                            mediaVM.TimeWatched = searchVideo.TimeWatched;
                        }

                        if (isTvShow)
                        {
#if WINDOWS_APP
                            if (Panels.Count == 1)
                            {
                                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                    Panels.Add(new Panel(resourceLoader.GetString("Shows"), 1, 0.4, null)));
                            }
#endif
                            TvShow show = Shows.FirstOrDefault(x => x.ShowTitle == showInfoDictionary["tvShowName"]);
                            if (show == null)
                            {
                                show = new TvShow(showInfoDictionary["tvShowName"]);
                                await DispatchHelper.InvokeAsync(() =>
                                {
                                    show.Episodes.Add(mediaVM as VideoItem);
                                    Shows.Add(show);
                                    if (Shows.Count == 1)
                                        CurrentShow = Shows[0];
                                });
                            }
                            else
                            {
                                await DispatchHelper.InvokeAsync(() =>
                                show.Episodes.Add(mediaVM as VideoItem));
                            }
                        }
                        // Get back to UI thread
                        await DispatchHelper.InvokeAsync(() =>
                        {
                            if (!isTvShow)
                            {
                                Videos.Add(mediaVM);
                            }
#if WINDOWS_APP
                            if (ViewedVideos.Count < 6 && ViewedVideos.FirstOrDefault(x => x.FilePath == mediaVM.FilePath && x.TimeWatched == TimeSpan.Zero) == null)
                                ViewedVideos.Add(mediaVM);
#endif
                        });
                    }
                }
                catch
                {
                    Debug.WriteLine("An error occured while indexing a video folder");
                }
            }
            if (Videos.Count > 0)
            {
                await DispatchHelper.InvokeAsync(() => HasNoMedia = false);
            }
            else
            {
                await DispatchHelper.InvokeAsync(() => HasNoMedia = true);
            }
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
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

        private static async Task GetFilesFromSubFolders(StorageFolder folder, List<StorageFile> files)
        {
            var items = await folder.GetItemsAsync();
            foreach (IStorageItem storageItem in items)
            {
                if (storageItem.IsOfType(StorageItemTypes.Folder))
                    await GetFilesFromSubFolders(storageItem as StorageFolder, files);
                else if(storageItem.IsOfType(StorageItemTypes.File))
                {
                    var file = storageItem as StorageFile;
                    if (VLCFileExtensions.VideoExtensions.Contains(file.FileType.ToLower()))
                    {
                        files.Add(file);
                    }
                }
            }
        }

        private static async Task<List<StorageFile>> GetMediaFromFolder(StorageFolder folder)
        {
            var videoFiles = new List<StorageFile>();
            try
            {
                await GetFilesFromSubFolders(folder, videoFiles);
                return videoFiles;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("exception listing files");
                Debug.WriteLine(ex.ToString());
            }
            return null;
        }

        public void ExecuteSemanticZoom(SemanticZoom sZ, CollectionViewSource cvs)
        {
            (sZ.ZoomedOutView as ListViewBase).ItemsSource = cvs.View.CollectionGroups;
        }
        #endregion
    }
}
