using VLC_WINRT.Common;
using VLC_WINRT.Utility.Commands;
using VLC_WINRT.Utility.Services.RunTime;
using Windows.Foundation;
using Windows.Storage;
using Windows.System.Threading;

namespace VLC_WINRT.ViewModels.MainPage
{
    public class LastViewedViewModel : BindableBase
    {
        private ClearHistoryCommand _clearHistoryCommand;
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

            _clearHistoryCommand = new ClearHistoryCommand();

            //Get off UI thread
            ThreadPool.RunAsync(GetLastViewedMedia);
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

        public ClearHistoryCommand ClearHistory
        {
            get { return _clearHistoryCommand; }
            set { SetProperty(ref _clearHistoryCommand, value); }
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

        private async void GetLastViewedMedia(IAsyncAction operation)
        {
            var histserv = new HistoryService();

            StorageFile firstFile = await histserv.RetrieveFileAt(0);
            StorageFile secondFile = await histserv.RetrieveFileAt(1);
            StorageFile thirdFile = await histserv.RetrieveFileAt(2);

            DispatchHelper.Invoke(() =>
                                      {
                                          if (firstFile != null)
                                              LastViewedVM = new ViewedVideoViewModel(firstFile);
                                          if (secondFile != null)
                                              SecondLastViewedVM = new ViewedVideoViewModel(secondFile);
                                          if (thirdFile != null)
                                              ThirdLastViewedVM = new ViewedVideoViewModel(thirdFile);
                                      });
        }
    }
}