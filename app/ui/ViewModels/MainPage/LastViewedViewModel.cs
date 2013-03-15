using System;
using System.Collections.Generic;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Services;
using VLC_WINRT.Utility.Services.RunTime;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.System.Threading;

namespace VLC_WINRT.ViewModels.MainPage
{
    public class LastViewedViewModel : BindableBase
    {
        private ViewedVideoViewModel _lastViewedVM;
        private bool _lastViewedVisible;
        private ViewedVideoViewModel _secondLastViewedVM;
        private ViewedVideoViewModel _thirdLastViewedVM;
        private bool _welcomeSectionVisible;

        public LastViewedViewModel()
        {
            var history = new HistoryService();
            if (history.FileCount() > 0)
            {
                LastViewedVisible = true;
            }
            else
            {
                WelcomeSectionVisibile = true;
            }
            //TODO: implement actual last viewed functionaliy
            //Get off UI thread
            ThreadPool.RunAsync(AddRandomVideos);
        }

        public ViewedVideoViewModel LastViewedVM
        {
            get { return _lastViewedVM; }
            set { SetProperty(ref _lastViewedVM, value); }
        }

        public ViewedVideoViewModel SecondLastViewedVM
        {
            get { return _secondLastViewedVM; }
            set { SetProperty(ref _secondLastViewedVM, value); }
        }

        public ViewedVideoViewModel ThirdLastViewedVM
        {
            get { return _thirdLastViewedVM; }
            set { SetProperty(ref _thirdLastViewedVM, value); }
        }

        public bool WelcomeSectionVisibile
        {
            get { return _welcomeSectionVisible; }
            set { SetProperty(ref _welcomeSectionVisible, value); }
        }

        public bool LastViewedVisible
        {
            get { return _lastViewedVisible; }
            set { SetProperty(ref _lastViewedVisible, value); }
        }

        private async void AddRandomVideos(IAsyncAction operation)
        {
            var rand = new Random();
            IReadOnlyList<StorageFile> files =
                await
                MediaScanner.GetMediaFromFolder(KnownVLCLocation.VideosLibrary, uint.MaxValue,
                                                CommonFileQuery.OrderByTitle);

            if (files.Count >= 3)
            {
                StorageFile firstFile = files[rand.Next(files.Count)];
                StorageFile secondFile = files[rand.Next(files.Count)];
                StorageFile thirdFile = files[rand.Next(files.Count)];

                DispatchHelper.Invoke(() => { LastViewedVM = new ViewedVideoViewModel(firstFile); });
                DispatchHelper.Invoke(() => { SecondLastViewedVM = new ViewedVideoViewModel(secondFile); });
                DispatchHelper.Invoke(() => { ThirdLastViewedVM = new ViewedVideoViewModel(thirdFile); });
            }
        }
    }
}