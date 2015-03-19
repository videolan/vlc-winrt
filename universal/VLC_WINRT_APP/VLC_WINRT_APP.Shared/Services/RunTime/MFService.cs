using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using libVLCX;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Model.Stream;
using VLC_WINRT_APP.Services.Interface;

namespace VLC_WINRT_APP.Services.RunTime
{
    public class MFService : IMediaService
    {
        public MediaElement Instance { get; private set; }
        public event EventHandler MediaFailed;
        public event Action OnStopped;
        public event Action<long> OnLengthChanged;
        public event Action OnEndReached;

        public event EventHandler<MediaState> StatusChanged;
        public event TimeChanged TimeChanged;

        public TaskCompletionSource<bool> PlayerInstanceReady { get; set; }

        public bool IsBackground
        {
            get { throw new NotImplementedException(); }
        }

        private DispatcherTimer dispatchTimer;

        public MFService()
        {
            PlayerInstanceReady = new TaskCompletionSource<bool>();
        }

        public void Initialize(object mediaElement)
        {
            var mE = mediaElement as MediaElement;
            if (mE == null) throw new ArgumentNullException("mediaElement", "MediaFoundationService needs a MediaElement");
            Instance = mE;
            Instance.MediaFailed += Instance_MediaFailed;
            Instance.MediaOpened += Instance_MediaOpened;
            Instance.CurrentStateChanged += Instance_CurrentStateChanged;
            Instance.MediaEnded += Instance_MediaEnded;
            PlayerInstanceReady.SetResult(true);
            App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                dispatchTimer = new DispatcherTimer()
                {
                    Interval = TimeSpan.FromSeconds(1),
                };
                dispatchTimer.Tick += dispatchTimer_Tick;
            });
        }

        void Instance_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (OnLengthChanged != null)
                OnLengthChanged((long)GetLength());
        }

        private void Instance_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (OnEndReached != null)
                OnEndReached();
            if (OnStopped != null)
                OnStopped();
        }

        void dispatchTimer_Tick(object sender, object e)
        {
            if (TimeChanged != null)
                TimeChanged(GetTime());
        }

        void Instance_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            if (MediaFailed != null)
            {
                MediaFailed(this, new EventArgs());
            }
        }

        public async Task SetMediaFile(IVLCMedia media)
        {
            if (Instance == null) return;
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
                    await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Instance.SetSource(randomAccessStream, randomAccessStream.ContentType));
                }
            }
        }

        void Instance_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            switch (Instance.CurrentState)
            {
                case MediaElementState.Closed:
                    if (StatusChanged != null)
                        StatusChanged(this, MediaState.NothingSpecial);
                    break;
                case MediaElementState.Opening:
                    if (StatusChanged != null)
                        StatusChanged(this, MediaState.Opening);
                    break;
                case MediaElementState.Buffering:
                    if (StatusChanged != null)
                        StatusChanged(this, MediaState.Buffering);
                    break;
                case MediaElementState.Playing:
                    if (StatusChanged != null)
                        StatusChanged(this, MediaState.Playing);
                    dispatchTimer.Start();
                    break;
                case MediaElementState.Paused:
                    if (StatusChanged != null)
                        StatusChanged(this, MediaState.Paused);
                    dispatchTimer.Stop();
                    break;
                case MediaElementState.Stopped:
                    if (StatusChanged != null)
                        StatusChanged(this, MediaState.Stopped);
                    dispatchTimer.Stop();
                    if (OnStopped != null)
                        OnStopped();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public async void Play()
        {
            if (Instance == null) return;
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Instance.Play());
        }

        public void Pause()
        {
            // vlc pause() method is a play/pause toggle. we reproduce the same behaviour here
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
        }

        public void Stop()
        {
            if (Instance == null) return;
            dispatchTimer.Stop();
            Instance.Stop();
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

        public void SkipAhead()
        {
            if (Instance == null) return;
            Instance.Position = Instance.Position.Add(TimeSpan.FromSeconds(10));
        }

        public void SkipBack()
        {
            if (Instance == null) return;
            Instance.Position = TimeSpan.FromSeconds(Instance.Position.TotalSeconds - 10);
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
            vol = vol / 100; Instance.Volume = vol;
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
