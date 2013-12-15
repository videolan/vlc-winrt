using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Windows.System.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT.Model;
using VLC_WINRT.Utility.Commands;
using VLC_WINRT.Utility.IoC;
using VLC_WINRT.Utility.Services.RunTime;

namespace VLC_WINRT.ViewModels.PlayVideo
{
    public class PlayVideoViewModel : NavigateableViewModel, IDisposable
    {
        private readonly DisplayRequest _displayAlwaysOnRequest;
        private readonly DispatcherTimer _fiveSecondTimer = new DispatcherTimer();
        private readonly HistoryService _historyService;
        private readonly DispatcherTimer _sliderPositionTimer = new DispatcherTimer();
        private Subtitle _currentSubtitle;
        private TimeSpan _elapsedTime = TimeSpan.Zero;
        private string _fileToken;
        private StopVideoCommand _goBackCommand;
        private bool _isPlaying;
        private string _mrl;
        private PlayPauseCommand _playOrPause;
        private ActionCommand _skipAhead;
        private ActionCommand _skipBack;
        private ObservableCollection<Subtitle> _subtitles;
        private TimeSpan _timeTotal = TimeSpan.Zero;
        private string _title;
        private MediaPlayerService _vlcPlayerService;
        private MouseService _mouseService;

        public PlayVideoViewModel()
        {
            _playOrPause = new PlayPauseCommand();
            _historyService = IoC.GetInstance<HistoryService>();
            _goBackCommand = new StopVideoCommand();
            _displayAlwaysOnRequest = new DisplayRequest();
            _subtitles = new ObservableCollection<Subtitle>();

            _sliderPositionTimer.Tick += FirePositionUpdate;
            _sliderPositionTimer.Interval = TimeSpan.FromMilliseconds(16);

            _fiveSecondTimer.Tick += UpdateDate;
            _fiveSecondTimer.Interval = TimeSpan.FromSeconds(5);

            _vlcPlayerService = IoC.GetInstance<MediaPlayerService>();
            _vlcPlayerService.StatusChanged += PlayerStateChanged;

            _mouseService = IoC.GetInstance<MouseService>();

            _skipAhead = new ActionCommand(() => _vlcPlayerService.SkipAhead());
            _skipBack = new ActionCommand(() => _vlcPlayerService.SkipBack());
        }

        public double PositionInSeconds
        {
            get
            {
                if (_vlcPlayerService != null && _vlcPlayerService.CurrentState == MediaPlayerService.MediaPlayerState.Playing)
                {
                    return _vlcPlayerService.GetPosition().Result*TimeTotal.TotalSeconds;
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
                if (value)
                {
                    _sliderPositionTimer.Start();
                    _mouseService.HideMouse();
                    ProtectedDisplayCall(true);
                }
                else
                {
                    _sliderPositionTimer.Stop();
                    _mouseService.RestoreMouse();
                    ProtectedDisplayCall(false);
                }
            }
        }

        public ActionCommand SkipAhead
        {
            get { return _skipAhead; }
            set { SetProperty(ref _skipAhead, value); }
        }

        public ActionCommand SkipBack
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

        public void RegisterPanel()
        {
        
        }

        public void SetActiveVideoInfo(string token, string title)
        {
            _fileToken = token;
            _mrl = "winrt://" + token;
            Title = title;

            _vlcPlayerService.Open(_mrl);
            _fiveSecondTimer.Start();
            OnPropertyChanged("TimeTotal");

            _vlcPlayerService.Play();
        }

        public void SetActiveVideoInfo(string mrl)
        {
            _fileToken = null;
            _mrl = mrl;
        }

        public override void OnNavigatedFrom()
        {
            _fiveSecondTimer.Stop();
            _sliderPositionTimer.Stop();
            _vlcPlayerService.Stop();
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
    }
}