using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Commands;
using VLC_WINRT.Utility.Services;
using VLC_Wrapper;

namespace VLC_WINRT.ViewModels.PlayVideo
{
    public class PlayVideoViewModel : BindableBase, IDisposable
    {
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private StorageFile _currentFile;
        private TimeSpan _elapsedTime = TimeSpan.Zero;
        private string _fileToken;
        private bool _isPlaying;
        private HttpListener _listener;
        private PlayPauseCommand _playOrPause;
        private SkipAheadCommand _skipAhead;
        private SkipBackCommand _skipBack;
        private StopVideoCommand _stopVideoCommand;
        private TimeSpan _timeTotal = TimeSpan.Zero;
        private string _title;
        private Player _vlcPlayer;
        private bool _isVLCInitialized = false;


        public PlayVideoViewModel()
        {
            _listener = new HttpListener();
            _playOrPause = new PlayPauseCommand();
            _skipAhead = new SkipAheadCommand();
            _skipBack = new SkipBackCommand();
            _stopVideoCommand = new StopVideoCommand();

            _timer.Tick += UpdatePosition;
            _timer.Interval = TimeSpan.FromMilliseconds((1.0d/60.0d));
        }


        public StorageFile CurrentFile
        {
            get { return _currentFile; }
            set
            {
                SetProperty(ref _currentFile, value);
                _fileToken = StorageApplicationPermissions.FutureAccessList.Add(_currentFile);
                Title = _currentFile.Name;
            }
        }

        public double Position
        {
            get 
            {
                return _isVLCInitialized ? _vlcPlayer.GetPosition() : 0.0d;
            }
            set { _vlcPlayer.Seek((float) value); }
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
                    _timer.Start();
                }
                else
                {
                    _timer.Stop();
                }
            }
        }

        public SkipAheadCommand SkipAhead
        {
            get { return _skipAhead; }
            set { SetProperty(ref _skipAhead, value); }
        }

        public SkipBackCommand SkipBack
        {
            get { return _skipBack; }
            set { SetProperty(ref _skipBack, value); }
        }

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
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

        public void Dispose()
        {
            if (_listener != null)
            {
                _listener.Stop();
                _listener.Dispose();
                _listener = null;
            }

            if (_vlcPlayer != null)
            {
                _vlcPlayer.Dispose();
                _vlcPlayer = null;
            }
        }

        public async Task InitializeVLC(SwapChainBackgroundPanel renderPanel)
        {
            _vlcPlayer = new Player(renderPanel);
            await _vlcPlayer.Initialize();
            _isVLCInitialized = true;
            _vlcPlayer.Open("winrt://" + _fileToken);
        }

        private void UpdatePosition(object sender, object e)
        {
            OnPropertyChanged("Position");

            if (_timeTotal == TimeSpan.Zero)
            {
                TimeTotal = TimeSpan.FromMilliseconds(_vlcPlayer.GetLength());
            }

            ElapsedTime = TimeSpan.FromMilliseconds(TimeTotal.TotalMilliseconds*Position);
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
    }
}