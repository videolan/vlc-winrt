using System;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Services;
using VLC_Wrapper;
using Windows.Storage;
using Windows.UI.Xaml.Media;

namespace VLC_WINRT.ViewModels.PlayVideo
{
    public class PlayVideoViewModel : BindableBase, IDisposable
    {
        public Player VLCPlayer;

        private ImageBrush _brush;
        private StorageFile _currentFile;
        private HttpListener _listener;

        public PlayVideoViewModel()
        {
            Brush = new ImageBrush();
            VLCPlayer = new Player(Brush);
            _listener = new HttpListener();
        }

        public StorageFile CurrentFile
        {
            get { return _currentFile; }
            set
            {
                SetProperty(ref _currentFile, value);
                VLCPlayer.Open("http://localhost:8000/" + _currentFile.Name);
            }
        }

        public ImageBrush Brush
        {
            get { return _brush; }
            set { SetProperty(ref _brush, value); }
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
    }
}