using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using libVLCX;

namespace VLC_WINRT.Utility.Services.RunTime
{
    public class MediaPlayerService
    {
        public enum MediaPlayerState
        {
            Playing,
            NotPlaying,
            Stopped,
            Paused
        }

        private readonly object _controlLock = new object();

        private readonly HistoryService _historyService;
        public MediaPlayerState CurrentState;
        private Task _vlcInitializeTask;
        private Player _vlcPlayer;

        public MediaPlayerService()
        {
            CurrentState = MediaPlayerState.Stopped;
            _historyService = IoC.IoC.GetInstance<HistoryService>();
        }

        public event EventHandler<MediaPlayerState> StatusChanged;

        private void UpdateStatus(MediaPlayerState status)
        {
            if (CurrentState != status)
            {
                CurrentState = status;
                if (StatusChanged != null)
                {
                    StatusChanged(this, CurrentState);
                }
            }
        }

        public void Stop()
        {
            DoVLCSafeAction(() =>
            {
                _vlcPlayer.Stop();
                UpdateStatus(MediaPlayerState.Stopped);
            });
        }

        private async void DoVLCSafeAction(Action a)
        {
            if (_vlcPlayer == null || _vlcInitializeTask == null)
                return;

            await _vlcInitializeTask;
            lock (_controlLock)
            {
                a();
            }
        }


        public void Seek(float position)
        {
            DoVLCSafeAction(() => _vlcPlayer.Seek(position));
        }

        public void Play()
        {
            DoVLCSafeAction(() =>
            {
                _vlcPlayer.Play();
                UpdateStatus(MediaPlayerState.Playing);
            });
        }

        public void Pause()
        {
            DoVLCSafeAction(() =>
            {
                _vlcPlayer.Pause();
                UpdateStatus(MediaPlayerState.Paused);
            });
        }

        public async Task Initialize(SwapChainBackgroundPanel panel, string mrl)
        {
            _vlcPlayer = new Player(panel);
            _vlcInitializeTask = _vlcPlayer.Initialize().AsTask();
            await _vlcInitializeTask;
            _vlcPlayer.Open(mrl);

            //string token = _historyService.GetTokenAtPosition(0);
            //_vlcPlayer.Open("winrt://" + token);
        }

        public void SkipAhead()
        {
            TimeSpan relativeTimeSpan = TimeSpan.FromSeconds(10);
            SeekToRelativeTime(relativeTimeSpan);
        }

        public void SkipBack()
        {
            TimeSpan relativeTimeSpan = TimeSpan.FromSeconds(-10);
            SeekToRelativeTime(relativeTimeSpan);
        }

        private async void SeekToRelativeTime(TimeSpan relativeTimeSpan)
        {
            double position = await GetPosition();
            double length = await GetLength();
            TimeSpan seekTo = TimeSpan.FromMilliseconds(position*length) + relativeTimeSpan;
            double relativePosition = seekTo.TotalMilliseconds/length;
            if (relativePosition < 0.0f)
            {
                relativePosition = 0.0f;
            }
            if (relativePosition > 1.0f)
            {
                relativePosition = 1.0f;
            }
            Seek((float) relativePosition);
        }

        public void Close()
        {
            if (_vlcPlayer != null)
            {
                if (CurrentState != MediaPlayerState.Stopped)
                {
                    Stop();
                }

                lock (_controlLock)
                {
                    if (_vlcInitializeTask != null)
                    {
                        _vlcInitializeTask.Wait(20000);
                        _vlcInitializeTask = null;
                        GC.Collect();
                    }
                }

                lock (_controlLock)
                {
                    try
                    {
                        _vlcPlayer.Dispose();
                        _vlcPlayer = null;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("We ran into an exception disposing vlc instance");
                        Debug.WriteLine(ex);
                    }
                }
            }
        }

        public async Task<double> GetPosition()
        {
            double position = 0.0f;
            if (_vlcPlayer == null || _vlcInitializeTask == null)
                return position;

            await _vlcInitializeTask;
            lock (_controlLock)
            {
                {
                    if (CurrentState == MediaPlayerState.Playing)
                    {
                        position = _vlcPlayer.GetPosition();
                    }
                    else return 0;
                }
            }
            return position;
        }

        public async Task<long> GetLength()
        {
            long length = 0;
            if (_vlcPlayer == null || _vlcInitializeTask == null)
                return length;

            await _vlcInitializeTask;
            lock (_controlLock)
            {
                length = _vlcPlayer.GetLength();
            }
            return length;
        }
    }
}