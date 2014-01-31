using System;
using Windows.Storage;
using VLC_WINRT.Common;
using Windows.Foundation;
using Windows.System.Threading;
using VLC_WINRT.Model;
using VLC_WINRT.Utility.IoC;
using VLC_WINRT.Utility.Services.RunTime;

namespace VLC_WINRT.ViewModels.MainPage
{
    public class ViewedVideoViewModel : MediaViewModel
    {
        private string _token;

        public ViewedVideoViewModel(string token, StorageFile file)
            : base(file)
        {
            _token = token;
            ThreadPool.RunAsync(GatherTimeInformation);
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

        private async void GatherTimeInformation(IAsyncAction operation)
        {
            var historyService = IoC.GetInstance<HistoryService>();
            MediaHistory history = historyService.GetHistory(_token);

            DispatchHelper.Invoke(() =>
                                      {
                                          TimeWatched = TimeSpan.FromMilliseconds(history.TotalWatchedMilliseconds);
                                      });
        }
    }
}