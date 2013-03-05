using System;
using VLC_WINRT.Common;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.System.Threading;

namespace VLC_WINRT.ViewModels.MainPage
{
    public class ViewedVideoViewModel : MediaViewModel
    {
        private TimeSpan _duration;
        private TimeSpan _timeWatched;

        public ViewedVideoViewModel(StorageFile storageFile) : base(storageFile)
        {
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
            var rand = new Random();
            int seconds = rand.Next((int) duration.TotalSeconds);

            DispatchHelper.Invoke(() =>
                                      {
                                          Duration = duration;
                                          TimeWatched = TimeSpan.FromSeconds(seconds);
                                      });
        }
    }
}