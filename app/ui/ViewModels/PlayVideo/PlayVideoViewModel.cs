using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.System.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using GalaSoft.MvvmLight.Command;
using Microsoft.Practices.ServiceLocation;
using VLC_WINRT.Model;
using VLC_WINRT.Utility.Commands;
using VLC_WINRT.Utility.Services.RunTime;

namespace VLC_WINRT.ViewModels.PlayVideo
{
    public class PlayVideoViewModel : NavigateableViewModel, IDisposable
    {
        private readonly DisplayRequest _displayAlwaysOnRequest;
        private readonly DispatcherTimer _fiveSecondTimer = new DispatcherTimer();
        private readonly HistoryService _historyService;
        private readonly DispatcherTimer _sliderPositionTimer = new DispatcherTimer();
        private MediaPlayerService _vlcPlayerService;
        private Subtitle _currentSubtitle;
        private TimeSpan _elapsedTime = TimeSpan.Zero;
        private string _fileToken;
        private bool _isPlaying;
        private bool _isVLCInitialized;
        private PlayPauseCommand _playOrPause;
        private RelayCommand _skipAhead;
        private RelayCommand _skipBack;
        private StopVideoCommand _goBackCommand;
        private ObservableCollection<Subtitle> _subtitles;
        private TimeSpan _timeTotal = TimeSpan.Zero;
        private string _title;

        public PlayVideoViewModel()
        {
            _playOrPause = new PlayPauseCommand();
            _historyService = ServiceLocator.Current.GetInstance<HistoryService>();
            _goBackCommand = new StopVideoCommand();
            _displayAlwaysOnRequest = new DisplayRequest();
            _subtitles = new ObservableCollection<Subtitle>();

            _sliderPositionTimer.Tick += FirePositionUpdate; 
            _sliderPositionTimer.Interval = TimeSpan.FromMilliseconds(16);

            _fiveSecondTimer.Tick += UpdateDate;
            _fiveSecondTimer.Interval = TimeSpan.FromSeconds(5);

            _vlcPlayerService = ServiceLocator.Current.GetInstance<MediaPlayerService>();
            _vlcPlayerService.StatusChanged += PlayerStateChanged;

            _skipAhead = new RelayCommand(() => _vlcPlayerService.SkipAhead());
            _skipBack = new RelayCommand(() => _vlcPlayerService.SkipBack());
        }

        private void FirePositionUpdate(object sender, object e)
        {
            UpdatePosition(this, e);
        }

        private void PlayerStateChanged(object sender, MediaPlayerService.MediaPlayerState e)
        {
            if (e == MediaPlayerService.MediaPlayerState.Playing)
            {
                IsPlaying = true;
            }
            else
            {
                IsPlaying = false;
            }
        }

        public double PositionInSeconds
        {
            get
            {
                if (_isVLCInitialized && _vlcPlayerService != null)
                {
                    return _vlcPlayerService.GetPosition().Result * TimeTotal.TotalSeconds;
                }
                return 0.0d;
            }
            set { _vlcPlayerService.Seek((float) (value/TimeTotal.TotalSeconds)); }
        }

        public string Now
        {
            get { return DateTime.Now.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern); }
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

        public StopVideoCommand GoBack
        {
            get { return _goBackCommand; }
            set { SetProperty(ref _goBackCommand, value); }
        }

        public Subtitle CurrentSubtitle
        {
            get { return _currentSubtitle; }
            set { SetProperty(ref _currentSubtitle, value); }
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

        public ObservableCollection<Subtitle> Subtitles
        {
            get { return _subtitles; }
            private set { SetProperty(ref _subtitles, value); }
        }

        public async Task RegisterPanel(SwapChainBackgroundPanel panel)
        {
            await _vlcPlayerService.Initialize(panel);
            _fiveSecondTimer.Start();
            RaisePropertyChanged("TimeTotal");

            //TODO: Remove
            Subtitles.Add(new Subtitle {Id = 1, Name = "English"});
            Subtitles.Add(new Subtitle {Id = 2, Name = "French"});
            Subtitles.Add(new Subtitle {Id = 3, Name = "German"});
            _vlcPlayerService.Play();
        }

        public void SetActiveVideoInfo(string token, string title)
        {
            _fileToken = token;
            Title = title;
        }

        public override void OnNavigatedFrom()
        {
            _fiveSecondTimer.Stop();
            _sliderPositionTimer.Stop();

            _isVLCInitialized = false;

            _vlcPlayerService.Stop();
            _vlcPlayerService.Close();
        }

        private void UpdateDate(object sender, object e)
        {
            if (!string.IsNullOrEmpty(_fileToken))
            {
                _historyService.UpdateMediaHistory(_fileToken, ElapsedTime);
            }

            OnPropertyChanged("Now");
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


        private async Task UpdatePosition(object sender, object e)
        {
            OnPropertyChanged("PositionInSeconds");

            if (_timeTotal == TimeSpan.Zero)
            {
                double timeInMilliseconds = await _vlcPlayerService.GetLength();
                TimeTotal = TimeSpan.FromMilliseconds(timeInMilliseconds);
            }

            ElapsedTime = TimeSpan.FromSeconds(PositionInSeconds);
        }

        public void Dispose()
        {
            if (_vlcPlayerService != null)
            {
                _vlcPlayerService.StatusChanged -= PlayerStateChanged;
                _vlcPlayerService.Stop();
                _vlcPlayerService.Close();
                _vlcPlayerService = null;
            }

            _skipAhead = null;
            _skipBack = null;
        }
    }
}