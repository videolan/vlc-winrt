using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using VLC_WinRT.Model;
using VLC_WinRT.Model.Stream;
using VLC_WinRT.Services.Interface;
using libVLCX;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Services.RunTime
{
    public class MFService : IMediaService
    {
        public MediaElement Instance
        {
            get { return null; }
        }

        public event EventHandler MediaFailed;
        public event Action<IMediaService> OnStopped;
        public event Action<long> OnLengthChanged;
        public event Action OnEndReached;
        public event Action<int> OnBuffering;

        public event EventHandler<MediaState> StatusChanged;
        public event TimeChanged TimeChanged;

        public TaskCompletionSource<bool> PlayerInstanceReady { get; set; }

        private DispatcherTimer dispatchTimer;

        public MFService()
        {
            PlayerInstanceReady = new TaskCompletionSource<bool>();
        }

        public Task Initialize(object mediaElement)
        {
            var mE = mediaElement as MediaElement;
            if (mE == null) throw new ArgumentNullException("mediaElement", "MediaFoundationService needs a MediaElement");

            Instance.MediaFailed += Instance_MediaFailed;
            Instance.MediaOpened += Instance_MediaOpened;
            Instance.CurrentStateChanged += Instance_CurrentStateChanged;
            Instance.MediaEnded += Instance_MediaEnded;
            Instance.BufferingProgressChanged += Instance_BufferingProgressChanged;
            PlayerInstanceReady.SetResult(true);
            return DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                dispatchTimer = new DispatcherTimer()
                {
                    Interval = TimeSpan.FromSeconds(1),
                };
                dispatchTimer.Tick += dispatchTimer_Tick;
            });
        }

        void Instance_BufferingProgressChanged(object sender, RoutedEventArgs e)
        {
            OnBuffering?.Invoke((int)(Instance.BufferingProgress * 100));
        }

        void Instance_MediaOpened(object sender, RoutedEventArgs e)
        {
            OnBuffering?.Invoke(100);
            OnLengthChanged?.Invoke((long)GetLength());
        }

        private void Instance_MediaEnded(object sender, RoutedEventArgs e)
        {
            OnEndReached?.Invoke();
            OnStopped?.Invoke(this);
        }

        void dispatchTimer_Tick(object sender, object e)
        {
            TimeChanged?.Invoke(GetTime());
        }

        void Instance_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MediaFailed?.Invoke(this, new EventArgs());
        }

        public async Task SetMediaFile(IVLCMedia media)
        {
            var tcs = new TaskCompletionSource<bool>();
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            { tcs.SetResult(Instance != null); });
            var instanceExists = await tcs.Task;
            if (!instanceExists) return;

            RandomAccessStreamReference randomAccessStreamReference = null;
            if (media is StreamMedia)
            {
                randomAccessStreamReference = RandomAccessStreamReference.CreateFromUri(new Uri(media.Path));
            }
            else
            {
                if (media.File != null)
                {
                    randomAccessStreamReference = RandomAccessStreamReference.CreateFromFile(media.File);
                }
                else
                {
                    var file = await StorageFile.GetFileFromPathAsync(media.Path);
                    if (file != null)
                        randomAccessStreamReference = RandomAccessStreamReference.CreateFromFile(file);
                }
            }
            if (randomAccessStreamReference != null)
            {
                var randomAccessStream = await randomAccessStreamReference.OpenReadAsync();
                if (randomAccessStream != null)
                {
                    await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        try
                        {
                            Instance.AutoPlay = false;
                            Instance.SetSource(randomAccessStream, randomAccessStream.ContentType);
                        }
                        catch { }
                    });
                }
            }
        }

        void Instance_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            switch (Instance.CurrentState)
            {
                case MediaElementState.Closed:
                    StatusChanged?.Invoke(this, MediaState.NothingSpecial);
                    break;
                case MediaElementState.Opening:
                    StatusChanged?.Invoke(this, MediaState.Opening);
                    break;
                case MediaElementState.Buffering:
                    StatusChanged?.Invoke(this, MediaState.Buffering);
                    break;
                case MediaElementState.Playing:
                    StatusChanged?.Invoke(this, MediaState.Playing);
                    dispatchTimer?.Start();
                    break;
                case MediaElementState.Paused:
                    StatusChanged?.Invoke(this, MediaState.Paused);
                    dispatchTimer?.Stop();
                    break;
                case MediaElementState.Stopped:
                    StatusChanged?.Invoke(this, MediaState.Stopped);
                    dispatchTimer?.Stop();
                    OnStopped?.Invoke(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public async void Play()
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (Instance == null) return;
                Instance.MediaOpened += (sender, args) => Instance.Play();
            });
        }


        public void Play(int trackId)
        {
            throw new NotImplementedException();
        }

        public async void Pause()
        {
            // vlc pause() method is a play/pause toggle. we reproduce the same behaviour here
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (Instance == null) return;
                switch (Instance.CurrentState)
                {
                    case MediaElementState.Playing:
                        Instance.Pause();
                        break;
                    case MediaElementState.Paused:
                        Instance.Play();
                        break;
                }
            });
        }

        public async void Stop()
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (Instance == null) return;
                dispatchTimer?.Stop();
                Instance.Stop();
            });
        }

        public void SetNullMediaPlayer()
        {
        }

        public void FastForward()
        {
            throw new NotImplementedException();
        }

        public void Rewind()
        {
            throw new NotImplementedException();
        }

        public async void SkipAhead()
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (Instance == null) return;
                Instance.Position = Instance.Position.Add(TimeSpan.FromSeconds(10));
            });
        }

        public async void SkipBack()
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (Instance == null) return;
                Instance.Position = TimeSpan.FromSeconds(Instance.Position.TotalSeconds - 10);
            });
        }

        public float GetLength()
        {
            if (Instance.NaturalDuration.HasTimeSpan)
                return (float)Instance.NaturalDuration.TimeSpan.TotalMilliseconds;
            return 0f;
        }

        public void SetTime(long desiredTime)
        {
            if (Instance == null) return;
            Instance.Position = TimeSpan.FromMilliseconds(desiredTime);
        }

        public long GetTime()
        {
            return (Instance == null) ? 0 : (long)Instance.Position.TotalMilliseconds;
        }

        public float GetPosition()
        {
            if (Instance == null) return 0.0f;
            var pos = (float)Instance.Position.TotalSeconds;
            var dur = (float)Instance.NaturalDuration.TimeSpan.TotalSeconds;
            return pos / dur;
        }

        public void SetPosition(float desiredPosition)
        {
            if (Instance == null) return;
            var posInTimeSpan = Instance.NaturalDuration.TimeSpan.TotalSeconds * desiredPosition;
            Instance.Position = TimeSpan.FromSeconds(posInTimeSpan);
        }

        public int GetVolume()
        {
            if (Instance == null)
                return 0;
            return (int)(Instance.Volume * 100);
        }

        public void SetVolume(int volume)
        {
            if (Instance == null) return;
            var vol = (double)volume;
            vol = vol / 100;
            Instance.Volume = vol;
        }

        public void SetSpeedRate(float desiredRate)
        {
            if (Instance == null) return;
            Instance.PlaybackRate = desiredRate;
        }

        public void Trim()
        {
            throw new NotImplementedException();
        }

        public void SetSizeVideoPlayer(uint x, uint y)
        {
            // MediaElement resizes automatically, nothing to do here
        }
    }
}
