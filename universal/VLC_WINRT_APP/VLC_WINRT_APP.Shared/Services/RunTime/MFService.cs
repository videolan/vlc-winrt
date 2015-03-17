using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using libVLCX;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Services.Interface;

namespace VLC_WINRT_APP.Services.RunTime
{
    public class MFService : IMediaService
    {
        public MediaElement Instance { get; private set; }

        public event EventHandler<MediaState> StatusChanged;
        public event TimeChanged TimeChanged;
        
        public TaskCompletionSource<bool> PlayerInstanceReady
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public bool IsBackground
        {
            get { throw new NotImplementedException(); }
        }

        public void Initialize(object mediaElement)
        {
            var mE = mediaElement as MediaElement;
            if (mE == null) throw new ArgumentNullException("mediaElement", "MediaFoundationService needs a MediaElement");

        }

        public Task SetMediaTransportControlsInfo(string artistName, string albumName, string trackName, string albumUri)
        {
            throw new NotImplementedException();
        }

        public Task SetMediaTransportControlsInfo(string title)
        {
            throw new NotImplementedException();
        }

        public Task SetMediaFile(IVLCMedia media, bool isAudioMedia, bool isStream)
        {
            throw new NotImplementedException();
        }

        public void Play()
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void SetNullMediaPlayer()
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void SkipBack()
        {
            throw new NotImplementedException();
        }

        public float GetLength()
        {
            throw new NotImplementedException();
        }

        public int GetVolume()
        {
            throw new NotImplementedException();
        }

        public void SetVolume(int volume)
        {
            throw new NotImplementedException();
        }

        public void Trim()
        {
            throw new NotImplementedException();
        }

        public void SetSizeVideoPlayer(uint x, uint y)
        {
            throw new NotImplementedException();
        }

    }
}
