using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Windows.System.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using GalaSoft.MvvmLight.Command;
using Microsoft.Practices.ServiceLocation;
using VLC_WINRT.Utility.Commands;
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT.Views;
using VLC_Wrapper;

namespace VLC_WINRT.ViewModels.PlayVideo
{
    public class PlayVideoViewModel : NavigateableViewModel, IDisposable
    {
        private readonly DispatcherTimer _sliderPositionTimer = new DispatcherTimer();
        private readonly DispatcherTimer _fiveSecondTimer = new DispatcherTimer();
        private TimeSpan _elapsedTime = TimeSpan.Zero;
        private string _fileToken;
        private bool _isPlaying;
        private PlayPauseCommand _playOrPause;
        private RelayCommand _skipAhead;
        private RelayCommand _skipBack;
        private StopVideoCommand _stopVideoCommand;
        private TimeSpan _timeTotal = TimeSpan.Zero;
        private string _title;
        private Player _vlcPlayer;
        private bool _isVLCInitialized;
        private readonly DisplayRequest _displayAlwaysOnRequest;
        private readonly HistoryService _historyService;
        private SwapChainBackgroundPanel _panel;


        public PlayVideoViewModel()
        {
            _playOrPause = new PlayPauseCommand();
            _historyService = ServiceLocator.Current.GetInstance<HistoryService>();
            _stopVideoCommand = new StopVideoCommand();
            _displayAlwaysOnRequest = new DisplayRequest();

            _sliderPositionTimer.Tick += UpdatePosition;
            _sliderPositionTimer.Interval = TimeSpan.FromMilliseconds(16);

            _fiveSecondTimer.Tick += UpdateDate;
            _fiveSecondTimer.Interval = TimeSpan.FromSeconds(5);
        }

        public void Dispose()
        {
            if (_vlcPlayer != null)
            {
                Stop();
                _vlcPlayer.Dispose();
                _vlcPlayer = null;
            }
        }

        public async Task InitializeVLC()
        {
            _vlcPlayer = new Player(_panel);
            await _vlcPlayer.Initialize();
            _isVLCInitialized = true;
            string token = _historyService.GetTokenAtPosition(0);
            _vlcPlayer.Open("winrt://" + token);

            if (!_isVLCInitialized)
            {
                Debug.Assert(_isVLCInitialized);
                return;
            }

            _skipAhead = new RelayCommand(() =>
            {
                TimeSpan seekTo = ElapsedTime + TimeSpan.FromSeconds(10);
                double relativePosition = seekTo.TotalMilliseconds / TimeTotal.TotalMilliseconds;
                if (relativePosition > 1.0f)
                {
                    relativePosition = 1.0f;
                }
                Seek((float)relativePosition);
            });
            _skipBack = new RelayCommand(() =>
            {
                TimeSpan seekTo = ElapsedTime - TimeSpan.FromSeconds(10);
                double relativePosition = seekTo.TotalMilliseconds / TimeTotal.TotalMilliseconds;
                if (relativePosition < 0.0f)
                {
                    relativePosition = 0.0f;
                }
                Seek((float)relativePosition);
            });

            _fiveSecondTimer.Start();

            RaisePropertyChanged("TimeTotal");
      
        }

        public override void OnNavigatedFrom()
        {
            _fiveSecondTimer.Stop();
            _sliderPositionTimer.Stop();

            _isVLCInitialized = false;

            if (_vlcPlayer != null)
            {
                Stop();
                _vlcPlayer.Dispose();
                _vlcPlayer = null;
            }
        }

        private void UpdateDate(object sender, object e)
        {
            if (!string.IsNullOrEmpty(_fileToken))
            {
                _historyService.UpdateMediaHistory(_fileToken, ElapsedTime);
            }

            OnPropertyChanged("Now");
        }

        public double PositionInSeconds
        {
            get
            {
                if (_isVLCInitialized && _vlcPlayer != null)
                {
                    return _vlcPlayer.GetPosition()*TimeTotal.TotalSeconds;
                }
                else
                {
                    return 0.0d;
                }
            }
            set { Seek((float) (value/TimeTotal.TotalSeconds)); }
        }

        public string Now
        {
            get
            {
                return DateTime.Now.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern);
            }
        }

        public PlayPauseCommand PlayOrPause
        {
            get { return _playOrPause; }
            set { SetProperty(ref _playOrPause, value); }
        }

        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                SetProperty(ref _isPlaying, value);
                var mouseService = ServiceLocator.Current.GetInstance<MouseService>();
                if (value)
                {
                    _sliderPositionTimer.Start();
                    mouseService.HideMouse();
                    ProtectedDisplayCall(true);
                }
                else
                {
                    _sliderPositionTimer.Stop();
                    mouseService.RestoreMouse();
                    ProtectedDisplayCall(false);
                }
            }

        }

        private void ProtectedDisplayCall(bool shouldActivate)
        {
            if (_displayAlwaysOnRequest == null) return;
            try
            {
                if (shouldActivate)
                {
                    _displayAlwaysOnRequest.RequestActive();
                }
                else
                {
                    _displayAlwaysOnRequest.RequestRelease();
                }
            }

            catch (ArithmeticException badMathEx)
            {
                //  Work around for platform bug 

                Debug.WriteLine("display request failed again");
                Debug.WriteLine(badMathEx.ToString());
            }
        }

        public RelayCommand SkipAhead
        {
            get { return _skipAhead; }
            set { SetProperty(ref _skipAhead, value); }
        }

        public RelayCommand SkipBack
        {
            get { return _skipBack; }
            set { SetProperty(ref _skipBack, value); }
        }

        public string Title
        {
            get { return _title; }
            private set { SetProperty(ref _title, value); }
        }

        public void SetActiveVideoInfo(string token, string title)
        {
            _fileToken = token;
            Title = title;
        }

        public StopVideoCommand StopVideo
        {
            get { return _stopVideoCommand; }
            set { SetProperty(ref _stopVideoCommand, value); }
        }

        public TimeSpan TimeTotal
        {
            get { return _timeTotal; }
            set { SetProperty(ref _timeTotal, value); }
        }

        public TimeSpan ElapsedTime
        {
            get { return _elapsedTime; }
            set { SetProperty(ref _elapsedTime, value); }
        }

        private void UpdatePosition(object sender, object e)
        {
            OnPropertyChanged("PositionInSeconds");

            if (!_isVLCInitialized || _vlcPlayer == null)
                return;

            if (_timeTotal == TimeSpan.Zero)
            {
                TimeTotal = TimeSpan.FromMilliseconds(_vlcPlayer.GetLength());
            }

            ElapsedTime = TimeSpan.FromSeconds(PositionInSeconds);
        }

        public void Play()
        {
            _vlcPlayer.Play();
            IsPlaying = true;
        }

        public void Pause()
        {
            _vlcPlayer.Pause();
            IsPlaying = false;
        }

        public void Stop()
        {
            _vlcPlayer.Stop();
            IsPlaying = false;
        }

        public void Seek(float position)
        {
            _vlcPlayer.Seek(position);
        }
        public void SetVideoPage(IVideoPage playVideo)
        {
            _panel = playVideo.Panel;   
            playVideo.PageLoaded += VideoPageLoaded;
        }

        private async void VideoPageLoaded(object sender, RoutedEventArgs e)
        {
            await InitializeVLC();
            Play();
        }
    }
}