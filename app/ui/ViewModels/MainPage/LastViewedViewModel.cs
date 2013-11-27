using System;
using Microsoft.Practices.ServiceLocation;
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
        private bool _lastViewedSectionVisible;
        private ViewedVideoViewModel _secondLastViewedVM;
        private ViewedVideoViewModel _thirdLastViewedVM;
        private bool _welcomeSectionVisible;
        private readonly HistoryService _historyService;

        public LastViewedViewModel()
        {
            _historyService = ServiceLocator.Current.GetInstance<HistoryService>();

            if (_historyService.FileCount() > 0)
            {
                LastViewedSectionVisible = true;
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

        public bool LastViewedSectionVisible
        {
            get { return _lastViewedSectionVisible; }
            set { SetProperty(ref _lastViewedSectionVisible, value); }
        }

        private async void GetLastViewedMedia(IAsyncAction operation)
        {

            string firstToken = _historyService.GetTokenAtPosition(0);
            string secondToken = _historyService.GetTokenAtPosition(1);
            string thirdToken = _historyService.GetTokenAtPosition(2);

            StorageFile firstFile = null;
            if (!string.IsNullOrEmpty(firstToken))
                firstFile = await _historyService.RetrieveFile(firstToken);

            StorageFile secondFile = null;
            if (!string.IsNullOrEmpty(secondToken))
                secondFile = await _historyService.RetrieveFile(secondToken);

            StorageFile thirdFile = null;
            if (!string.IsNullOrEmpty(thirdToken))
                thirdFile = await _historyService.RetrieveFile(thirdToken);

            DispatchHelper.Invoke(() =>
                                      {
                                          if (firstFile != null)
                                              LastViewedVM = new ViewedVideoViewModel(firstToken, firstFile);
                                          if (secondFile != null)
                                              SecondLastViewedVM = new ViewedVideoViewModel(secondToken, secondFile);
                                          if (thirdFile != null)
                                              ThirdLastViewedVM = new ViewedVideoViewModel(thirdToken, thirdFile);
                                      });
        }
    }
}