using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Services;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.System.Threading;

namespace VLC_WINRT.ViewModels.MainPage
{
    public class ThumbnailsViewModel : BindableBase
    {
        private ObservableCollection<ThumbnailViewModel> _thumbnails;

        public ThumbnailsViewModel()
        {
            Thumbnails = new ObservableCollection<ThumbnailViewModel>();

            ThreadPool.RunAsync(GetMedia);
        }

        public ObservableCollection<ThumbnailViewModel> Thumbnails
        {
            get { return _thumbnails; }
            set { SetProperty(ref _thumbnails, value); }
        }

        private async void GetMedia(IAsyncAction operation)
        {
            //Todo: add other sources of videos
            var scanner = new MediaFolderScanner();

            //Pick 
            IList<StorageFile> files =
                await scanner.GetMediaFromFolder(KnownVLCLocation.VideosLibrary, 10, CommonFileQuery.OrderByDate);

            //build a grid of 30 or so random images
            List<StorageFile> randomFiles = new List<StorageFile>();
            Random rand = new Random();

            for (int i = 0; i < 300; i++)
            {
                int index = rand.Next(files.Count);
                Thumbnails.Add(new ThumbnailViewModel(files[index]));
            }
        }
    }
}