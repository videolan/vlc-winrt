/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using VLC_WINRT_APP.Model.Music;
#if WINDOWS_PHONE_APP
using Windows.Media.Playback;
#endif
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
#if WINDOWS_APP
using libVLCX;
#endif
using VLC_WINRT.Common;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Services.RunTime
{
    public class VlcService
    {
        public enum MediaPlayerState
        {
            Playing,
            NotPlaying,
            Stopped,
            Paused
        }

        private readonly object _controlLock = new object();

        public MediaPlayerState CurrentState;
        private Task _vlcInitializeTask;
        private Player _vlcPlayer;


        public VlcService()
        {
            CurrentState = MediaPlayerState.Stopped;
        }

        public event EventHandler<MediaPlayerState> StatusChanged;

        public event EventHandler<Player> MediaEnded;
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
            //TODO: fix this work around.
#if WINDOWS_APP
            if (CurrentState == MediaPlayerState.Paused)
            {
                Play();
            }
#endif
            DoVLCSafeAction(() =>
            {
                _vlcPlayer.Stop();
                UpdateStatus(MediaPlayerState.Stopped);
            });
        }

        private async void DoVLCSafeAction(Action a)
        {
#if WINDOWS_APP
            if (_vlcPlayer == null || _vlcInitializeTask == null)
                return;

            await _vlcInitializeTask;
            lock (_controlLock)
            {
                a();
            }
#else
            a();
#endif
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

        public async Task Initialize(SwapChainPanel panel)
        {
            _vlcPlayer = new Player(panel);
            IAsyncAction init = _vlcPlayer.Initialize();
            if (init != null)
            {
                _vlcInitializeTask = init.AsTask();
            }
            _vlcPlayer.MediaEnded += _vlcPlayer_MediaEnded;
#if WINDOWS_PHONE_APP
            _vlcInitializeTask = new Task(() =>
            {
                Debug.WriteLine("FakeInitialieTaskCalled");
            });
#endif
            await _vlcInitializeTask;

        }

        private void _vlcPlayer_MediaEnded()
        {
            UpdateStatus(MediaPlayerState.NotPlaying);
            var mediaEnded = MediaEnded;
            if (mediaEnded != null)
            {
                MediaEnded(this, _vlcPlayer);
            }
        }

        public void Open(string mrl)
        {
            DoVLCSafeAction(() => { _vlcPlayer.Open(mrl); });
        }

        public void OpenSubtitle(string mrl)
        {
            DoVLCSafeAction(() => { _vlcPlayer.OpenSubtitle(mrl); });
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
            TimeSpan seekTo = TimeSpan.FromMilliseconds(position * length) + relativeTimeSpan;
            double relativePosition = seekTo.TotalMilliseconds / length;
            if (relativePosition < 0.0f)
            {
                relativePosition = 0.0f;
            }
            if (relativePosition > 1.0f)
            {
                relativePosition = 1.0f;
            }
            Seek((float)relativePosition);
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

        public async Task<float> GetPosition()
        {
            float position = 0.0f;
#if WINDOWS_APP
            if (_vlcPlayer == null || _vlcInitializeTask == null)
                return position;

            await _vlcInitializeTask;
            lock (_controlLock)
            {
                {
                    if (CurrentState == MediaPlayerState.Playing
                        || CurrentState == MediaPlayerState.Paused)
                    {
                        position = _vlcPlayer.GetPosition();
                    }
                    else return 0;
                }
            }
            return position;
#else
            return _vlcPlayer.GetPosition();
#endif
        }

        public async Task<long> GetLength()
        {
#if WINDOWS_APP
            long length = 0;
            if (_vlcPlayer == null || _vlcInitializeTask == null)
                return length;

            await _vlcInitializeTask;
            lock (_controlLock)
            {
                length = _vlcPlayer.GetLength();
            }
            return length;
#else
            return _vlcPlayer.GetLength();
#endif
        }

        public async Task SetSizeVideoPlayer(uint x, uint y)
        {
            if (_vlcPlayer == null || _vlcInitializeTask == null)
                return;
            await _vlcInitializeTask;

            lock (_controlLock)
            {
                _vlcPlayer.UpdateSize(x, y);
            }
        }

        public async Task<int> GetSubtitleCount()
        {
            if (_vlcPlayer == null || _vlcInitializeTask == null)
                return 0;
            await _vlcInitializeTask;
            lock (_controlLock)
            {
                return _vlcPlayer.GetSubtitleCount();
            }
        }

        public async Task<int> GetAudioTrackCount()
        {
            if (_vlcPlayer == null || _vlcInitializeTask == null)
                return 0;
            await _vlcInitializeTask;
            lock (_controlLock)
            {
                return _vlcPlayer.GetAudioTracksCount();
            }
        }

        public async Task<int> GetSubtitleDescription(IDictionary<int, string> subtitles)
        {
            if (_vlcPlayer == null || _vlcInitializeTask == null)
                return 0;
            await _vlcInitializeTask;
            lock (_controlLock)
            {
                return _vlcPlayer.GetSubtitleDescription(subtitles);
            }
        }
        public async Task<int> GetAudioTrackDescription(IDictionary<int, string> audioTracks)
        {
            if (_vlcPlayer == null || _vlcInitializeTask == null)
                return 0;
            await _vlcInitializeTask;
            lock (_controlLock)
            {
                return _vlcPlayer.GetAudioTracksDescription(audioTracks);
            }
        }

        public async Task SetSubtitleTrack(int track)
        {
            if (_vlcPlayer == null || _vlcInitializeTask == null)
                return;
            await _vlcInitializeTask;
            lock (_controlLock)
            {
                _vlcPlayer.SetSubtitleTrack(track);
            }
        }

        public async Task SetAudioTrack(int track)
        {
            if (_vlcPlayer == null || _vlcInitializeTask == null)
                return;
            await _vlcInitializeTask;
            lock (_controlLock)
            {
                _vlcPlayer.SetAudioTrack(track);
            }
        }
        public async Task SetRate(float rate)
        {
            if (_vlcPlayer == null || _vlcInitializeTask == null)
                return;
            await _vlcInitializeTask;
            lock (_controlLock)
            {
                _vlcPlayer.SetRate(rate);
            }
        }

        public async Task SetVolume(int volume)
        {
            if (_vlcPlayer == null || _vlcInitializeTask == null)
                return;
            await _vlcInitializeTask;
            lock (_controlLock)
            {
                _vlcPlayer.SetVolume(volume);
            }
        }

        public async Task<int> GetVolume()
        {
            if (_vlcPlayer == null || _vlcInitializeTask == null)
                return 0;
            await _vlcInitializeTask;
            lock (_controlLock)
            {
                int vol = _vlcPlayer.GetVolume();
                return vol;
            }
        }

        public async Task Trim()
        {
            if (_vlcPlayer == null || _vlcInitializeTask == null)
                return;
            await _vlcInitializeTask;
            lock (_controlLock)
            {
                _vlcPlayer.Trim();
            }
        }
    }

#if WINDOWS_PHONE_APP
    public class Player
    {
        public event Action MediaEnded;
        public delegate void MediaEndedHandler();

        public IAsyncAction Initialize()
        {
            App.RootPage.MediaElement.MediaEnded += (sender, args) => MediaEnded();
            return null;
        }

        public Player(SwapChainPanel SwapChainPanel)
        {
        }

        public void SetRate(float rate)
        {
        }

        public void SetAudioTrack(int track)
        {

        }

        public void SetSubtitleTrack(int track)
        {

        }

        public int GetAudioTracksDescription(IDictionary<int, string> audioTracks)
        {
            return 0;
        }

        public int GetSubtitleDescription(IDictionary<int, string> subtitles)
        {
            return 0;
        }

        public int GetAudioTracksCount()
        {
            return 0;
        }

        public int GetSubtitleCount()
        {
            return 0;
        }

        public int GetVolume()
        {
            return 0;
        }

        public void SetVolume(int vol)
        {

        }
        public void UpdateSize(uint u, uint u1)
        {

        }

        public long GetLength()
        {
            if (Locator.VideoVm.PlayingType == PlayingType.Music)
            {
                long length;
                length = (long)Locator.MusicPlayerVM.CurrentTrack.Duration.TotalMilliseconds;
                return length;
            }
            else
            {
                return (long)Locator.VideoVm.CurrentVideo.Duration.TotalMilliseconds;
            }

            return 0;
        }

        public float GetPosition()
        {
            try
            {
                if (Locator.MusicPlayerVM.TrackCollection.IsRunning)
                {
#if WINDOWS_APP
                float pos;
                //#if WINDOWS_APP
                pos = (float)
                    (App.RootPage.MediaElement.Position.TotalSeconds /
                     Locator.MusicPlayerVM.CurrentPlayingArtist.CurrentAlbumItem.Tracks[Locator.MusicPlayerVM.CurrentPlayingArtist.CurrentAlbumItem.CurrentTrackPosition].Duration.TotalSeconds);
                //#else
                //                pos = (float)
                //                    (BackgroundMediaPlayer.Current.Position.TotalSeconds /
                //                         Locator.MusicPlayer.CurrentPlayingArtist.CurrentAlbumItem.CurrentTrack.Duration.TotalSeconds);
                //#endif
                return pos;
#else
                    return
                        (float)
                            (App.RootPage.MediaElement.Position.TotalSeconds /
                             Locator.MusicPlayerVM.CurrentTrack.Duration.TotalSeconds);
#endif
                }
                else
                {
                    return
                        (float)
                            (App.RootPage.MediaElement.Position.TotalSeconds /
                             Locator.VideoVm.CurrentVideo.Duration.TotalSeconds);
                }
            }
            catch { }
            return 0f;
        }

        public void Dispose()
        {

        }

        public void OpenSubtitle(string mrl)
        {

        }

        public async Task Open(string mrl)
        {
            Debug.WriteLine("Play with dummy player");
            StorageFile file = null;
            TrackItem trackItem = null;
            try
            {
                if (Locator.VideoVm.PlayingType == PlayingType.Music)
                {
                    trackItem = Locator.MusicPlayerVM.CurrentTrack;
                }
                else
                {
                    file = Locator.VideoVm.CurrentVideo.File;
                }
            }
            catch
            {
                trackItem = Locator.MusicPlayerVM.CurrentTrack;
            }
            if (trackItem != null)
            {
                file = await StorageFile.GetFileFromPathAsync(trackItem.Path);
            }
            var stream = await file.OpenAsync(FileAccessMode.Read);

            //DispatchHelper.Invoke(() =>
            //{
            //#if WINDOWS_APP
            App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                App.RootPage.MediaElement.SetSource(
                    stream, file.ContentType);
            });
            //#else
            //                if (Locator.MusicPlayer.IsRunning)
            //                {
            //                    BackgroundMediaPlayer.SendMessageToBackground(new ValueSet()
            //                    {
            //                        {"filePath", file.Path},
            //                    });
            //                }
            //                else
            //                {
            //                    App.RootPage.MediaElement.SetSource(stream, file.ContentType);
            //                }
            //#endif
            //});
        }

        public void Pause()
        {
            //if (Locator.MusicPlayer.IsRunning)
            //{
            //#if WINDOWS_APP

            App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => App.RootPage.MediaElement.Pause());
            //#else
            //            if (Locator.MusicPlayer.IsRunning)
            //            {
            //                BackgroundMediaPlayer.Current.Pause();
            //            }
            //            else
            //            {
            //                DispatchHelper.Invoke(() => App.RootPage.MediaElement.Pause());
            //            }
            //#endif
            //}
        }

        public async void Play()
        {
            //            Debug.WriteLine("Play with dummy player");
            //            if (Locator.MusicPlayerVM.IsRunning)
            //            {
            //                DispatchHelper.Invoke(() =>
            //                {
            //#if WINDOWS_APP

            //            App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, ()=>App.RootPage.MediaElement.Play());
            //#else
            //                    if (Locator.MusicPlayerVM.IsRunning)
            //                    {
            //                        BackgroundMediaPlayer.Current.Play();
            //                    }
            //                    else
            //                    {
            //                        DispatchHelper.Invoke(() => App.RootPage.MediaElement.Play());
            //                    }
            //#endif
            //                });
            //            }
            //            else
            //            {

            //            }
            try
            {
                App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => App.RootPage.MediaElement.Play());
            }
            catch { }
        }

        public void Seek(float position)
        {
            if (Locator.MusicPlayerVM.TrackCollection.IsRunning)
            {
                if (double.IsNaN(position)) return;
                TimeSpan tS;
                tS = TimeSpan.FromSeconds(position *
                                         Locator.MusicPlayerVM.CurrentTrack.Duration.TotalSeconds);

                //#if WINDOWS_APP
                App.RootPage.MediaElement.Position = tS;
                //#else
                //                BackgroundMediaPlayer.Current.Position = tS;
                //#endif
            }
            else
            {
                if (!double.IsNaN(position))
                {
                    App.RootPage.MediaElement.Position =
                        TimeSpan.FromSeconds(position * (int)Locator.VideoVm.CurrentVideo.Duration.TotalSeconds);
                }
            }
        }

        public void Stop()
        {
            App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                App.RootPage.MediaElement.Stop();
                App.RootPage.MediaElement.Source = null;
            });
        }

        public void Trim()
        {

        }
    }
#endif
}