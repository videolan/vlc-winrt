/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Autofac;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Commands;
using VLC_WINRT.Utility.Services.RunTime;
#if WINDOWS_PHONE_APP
using VLC_WINPRT;
#endif

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
            _historyService = App.Container.Resolve<HistoryService>();

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
        }
        public void Dispose()
        {
            _historyService.HistoryUpdated -= UpdateHistory;
        }

        public async Task Initialize()
        {
            await UpdateHistory();
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
            var badTokens = new List<string>();
            var max = 4;
            for (int i = 0; i < max; i++)
            {
                if (!_historyService.IsAudioAtPosition(i))
                {
                    try
                    {
                        var token = _historyService.GetTokenAtPosition(i);
                        if (string.IsNullOrEmpty(token))
                            continue;

                        StorageFile file = null;
                        var fileException = false;
                        if (!string.IsNullOrEmpty(token))
                        {
                            try
                            {
                                file = await _historyService.RetrieveFile(token);
                            }
                            catch (System.IO.FileNotFoundException)
                            {
                                fileException = true;
                            }
                        }

                        if (file == null || fileException)
                        {
                            badTokens.Add(token);
                            max++;
                            continue;
                        }

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

            if (badTokens.Any())
            {
                foreach (var token in badTokens)
                    await _historyService.RemoveToken(token);
            }

            return viewedVideos;
        }
    }
}
