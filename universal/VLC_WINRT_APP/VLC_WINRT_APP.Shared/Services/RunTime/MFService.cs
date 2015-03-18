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

        public async Task SetMediaTransportControlsInfo(string title)
        {
            //throw new NotImplementedException();
        }

        public async Task SetMediaFile(IVLCMedia media)
        {
            if (Instance == null) return;
            RandomAccessStreamReference randomAccessStreamReference = null;
            if (media is StreamMedia)
            {
                randomAccessStreamReference = RandomAccessStreamReference.CreateFromUri(new Uri(media.Path));
            }
            else
            {
                if (media.File != null)
                {
                    randomAccessStreamReference = RandomAccessStreamReference.CreateFromFile(media.File);
                }
                else
                {
                    var file = await StorageFile.GetFileFromPathAsync(media.Path);
                    if (file != null)
                        randomAccessStreamReference = RandomAccessStreamReference.CreateFromFile(file);
                }
            }
            if (randomAccessStreamReference != null)
            {
                var randomAccessStream = await randomAccessStreamReference.OpenReadAsync();
                if (randomAccessStream != null)
                    Instance.SetSource(randomAccessStream, randomAccessStream.ContentType);
            }
        }

        public void Play()
        {
            if (Instance == null) return;
            Instance.Play();
        }

        public void Pause()
        {
            // vlc pause() method is a play/pause toggle. we reproduce the same behaviour here
            if (Instance == null) return;
            switch (Instance.CurrentState)
            {
                case MediaElementState.Playing:
                    Instance.Pause();
                    break;
                case MediaElementState.Paused:
                    Instance.Play();
                    break;
            }
        }

        public void Stop()
        {
            if (Instance == null) return;
            Instance.Stop();
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
            if (Instance.NaturalDuration.HasTimeSpan)
                return Instance.NaturalDuration.TimeSpan.Ticks;
            return 0f;
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
