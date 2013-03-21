using System;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Commands;
using VLC_WINRT.Utility.Services;
using VLC_Wrapper;
using Windows.Storage;
using Windows.UI.Xaml.Media;

namespace VLC_WINRT.ViewModels.PlayVideo
{
    public class PlayVideoViewModel : BindableBase, IDisposable
    {
        private readonly Player _vlcPlayer;
        private ImageBrush _brush;
        private StorageFile _currentFile;
        private bool _isPlaying;
        private HttpListener _listener;
        private PlayPauseCommand _playOrPause;
        private SkipAheadCommand _skipAhead;
        private SkipBackCommand _skipBack;

        public PlayVideoViewModel()
        {
            Brush = new ImageBrush();
            _vlcPlayer = new Player(Brush);
            _listener = new HttpListener();
            _playOrPause = new PlayPauseCommand();
            _skipAhead = new SkipAheadCommand();
            _skipBack = new SkipBackCommand();
        }

        public StorageFile CurrentFile
        {
            get { return _currentFile; }
            set
            {
                SetProperty(ref _currentFile, value);
                _vlcPlayer.Open("http://localhost:8000/" + _currentFile.Name);
            }
        }

        public ImageBrush Brush
        {
            get { return _brush; }
            set { SetProperty(ref _brush, value); }
        }

        public PlayPauseCommand PlayOrPause
        {
            get { return _playOrPause; }
            set { SetProperty(ref _playOrPause, value); }
        }

        public bool IsPlaying
        {
            get { return _isPlaying; }
            set { SetProperty(ref _isPlaying, value); }
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

        public void Dispose()
        {
            if (_listener != null)
            {
                _listener.Stop();
                _listener.Dispose();
                _listener = null;
            }
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
    }
}