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
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Commands;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.System.Threading;
using VLC_WINRT.Utility.Helpers;
using VLC_WINRT.Views.Controls.MainPage;
using Panel = VLC_WINRT.Model.Panel;

namespace VLC_WINRT.ViewModels.MainPage
{
    public class VideoLibraryViewModel : BindableBase
    {
        private ObservableCollection<Panel> _panels = new ObservableCollection<Panel>();
        private bool _hasNoMedia;
        private StorageFolder _location;
        private ObservableCollection<MediaViewModel> _media;
        private ObservableCollection<MediaViewModel> _mediaRandom;
        private IEnumerable<IGrouping<char, MediaViewModel>> _mediaGroupedByAlphabet;
        private PickVideoCommand _pickCommand = new PickVideoCommand();
        private string _title;

        public VideoLibraryViewModel(StorageFolder location)
        {
            Media = new ObservableCollection<MediaViewModel>();
            MediaRandom = new ObservableCollection<MediaViewModel>();

            Location = location;

            Title = location.DisplayName;
            if (!string.IsNullOrEmpty(location.DisplayType))
                Title += " " + location.DisplayType;

            Title = Title.ToLower();

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

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public StorageFolder Location
        {
            get { return _location; }
            set { SetProperty(ref _location, value); }
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
            try
            {
                IReadOnlyList<StorageFile> files =
                    await GetMediaFromFolder(_location, CommonFileQuery.OrderByName);

                if (files.Count > 0)
                {
                    await DispatchHelper.InvokeAsync(() => HasNoMedia = false);
                }
                else
                {
                    await DispatchHelper.InvokeAsync(() => HasNoMedia = true);
                }

                int j = 0;
                foreach (StorageFile storageFile in files)
                {
                    var mediaVM = new MediaViewModel(storageFile);

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
                        ExecuteSemanticZoom();
                    });
                }
            }
            catch
            {
               
            }
        }

        public void ExecuteSemanticZoom()
        {
            var page = App.ApplicationFrame.Content as Views.MainPage;
            if (page == null) return;
            var videoColumn = page.GetFirstDescendantOfType<VideoColumn>() as VideoColumn;
            var semanticZoom = videoColumn.GetDescendantsOfType<SemanticZoom>().First() as SemanticZoom;
            var semanticZoomVertical = videoColumn.GetDescendantsOfType<SemanticZoom>().ElementAt(1) as SemanticZoom;
            var collection = videoColumn.Resources["MediaGroupedByAlphabet"] as CollectionViewSource;
            if (semanticZoom == null) return;
            try
            {
                var listviewbase = semanticZoom.ZoomedOutView as ListViewBase;
                var listviewBaseVertical = semanticZoomVertical.ZoomedOutView as ListViewBase;
                if (collection == null) return;
                // Collection or Collection View can also be null. In these cases, return.
                if (collection.View == null) return;
                if (listviewbase != null) 
                    listviewbase.ItemsSource = collection.View.CollectionGroups;
                if (listviewBaseVertical != null)
                    listviewBaseVertical.ItemsSource = collection.View.CollectionGroups;
            }
            catch { }
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
