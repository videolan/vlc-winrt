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
    public class LastViewedViewModel : BindableBase, IDisposable
    {
        private readonly HistoryService _historyService;
        private ClearHistoryCommand _clearHistoryCommand;
        private bool _lastViewedSectionVisible;
        private List<ViewedVideoViewModel> _lastViewedVideos;
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
                WelcomeSectionVisible = true;
            }
            _clearHistoryCommand = new ClearHistoryCommand();
            _historyService.HistoryUpdated += UpdateHistory;
            //FIXME: Needs to be awaited on from outside CTOR
            UpdateHistory();
        }
        public void Dispose()
        {
            _historyService.HistoryUpdated -= UpdateHistory;
        }

        private async void UpdateHistory(object sender, EventArgs e)
        {
            await UpdateHistory();
        }

        private async Task UpdateHistory()
        {
            var viewedMedia = await GetRecentMedia();

            await DispatchHelper.InvokeAsync(() =>
            {
                LastViewedVM = viewedMedia;
            });
        }

        public List<ViewedVideoViewModel> LastViewedVM
        {
            get { return _lastViewedVideos; }
            set { SetProperty(ref _lastViewedVideos, value); }
        }


        public ClearHistoryCommand ClearHistory
        {
            get { return _clearHistoryCommand; }
            set { SetProperty(ref _clearHistoryCommand, value); }
        }

        public bool WelcomeSectionVisible
        {
            get { return _welcomeSectionVisible; }
            set { SetProperty(ref _welcomeSectionVisible, value); }
        }

        public bool LastViewedSectionVisible
        {
            get { return _lastViewedSectionVisible; }
            set { SetProperty(ref _lastViewedSectionVisible, value); }
        }

        private async Task<List<ViewedVideoViewModel>> GetRecentMedia()
        {
            var viewedVideos = new List<ViewedVideoViewModel>();
            for (int i = 0; i < 4; i++)
            {
                if (!_historyService.IsAudioAtPosition(i))
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

                        var video = new ViewedVideoViewModel(token, file);
                        await video.Initialize();
                        viewedVideos.Add(video);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Couldn't load file from history, we may no longer have acces to it.");
                        Debug.WriteLine(ex.ToString());
                    }
                }
            }
            return viewedVideos;
        }
    }
}
