#if WINDOWS_PHONE_APP
using System;
using VLC_WinRT.Services.Interface;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using VLC_WinRT.Model;
using Windows.UI.Core;
using Windows.Storage.Streams;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using libVLCX;
using Windows.Media.Playback;
using System.Diagnostics;
using VLC_WinRT.Helpers;
using VLC_WinRT.BackgroundAudioPlayer.Model;
using VLC_WinRT.ViewModels;
using Windows.Foundation.Collections;
using VLC_WinRT.BackgroundHelpers;
using MediaPlayer = Windows.Media.Playback.MediaPlayer;

namespace VLC_WinRT.Services.RunTime
{
    public class BGPlayerService : IMediaService
    {
        public MediaPlayer Instance => BackgroundAudioHelper.Instance;

        public event EventHandler MediaFailed;
        public event Action<IMediaService> OnStopped;
        public event Action<long> OnLengthChanged;
        public event Action OnEndReached;
        public event Action<int> OnBuffering;

        public event EventHandler<MediaState> StatusChanged;
        public event TimeChanged TimeChanged;

        public TaskCompletionSource<bool> PlayerInstanceReady { get; set; }

        private DispatcherTimer dispatchTimer;

        // HACK
        // We cache those values to reduce the CPU consumption of the IPC
        private long time;
        private float position;
        private float length;
        private MediaPlayerState mediaState;
        public BGPlayerService()
        {
            PlayerInstanceReady = new TaskCompletionSource<bool>();
            Initialize(null);
        }

        public async Task Initialize(object mediaElement = null)
        {
            ApplicationSettingsHelper.SaveSettingsValue(BackgroundAudioConstants.AppState, BackgroundAudioConstants.ForegroundAppActive);

            AddMediaPlayerEventHandlers();
            
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                dispatchTimer = new DispatcherTimer()
                {
                    Interval = TimeSpan.FromSeconds(1),
                };
                dispatchTimer.Tick += dispatchTimer_Tick;
            });

