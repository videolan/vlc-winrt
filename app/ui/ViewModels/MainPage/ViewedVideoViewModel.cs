using System;
using Windows.Storage;
using VLC_WINRT.Common;
using Windows.Foundation;
using Windows.Storage.FileProperties;
using Windows.System.Threading;
using VLC_WINRT.Model;
using VLC_WINRT.Utility.IoC;
using VLC_WINRT.Utility.Services.RunTime;

namespace VLC_WINRT.ViewModels.MainPage
{
    public class ViewedVideoViewModel : MediaViewModel
    {
        private TimeSpan _duration;
        private TimeSpan _timeWatched;
        private string _token;

        public ViewedVideoViewModel(string token, StorageFile file)
            : base(file)
        {
            _token = token;
            ThreadPool.RunAsync(GatherTimeInformation);
        }

        public TimeSpan TimeWatched
        {
            get { return _timeWatched; }
            set
            {
                SetProperty(ref _timeWatched, value);
                OnPropertyChanged("PortionWatched");
            }
        }

        public TimeSpan Duration
        {
            get { return _duration; }
            set { SetProperty(ref _duration, value); }
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
            VideoProperties videoProps = await File.Properties.GetVideoPropertiesAsync();
            TimeSpan duration = videoProps.Duration;
            var historyService = IoC.GetInstance<HistoryService>();
            MediaHistory history=  historyService.GetHistory(_token);

            DispatchHelper.Invoke(() =>
                                      {
                                          Duration = duration;
                                          TimeWatched = TimeSpan.FromMilliseconds(history.TotalWatchedMilliseconds);
                                      });
        }
    }
}