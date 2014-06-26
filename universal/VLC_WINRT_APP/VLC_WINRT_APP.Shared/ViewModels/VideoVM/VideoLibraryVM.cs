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
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;
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
using VLC_WINRT_APP.ViewModels.Settings;
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
        private IEnumerable<IGrouping<char, VideoVM>> _mediaGroupedByAlphabet;
        #endregion

        #region private props
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

        public IEnumerable<IGrouping<char, VideoVM>> MediaGroupedByAlphabet
        {
            get { return _mediaGroupedByAlphabet; }
            set { SetProperty(ref _mediaGroupedByAlphabet, value); }
        }
        #endregion

        #region public props
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
        #endregion
        #region contructors


        public VideoLibraryVM()
        {
            OpenVideo = new PlayVideoCommand();
            Videos = new ObservableCollection<VideoVM>();
            ViewedVideos = new ObservableCollection<VideoVM>();
            ThreadPool.RunAsync(operation => GetViewedVideos());
            //Task.Run(() => GetViewedVideos());
#if WINDOWS_APP
            Panels.Add(new Panel("all", 0, 1, App.Current.Resources["HomePath"].ToString()));
            //Panels.Add(new Panel("favorite", 2, 0.4));
#endif
        }

        #endregion

        #region methods
        public async Task GetViewedVideos()
        {
            App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => NewVideos = new ObservableCollection<VideoVM>());
            DispatchHelper.Invoke(async () => ViewedVideos = await _lastVideosRepository.Load());
            GetVideos();
        }

        public async Task GetVideos()
        {
#if WINDOWS_APP
            foreach (CustomFolder folder in Locator.SettingsVM.VideoFolders)
            {
#endif
                try
                {
                    StorageFolder customVideoFolder;
#if WINDOWS_APP
                    if (folder.Mru == "Video Library")
                    {
                        customVideoFolder = KnownFolders.VideosLibrary;
                    }
                    else
                    {
                        customVideoFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(
                            folder.Mru);
                    }
#endif
#if WINDOWS_PHONE_APP                    
                customVideoFolder = KnownFolders.VideosLibrary;
#endif

                    IReadOnlyList<StorageFile> files =
                        await GetMediaFromFolder(customVideoFolder, CommonFileQuery.OrderByName);

                    //int j = 0;
                    foreach (StorageFile storageFile in files)
                    {
                        var mediaVM = new VideoVM();
                        mediaVM.Initialize(storageFile);
                        await mediaVM.Initialize();

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
                        await DispatchHelper.InvokeAsync(() =>
                        {
                            Videos.Add(mediaVM);
                            //int i = new Random().Next(0, files.Count - 1);
                            //if (j < 5 && i <= (files.Count - 1) / 2)
                            //{
                            //    MediaRandom.Add(mediaVM);
                            //    j++;
                            //}
                            MediaGroupedByAlphabet = Videos.OrderBy(x => x.AlphaKey).GroupBy(x => x.AlphaKey);
                        });
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine("An error occured while indexing a video folder");
                }
#if WINDOWS_APP
            }
#endif

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

                if (NewVideos.Any())
                {
                    Panels.Add(new Panel("new", 1, 0.4, App.Current.Resources["HomePath"].ToString()));
                }
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
