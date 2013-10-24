using System;
using Windows.Storage.AccessCache;
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
        private Player _vlcPlayer;
        private ImageBrush _brush;
        private StorageFile _currentFile;
        private bool _isPlaying;
        private HttpListener _listener;
        private PlayPauseCommand _playOrPause;
        private SkipAheadCommand _skipAhead;
        private SkipBackCommand _skipBack;
        private StopVideoCommand _stopVideoCommand;
        private string _title;


        public PlayVideoViewModel()
        {
            InitializeVLC();
            _listener = new HttpListener();
            _playOrPause = new PlayPauseCommand();
            _skipAhead = new SkipAheadCommand();
            _skipBack = new SkipBackCommand();
            _stopVideoCommand = new StopVideoCommand();
        }

        private void InitializeVLC()
        {
            Brush = new ImageBrush();
            _vlcPlayer = new Player(Brush);
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
    }
}