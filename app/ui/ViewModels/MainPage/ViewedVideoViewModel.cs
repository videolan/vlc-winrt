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
using Windows.Storage;
using VLC_WINRT.Common;
using Windows.Foundation;
using Windows.System.Threading;
using VLC_WINRT.Model;
using VLC_WINRT.Utility.Services.RunTime;
using System.Threading.Tasks;

namespace VLC_WINRT.ViewModels.MainPage
{
    public class ViewedVideoViewModel : MediaViewModel
    {
        private string _token;

        public ViewedVideoViewModel(string token, StorageFile file)
            : base(file)
        {
            _token = token;
        }

        public double PortionWatched
        {
            get
            {
                double timeWatchedms = TimeWatched.TotalMilliseconds;
                double totalms = Duration.TotalMilliseconds;
                return (timeWatchedms/totalms)*100.0f;
            }
        }

        private async Task GatherTimeInformation()
        {
            var historyService = App.Container.Resolve<HistoryService>();
            await historyService.RestoreHistory();
            MediaHistory history = historyService.GetHistory(_token);

            DispatchHelper.Invoke(() =>
                                      {
                                          TimeWatched = TimeSpan.FromMilliseconds(history.TotalWatchedMilliseconds);
                                      });
        }
    }
}