            try
            {
                if (Instance?.CurrentState == MediaPlayerState.Playing)
                {
                    if (!dispatchTimer.IsEnabled)
                    {
                        await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            Locator.MediaPlaybackViewModel.IsPlaying = true;
                            Instance_CurrentStateChanged(null, new RoutedEventArgs());
                        });
                    }
                }
                if (Instance != null && PlayerInstanceReady.Task?.Status != TaskStatus.RanToCompletion)
                    PlayerInstanceReady.SetResult(true);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Subscribes to MediaPlayer events
        /// </summary>
        public void AddMediaPlayerEventHandlers()
        {
            if (Instance != null)
            {
                Instance.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
                Instance.MediaOpened += CurrentOnMediaOpened;
            }
            BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
        }

        private async void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            try
            {
                foreach (string key in e.Data.Keys)
                {
                    switch (key)
                    {
                        case BackgroundAudioConstants.BackgroundTaskStarted:
                            //Wait for Background Task to be initialized before starting playback
                            Debug.WriteLine("Background Task started");
                            if (PlayerInstanceReady.Task?.Status != TaskStatus.RanToCompletion)
                                PlayerInstanceReady.SetResult(true);
                            break;
                        case BackgroundAudioConstants.BackgroundTaskCancelled:
                            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () =>
                            {
                                Locator.MediaPlaybackViewModel.IsPlaying = false;
                            });
                            break;
                        case BackgroundAudioConstants.MFFailed:
                            LogHelper.Log("VLC process is aware MF Background Media Player failed to open the file : " + e.Data[key]);
                            await Locator.MediaPlaybackViewModel.SetMedia(Locator.MusicPlayerVM.CurrentTrack, true);
                            MediaFailed?.Invoke(this, new EventArgs());
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private async void CurrentOnMediaOpened(MediaPlayer sender, object args)
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () => Instance_MediaOpened(null, new RoutedEventArgs()));
        }

        private async void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () => Instance_CurrentStateChanged(null, new RoutedEventArgs()));
        }

        /// <summary>
        /// Unsubscribes to MediaPlayer events. Should run only on suspend
        /// </summary>
        private void RemoveMediaPlayerEventHandlers()
        {
            if (Instance != null)
                Instance.CurrentStateChanged -= this.MediaPlayer_CurrentStateChanged;
            BackgroundMediaPlayer.MessageReceivedFromBackground -= this.BackgroundMediaPlayer_MessageReceivedFromBackground;
        }

        async void Instance_BufferingProgressChanged(object sender, RoutedEventArgs e)
        {
            OnBuffering?.Invoke((int)(Instance?.BufferingProgress * 100));
            await Locator.MusicPlayerVM.UpdateTrackFromMF();
        }

        void Instance_MediaOpened(object sender, RoutedEventArgs e)
        {
            OnLengthChanged?.Invoke((long)GetLength());
            Locator.MusicPlayerVM.UpdateTrackFromMF();
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
        }

        void Instance_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            if (Instance == null) return;
            mediaState = Instance.CurrentState;
            switch (mediaState)
            {
                case MediaPlayerState.Playing:
                    Debug.WriteLine("Media State Changed: Playing");
                    StatusChanged?.Invoke(this, MediaState.Playing);
                    dispatchTimer?.Start();
                    break;
                case MediaPlayerState.Paused:
                    Debug.WriteLine("Media State Changed: Paused");
                    StatusChanged?.Invoke(this, MediaState.Paused);
                    dispatchTimer?.Stop();
                    break;
            }
        }

        public void Play()
        {
            var id = Locator.MusicPlayerVM.CurrentTrack?.Id;
            if (id.HasValue)
                Play(id.Value);
        }

        public async void Play(int trackId)
        {
            // todo : remove the mockup
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (Instance == null) return;
                try
                {
                    ValueSet messageDictionary = new ValueSet();
                    string ts = trackId.ToString();
                    messageDictionary.Add(BackgroundAudioConstants.PlayTrack, ts);
                    BackgroundMediaPlayer.SendMessageToBackground(messageDictionary);
                }
                catch (Exception e)
                {
                    return;
                }
            });
        }

        public async void Pause()
        {
            // vlc pause() method is a play/pause toggle. we reproduce the same behaviour here
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, async () =>
            {
                if (Instance == null) return;
                switch (mediaState)
                {
                    case MediaPlayerState.Closed:
                        // If MediaPlayer was closed, run it agaain
                        await Locator.MediaPlaybackViewModel.SetMedia(Locator.MediaPlaybackViewModel.CurrentMedia, false);
                        break;
                    case MediaPlayerState.Playing:
                        Instance?.Pause();
                        break;
                    case MediaPlayerState.Paused:
                        Instance?.Play();
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
                if (mediaState == MediaPlayerState.Playing)
                    Instance?.Pause();
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
                var seconds = Instance?.Position.TotalSeconds - 10;
                if (seconds != null && seconds.HasValue)
                    Instance.Position = TimeSpan.FromSeconds(seconds.Value);
            });
        }

        public float GetLength()
        {
            if (TimeSpan.Zero != Instance?.NaturalDuration)
            {
                length = (float)Instance?.NaturalDuration.TotalMilliseconds;
                return length;
            }
            return 0f;
        }

        public void SetTime(long desiredTime)
        {
            try
            {
                if (Instance == null) return;
                Instance.Position = TimeSpan.FromMilliseconds(desiredTime);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error with position : " + ex.Message);
            }
        }

        public long GetTime()
        {
            try
            {
                switch (mediaState)
                {
                    case MediaPlayerState.Playing:
                        time = (long)Instance?.Position.TotalMilliseconds;
                        return time;
                    case MediaPlayerState.Closed:
                        // TODO: Use saved time value to populate time field.
                        break;
                }
            }
            catch
            {
            }
            return 0;
        }

        public float GetPosition()
        {
            switch (mediaState)
            {
                case MediaPlayerState.Playing:
                    var pos = time / length;
                    float posfloat = (float)pos;
                    return posfloat;
                case MediaPlayerState.Closed:
                    break;
            }
            return 0;
        }

        public void SetPosition(float desiredPosition)
        {
            switch (mediaState)
            {
                case MediaPlayerState.Playing:
                case MediaPlayerState.Closed:
                    if (Instance?.NaturalDuration.TotalMilliseconds != 0)
                    {
                        var naturalMilliseconds = Instance?.NaturalDuration.TotalMilliseconds;
                        if (Instance != null && naturalMilliseconds != null && naturalMilliseconds.HasValue)
                            Instance.Position = TimeSpan.FromMilliseconds(desiredPosition * naturalMilliseconds.Value);
                    }
                    break;
            }
        }

        public int GetVolume()
        {
            if (Instance == null)
                return 0;
            return (int)(Instance?.Volume * 100);
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

        public void SetRepeat(bool value)
        {
            try
            {
                ValueSet messageDictionary = new ValueSet();
                messageDictionary.Add(BackgroundAudioConstants.Repeat, value);
                BackgroundMediaPlayer.SendMessageToBackground(messageDictionary);
            }
            catch
            {
            }
        }

        public void Trim()
        {
            throw new NotImplementedException();
        }

        public void SetSizeVideoPlayer(uint x, uint y)
        {
            // Background Player only renders sound
        }
    }
}
#endif