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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Utility.Commands;
using Windows.Storage;
using Windows.Storage.Search;
using VLC_WINRT_APP.Utility.Helpers;
using VLC_WINRT.ViewModels.Settings;
using VLC_WINRT_APP;
using VLC_WINRT_APP.ViewModels;
using Panel = VLC_WINRT.Model.Panel;
#if WINDOWS_PHONE_APP

#endif
#if NETFX_CORE
using VLC_WINRT.Views.Controls.MainPage;
#endif

namespace VLC_WINRT.ViewModels.MainPage
{
    public class VideoLibraryViewModel : BindableBase
    {
        private ObservableCollection<Panel> _panels = new ObservableCollection<Panel>();
        private bool _hasNoMedia = true;
        private ObservableCollection<MediaViewModel> _media;
        private ObservableCollection<MediaViewModel> _mediaRandom;
        private IEnumerable<IGrouping<char, MediaViewModel>> _mediaGroupedByAlphabet;
        private PickVideoCommand _pickCommand = new PickVideoCommand();

        public VideoLibraryViewModel()
        {
            Media = new ObservableCollection<MediaViewModel>();
            MediaRandom = new ObservableCollection<MediaViewModel>();
            Panels.Add(new Panel("ALL", 0, 1));
            Panels.Add(new Panel("NEVER SEEN BEFORE", 1, 0.4));
            Panels.Add(new Panel("FAVORITE", 2, 0.4));
        }

        public bool IsVideoLibraryEmpty
        {
            get { return Media.Any(); }
        }

        public ObservableCollection<Panel> Panels
        {
            get { return _panels; }
            set
            {
                SetProperty(ref _panels, value);
            }
        }
        public bool HasNoMedia
        {
            get { return _hasNoMedia; }
            set { SetProperty(ref _hasNoMedia, value); }
        }

        public IEnumerable<IGrouping<char, MediaViewModel>> MediaGroupedByAlphabet
        {
            get { return _mediaGroupedByAlphabet; }
            set { SetProperty(ref _mediaGroupedByAlphabet, value); }
        }
        public ObservableCollection<MediaViewModel> Media
        {
            get { return _media; }
            set { SetProperty(ref _media, value); }
        }

        public ObservableCollection<MediaViewModel> MediaRandom
        {
            get { return _mediaRandom; }
            set { SetProperty(ref _mediaRandom, value); }
        }

        public PickVideoCommand PickCommand
        {
            get { return _pickCommand; }
            set { SetProperty(ref _pickCommand, value); }
        }

        public async Task GetMedia()
        {
            foreach (CustomFolder folder in Locator.SettingsVM.VideoFolders)
            {
                try
                {
                    StorageFolder customVideoFolder;
                    if (folder.Mru == "Video Library")
                    {
                        customVideoFolder = KnownVLCLocation.VideosLibrary;
                    }
                    else
                    {
                        customVideoFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(
                            folder.Mru);
                    }
                    IReadOnlyList<StorageFile> files =
                        await GetMediaFromFolder(customVideoFolder, CommonFileQuery.OrderByName);
                    
                    int j = 0;
                    foreach (StorageFile storageFile in files)
                    {
                        var mediaVM = new MediaViewModel();
                        mediaVM.Initialize(storageFile);
                        await mediaVM.Initialize();
                        // Get back to UI thread
                        await DispatchHelper.InvokeAsync(() =>
                        {
                            Media.Add(mediaVM);
                            int i = new Random().Next(0, files.Count - 1);
                            if (j < 5 && i <= (files.Count - 1) / 2)
                            {
                                MediaRandom.Add(mediaVM);
                                j++;
                            }
                            MediaGroupedByAlphabet = Media.OrderBy(x => x.AlphaKey).GroupBy(x => x.AlphaKey);
                        });
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine("An error occured while indexing a video folder");
                }
            }

            if (Media.Count > 0)
            {
                DispatchHelper.InvokeAsync(() => HasNoMedia = false);
            }
            else
            {
                DispatchHelper.InvokeAsync(() => HasNoMedia = true);
            }
            ExecuteSemanticZoom();
        }

        public void ExecuteSemanticZoom()
        {
#if NETFX_CORE
            //var page = App.ApplicationFrame.Content as Views.MainPage;
            //if (page == null) return;
            //var videoColumn = page.GetFirstDescendantOfType<VideoColumn>() as VideoColumn;
            //var semanticZoom = videoColumn.GetDescendantsOfType<SemanticZoom>().First() as SemanticZoom;
            //var semanticZoomVertical = videoColumn.GetDescendantsOfType<SemanticZoom>().ElementAt(1) as SemanticZoom;
            //var collection = videoColumn.Resources["MediaGroupedByAlphabet"] as CollectionViewSource;
            //if (semanticZoom == null) return;
            //try
            //{
            //    var listviewbase = semanticZoom.ZoomedOutView as ListViewBase;
            //    var listviewBaseVertical = semanticZoomVertical.ZoomedOutView as ListViewBase;
            //    if (collection == null) return;
            //    // Collection or Collection View can also be null. In these cases, return.
            //    if (collection.View == null) return;
            //    if (listviewbase != null)
            //        listviewbase.ItemsSource = collection.View.CollectionGroups;
            //    if (listviewBaseVertical != null)
            //        listviewBaseVertical.ItemsSource = collection.View.CollectionGroups;
            //}
            //catch { }
#endif
        }

        private static async Task<IReadOnlyList<StorageFile>> GetMediaFromFolder(StorageFolder folder,
                                                                            CommonFileQuery query)
        {
            IReadOnlyList<StorageFile> files = null;
            StorageFileQueryResult fileQuery;
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

            try
            {
                fileQuery = folder.CreateFileQueryWithOptions(queryOptions);

                files = await fileQuery.GetFilesAsync();
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
    }
}
