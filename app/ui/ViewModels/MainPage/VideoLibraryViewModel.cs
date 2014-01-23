using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using VLC_WINRT.Common;
using VLC_WINRT.Model;
using VLC_WINRT.Utility.Commands;
using VLC_WINRT.Utility.Services;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.System.Threading;
using VLC_WINRT.Utility.Services.RunTime;

namespace VLC_WINRT.ViewModels.MainPage
{
    public class VideoLibraryViewModel : BindableBase
    {
        private ObservableCollection<Panel> _panels = new ObservableCollection<Panel>(); 
        private bool _hasNoMedia;
        private StorageFolder _location;
        private ObservableCollection<MediaViewModel> _media;
        private PickVideoCommand _pickCommand = new PickVideoCommand();
        private string _title;

        public VideoLibraryViewModel(StorageFolder location)
        {
            Media = new ObservableCollection<MediaViewModel>();
            Location = location;

            Title = location.DisplayName;
            if (!string.IsNullOrEmpty(location.DisplayType))
                Title += " " + location.DisplayType;

            Title = Title.ToLower();

            //Get off UI thread
            ThreadPool.RunAsync(GetMedia);
            Panels.Add(new Panel("ALL", 0, 1));
            Panels.Add(new Panel("NEVER SEEN BEFORE", 1, 0.4));
            Panels.Add(new Panel("FAVORITE", 2, 0.4));
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

        public ObservableCollection<MediaViewModel> Media
        {
            get { return _media; }
            set { SetProperty(ref _media, value); }
        }

        public PickVideoCommand PickCommand
        {
            get { return _pickCommand; }
            set { SetProperty(ref _pickCommand, value); }
        }

        protected async void GetMedia(IAsyncAction operation)
        {
            IReadOnlyList<StorageFile> files =
                await GetMediaFromFolder(_location, CommonFileQuery.OrderByDate);

            if (files.Count > 0)
            {
                DispatchHelper.Invoke(() => HasNoMedia = false);
            }
            else
            {
                DispatchHelper.Invoke(() => HasNoMedia = true);
            }


            foreach (StorageFile storageFile in files)
            {
                var mediaVM = new MediaViewModel(storageFile);

                // Get back to UI thread
                DispatchHelper.Invoke(() => Media.Add(mediaVM));
            }
        }

        private static async Task<IReadOnlyList<StorageFile>> GetMediaFromFolder(StorageFolder folder,
                                                                            CommonFileQuery query)
        {
            IReadOnlyList<StorageFile> files = null;
            StorageFileQueryResult fileQuery;
            var queryOptions = new QueryOptions(query,
                                               new List<string> { ".mkv", ".mp4", ".m4v", ".avi", ".mp3", ".wav" });
            try
            {
                fileQuery = folder.CreateFileQueryWithOptions(queryOptions);

                files = await fileQuery.GetFilesAsync();
            }
            catch (ArgumentException ex)
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