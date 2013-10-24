using System;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Commands;
using VLC_WINRT.Utility.Services;
using VLC_Wrapper;

namespace VLC_WINRT.ViewModels.PlayVideo
{
    public class PlayVideoViewModel : BindableBase, IDisposable
    {
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private ImageBrush _brush;
        private StorageFile _currentFile;
        private bool _isPlaying;
        private HttpListener _listener;
        private PlayPauseCommand _playOrPause;
        private SkipAheadCommand _skipAhead;
        private SkipBackCommand _skipBack;
        private StopVideoCommand _stopVideoCommand;
        private string _title;
        private Player _vlcPlayer;


        public PlayVideoViewModel()
        {
            InitializeVLC();
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
                string token = StorageApplicationPermissions.FutureAccessList.Add(value);
                //  Tell the player to play the video based on its token
                _vlcPlayer.Open("winrt://" + token);
                Title = _currentFile.Name;
            }
        }

        public ImageBrush Brush
        {
            get { return _brush; }
            set { SetProperty(ref _brush, value); }
        }

        public double Position
        {
            get { return (_vlcPlayer.GetPosition()); }
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

        public void Dispose()
        {
            if (_listener != null)
            {
                _listener.Stop();
                _listener.Dispose();
                _listener = null;
            }

            Brush = null;

            if (_vlcPlayer != null)
            {
                _vlcPlayer.Dispose();
                _vlcPlayer = null;
            }
        }

        private void InitializeVLC()
        {
            Brush = new ImageBrush();
            _vlcPlayer = new Player(Brush);
        }

        private void UpdatePosition(object sender, object e)
        {
            OnPropertyChanged("Position");
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