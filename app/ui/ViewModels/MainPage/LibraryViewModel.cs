using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VLC_WINRT.Common;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.System.Threading;

namespace VLC_WINRT.ViewModels.MainPage
{
    public class LibraryViewModel : BindableBase
    {
        //TODO: provide a better way to describe to the app what file types are supported
        private readonly List<string> ValidFiles = new List<string> {".m4v", ".mp4", ".mp3", ".avi"};
        private StorageFolder _location;
        private ObservableCollection<MediaViewModel> _media;

        public LibraryViewModel(StorageFolder location)
        {
            Media = new ObservableCollection<MediaViewModel>();
            Location = location;

            //Get off UI thread
            ThreadPool.RunAsync(GetMedia);
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

        private async void GetMedia(IAsyncAction operation)
        {
            IReadOnlyList<StorageFile> files = await _location.GetFilesAsync(CommonFileQuery.OrderByDate);
            IEnumerable<StorageFile> validFiles = files.Where(file => ValidFiles.Contains(file.FileType)).Take(5);

            foreach (StorageFile storageFile in validFiles)
            {
                var mediaVM = new MediaViewModel();

                mediaVM.Title = storageFile.Name;
                mediaVM.Subtitle = storageFile.FileType.ToUpper() + " File";
                mediaVM.File = storageFile;

                // Get back to UI thread
                DispatchHelper.Invoke(() => Media.Add(mediaVM));
            }
        }
    }
}