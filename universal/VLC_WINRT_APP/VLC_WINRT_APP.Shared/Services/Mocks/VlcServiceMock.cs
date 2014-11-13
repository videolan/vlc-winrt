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
using System.Text;
using System.Threading.Tasks;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.Services.Interface;
using VLC_WINRT_APP.ViewModels;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;


namespace VLC_WINRT_APP.Services.Mocks
{
#if WINDOWS_PHONE_APP
    class VlcServiceMock : IVlcService
    {
        public VlcState CurrentState { get; private set; }
        public event EventHandler<VlcState> StatusChanged;
        public event EventHandler MediaEnded;
        public delegate void MediaEndedHandler();

        private void UpdateStatus(VlcState status)
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
            App.RootPage.MediaElement.Stop();
            App.RootPage.MediaElement.Source = null;
            UpdateStatus(VlcState.Stopped);
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
            UpdateStatus(VlcState.Playing);
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
            UpdateStatus(VlcState.Paused);
        }

        private void _vlcPlayer_MediaEnded(object s, object e)
        {
            UpdateStatus(VlcState.NotPlaying);
            var mediaEnded = MediaEnded;
            if (mediaEnded != null)
            {
                MediaEnded(this, null);
            }
        }

        public Task Initialize(SwapChainPanel panel)
        {
            App.RootPage.MediaElement.MediaEnded += (sender, args) => MediaEnded(null, null);
            MediaEnded += _vlcPlayer_MediaEnded;
            return Task.FromResult(false);
        }

        public async void Open(string mrl)
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

        public void OpenSubtitle(string mrl)
        {
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
            if (CurrentState != VlcState.Stopped)
            {
                Stop();
            }
        }

        public Task<float> GetPosition()
        {
            float res = 0.0f;
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
                    res = (float)(App.RootPage.MediaElement.Position.TotalSeconds /
                            Locator.MusicPlayerVM.CurrentTrack.Duration.TotalSeconds);
#endif
                }
                else
                {
                    res = (float)(App.RootPage.MediaElement.Position.TotalSeconds /
                            Locator.VideoVm.CurrentVideo.Duration.TotalSeconds);
                }
            }
            catch { }
            return Task.FromResult(res);
        }
        
        public Task<long> GetLength()
        {
            long length = 0;
            if (Locator.VideoVm.PlayingType == PlayingType.Music)
            {
                length = (long)Locator.MusicPlayerVM.CurrentTrack.Duration.TotalMilliseconds;
            }
            else
            {
                length = (long)Locator.VideoVm.CurrentVideo.Duration.TotalMilliseconds;
            }
            return Task.FromResult(length);
        }

        public Task SetSizeVideoPlayer(uint x, uint y)
        {
            return Task.FromResult(false);
        }

        public Task<int> GetSubtitleCount()
        {
            return Task.FromResult(0);
        }

        public Task<int> GetAudioTrackCount()
        {
            return Task.FromResult(0);
        }

        public Task<int> GetSubtitleDescription(IDictionary<int, string> subtitles)
        {
            return Task.FromResult(0);
        }

        public Task<int> GetAudioTrackDescription(IDictionary<int, string> audioTracks)
        {
            return Task.FromResult(0);
        }

        public Task SetSubtitleTrack(int track)
        {
            return Task.FromResult(false);
        }

        public Task SetAudioTrack(int track)
        {
            return Task.FromResult(false);
        }

        public Task SetRate(float rate)
        {
            return Task.FromResult(false);
        }

        public Task SetVolume(int volume)
        {
            return Task.FromResult(false);
        }

        public Task<int> GetVolume()
        {
            return Task.FromResult(0);
        }

        public Task Trim()
        {
            return Task.FromResult(0);
        }
    }
#endif
}
