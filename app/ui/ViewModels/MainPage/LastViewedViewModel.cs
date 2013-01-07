using System;
using System.Collections.Generic;
using VLC_WINRT.Common;
using VLC_WINRT.Services;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.System.Threading;

namespace VLC_WINRT.ViewModels.MainPage
{
    public class LastViewedViewModel : BindableBase
    {
        private MediaViewModel _lastViewedVM;
        private MediaViewModel _secondLastViewedVM;
        private MediaViewModel _thirdLastViewedVM;

        public LastViewedViewModel()
        {
            //TODO: implement actual last viewed functionaliy
            //Get off UI thread
            ThreadPool.RunAsync(AddRandomVideos);
        }

        public MediaViewModel LastViewedVM
        {
            get { return _lastViewedVM; }
            set { SetProperty(ref _lastViewedVM, value); }
        }

        public MediaViewModel SecondLastViewedVM
        {
            get { return _secondLastViewedVM; }
            set { SetProperty(ref _secondLastViewedVM, value); }
        }

        public MediaViewModel ThirdLastViewedVM
        {
            get { return _thirdLastViewedVM; }
            set { SetProperty(ref _thirdLastViewedVM, value); }
        }

        private async void AddRandomVideos(IAsyncAction operation)
        {
            var rand = new Random();
            var scanner = new MediaFolderScanner();
            List<StorageFile> files =
                await scanner.GetMediaFromFolder(KnownVLCLocation.VideosLibrary, int.MaxValue, CommonFileQuery.OrderByTitle);

            if (files.Count > 3)
            {
                StorageFile firstFile = files[rand.Next(files.Count)];
                StorageFile secondFile = files[rand.Next(files.Count)];
                StorageFile thirdFile = files[rand.Next(files.Count)];

                DispatchHelper.Invoke(() => { LastViewedVM = new MediaViewModel(firstFile); });
                DispatchHelper.Invoke(() => { SecondLastViewedVM = new MediaViewModel(secondFile); });
                DispatchHelper.Invoke(() => { ThirdLastViewedVM = new MediaViewModel(thirdFile); });
            }
        }
    }
}