using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.System.Threading;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Commands;
using VLC_WINRT.Utility.IoC;
using VLC_WINRT.Utility.Services.RunTime;

namespace VLC_WINRT.ViewModels.MainPage
{
    public class LastViewedViewModel : BindableBase
    {
        private readonly HistoryService _historyService;
        private ClearHistoryCommand _clearHistoryCommand;
        private bool _lastViewedSectionVisible;
        private ViewedVideoViewModel _lastViewedVM;
        private ViewedVideoViewModel _secondLastViewedVM;
        private ViewedVideoViewModel _thirdLastViewedVM;
        private bool _welcomeSectionVisible;

        public LastViewedViewModel()
        {
            _historyService = IoC.GetInstance<HistoryService>();

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
            var viewedMedia = await GetRecentMedia();

            DispatchHelper.Invoke(() =>
            {
                if (viewedMedia.Count > 0)
                    LastViewedVM = viewedMedia[0];
                if (viewedMedia.Count > 1)
                    SecondLastViewedVM = viewedMedia[1];
                if (viewedMedia.Count > 2)
                    ThirdLastViewedVM = viewedMedia[2];
            });
        }

        private async Task<List<ViewedVideoViewModel>> GetRecentMedia()
        {
            var viewedVideos = new List<ViewedVideoViewModel>();
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    string token = _historyService.GetTokenAtPosition(i);
                    StorageFile file = null;
                    if (!string.IsNullOrEmpty(token))
                    {
                        file = await _historyService.RetrieveFile(token);
                    }
                    if (file == null) continue;

                    viewedVideos.Add(new ViewedVideoViewModel(token, file));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Couldn't load file from history, we may no longer have acces to it.");
                    Debug.WriteLine(ex.ToString());
                }
            }
            return viewedVideos;
        }
    }
}