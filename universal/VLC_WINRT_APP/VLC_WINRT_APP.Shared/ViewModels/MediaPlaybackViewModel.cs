/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.Diagnostics;
using Windows.Storage;
using System;
using Windows.UI.Core;
using VLC_WINRT_APP.Commands;
using VLC_WINRT_APP.Common;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Services.Interface;
using VLC_WINRT_APP.Services.RunTime;
using Windows.System.Display;
using Windows.UI.Xaml;
using VLC_WINRT_APP.Commands.MediaPlayback;
using System.Threading.Tasks;
using libVLCX;

#if WINDOWS_PHONE_APP
using Windows.Media.Playback;
using VLC_WINRT_APP.BackgroundAudioPlayer.Model;
#endif

namespace VLC_WINRT_APP.ViewModels
{
    public abstract class MediaPlaybackViewModel : BindableBase, IDisposable
    {
        #region private props
        protected readonly IMediaService _mediaService;

        protected bool _isPlaying;
        protected TimeSpan _timeTotal;
        protected ActionCommand _skipAhead;
        protected ActionCommand _skipBack;
        protected PlayNextCommand _playNext;
        protected PlayPreviousCommand _playPrevious;
        protected PlayPauseCommand _playOrPause;

        protected readonly DisplayRequest _displayAlwaysOnRequest;

        protected int _volume = 100;
        #endregion

        #region private fields

        #endregion

        #region public props

        public int Volume
        {
            get
            {
                return _volume;
            }
            set
            {
                if (value > 0)
                {
                    _mediaService.SetVolume(value);
                    SetProperty(ref _volume, value);
                }
            }
        }

        public bool IsPlaying
        {
            get
            {
                return _isPlaying;
            }
            set
            {
                if (value != _isPlaying)
                {
                    if (value)
                    {
                        OnPlaybackStarting();
                    }
                    else
                    {
                        OnPlaybackStopped();
                    }
                    SetProperty(ref _isPlaying, value);
                }
                OnPropertyChanged("PlayingType");
            }
        }

        public PlayingType PlayingType
        {
            get
            {
                if (Locator.MusicPlayerVM.TrackCollection.IsRunning)
                    return PlayingType.Music;
                return Locator.VideoVm.IsRunning ? PlayingType.Video : PlayingType.NotPlaying;
            }
        }

        public PlayPauseCommand PlayOrPauseCommand
        {
            get { return _playOrPause; }
            set { SetProperty(ref _playOrPause, value); }
        }

        public PlayNextCommand PlayNextCommand
        {
            get { return _playNext; }
            set { SetProperty(ref _playNext, value); }
        }
        public PlayPreviousCommand PlayPreviousCommand
        {
            get { return _playPrevious; }
            set { SetProperty(ref _playPrevious, value); }
        }

        public ActionCommand SkipAhead
        {
            get { return _skipAhead; }
            set { SetProperty(ref _skipAhead, value); }
        }

        public ActionCommand SkipBack
        {
            get { return _skipBack; }
            set { SetProperty(ref _skipBack, value); }
        }


        public TimeSpan TimeTotal
        {
            get { return _timeTotal; }
            set { SetProperty(ref _timeTotal, value); }
        }

        /**
         * Elasped time in milliseconds
         */
        public long Time
        {
            get
            {
#if WINDOWS_APP
                if (_mediaService.MediaPlayer == null)
                    return 0;
                return _mediaService.MediaPlayer.time();
#else
                if (BackgroundMediaPlayer.Current != null && BackgroundMediaPlayer.Current.CurrentState != MediaPlayerState.Closed && BackgroundMediaPlayer.Current.CurrentState != MediaPlayerState.Stopped)
                {
                    // CurrentState keeps failing OnCanceled, even though it actually has a state.
                    // The error is useless for now, so try catch and ignore.
                    try
                    {
                        switch (BackgroundMediaPlayer.Current.CurrentState)
                        {
                            case MediaPlayerState.Playing:
                                return (long)BackgroundMediaPlayer.Current.Position.TotalMilliseconds;
                            case MediaPlayerState.Closed:
                                // TODO: Use saved time value to populate time field.
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        return 0;
                    }
                    return 0;
                }
                else
                {
                    if (_mediaService.MediaPlayer == null)
                        return 0;
                    return _mediaService.MediaPlayer.time();
                }
#endif
            }
            set
            {
#if WINDOWS_APP
                _mediaService.MediaPlayer.setTime(value);
#else
                // Same as with "Time", BackgroundMediaPlayer.Current.CurrentState sometimes blows up on OnCancelled. So for now, throw it in a try catch.
                // This really seems like an WinRT API bug to me...
                try
                {
                    if (BackgroundMediaPlayer.Current != null &&
                    BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Playing)
                    {
                        BackgroundMediaPlayer.Current.Position = TimeSpan.FromMilliseconds(value);
                    }
                    else
                    {
                        if (_mediaService.MediaPlayer == null)
                            return;
                        _mediaService.MediaPlayer.setTime(value);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error with position : " + ex.Message);
                }
#endif
            }
        }

        public float Position
        {
            get
            {
#if WINDOWS_APP
                // XAML might ask for the position while no playback is running, hence the check.
                if (_mediaService.MediaPlayer == null)
                    return 0.0f;
                return _mediaService.MediaPlayer.position();
#else
                if (BackgroundMediaPlayer.Current != null && BackgroundMediaPlayer.Current.CurrentState != MediaPlayerState.Closed && BackgroundMediaPlayer.Current.CurrentState != MediaPlayerState.Stopped)
                {
                    switch (BackgroundMediaPlayer.Current.CurrentState)
                    {
                        case MediaPlayerState.Playing:
                            var pos = (BackgroundMediaPlayer.Current.Position.TotalMilliseconds /
                                BackgroundMediaPlayer.Current.NaturalDuration.TotalMilliseconds);
                            float posfloat = (float)pos;
                            return posfloat;
                        case MediaPlayerState.Closed:
                            break;
                    }
                    return 0;
                }
                else
                {
                    // XAML might ask for the position while no playback is running, hence the check.
                    if (_mediaService.MediaPlayer == null)
                        return 0.0f;
                    return _mediaService.MediaPlayer.position();
                }
#endif
            }
            set
            {
                // We shouldn't be able to set the position without a running playback.
#if WINDOWS_APP
                _mediaService.MediaPlayer.setPosition(value);
#else
                if (BackgroundMediaPlayer.Current != null && BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Playing)
                {
                    switch (BackgroundMediaPlayer.Current.CurrentState)
                    {
                        case MediaPlayerState.Playing:
                        case MediaPlayerState.Closed:
                            if (BackgroundMediaPlayer.Current.NaturalDuration.TotalMilliseconds != 0)
                            {
                                BackgroundMediaPlayer.Current.Position = TimeSpan.FromMilliseconds(value * BackgroundMediaPlayer.Current.NaturalDuration.TotalMilliseconds);
                            }
                            break;
                    }
                }
                else
                {
                    _mediaService.MediaPlayer.setPosition(value);
                }
#endif
            }
        }
        #endregion

        #region public fields

        #endregion
        #region constructors

        protected MediaPlaybackViewModel(IMediaService mediaService)
        {
            _mediaService = mediaService;
            _mediaService.StatusChanged += PlayerStateChanged;
            _mediaService.TimeChanged += UpdateTime;

            _displayAlwaysOnRequest = new DisplayRequest();

            _skipAhead = new ActionCommand(() =>
            {
                _mediaService.SkipAhead();
                VideoHUDHelper.ShowLittleTextWithFadeOut("+10s");
            });
            _skipBack = new ActionCommand(() =>
            {
                _mediaService.SkipBack();
                VideoHUDHelper.ShowLittleTextWithFadeOut("-10s");
            });
            _playNext = new PlayNextCommand();
            _playPrevious = new PlayPreviousCommand();
            _playOrPause = new PlayPauseCommand();
        }

        #endregion

        #region methods
        public void Dispose()
        {
            _mediaService.Stop();
            _skipAhead = null;
            _skipBack = null;
        }

        private void ProtectedDisplayCall(bool shouldActivate)
        {
            if (_displayAlwaysOnRequest == null) return;
            if (shouldActivate)
            {
                _displayAlwaysOnRequest.RequestActive();
            }
            else
            {
                _displayAlwaysOnRequest.RequestRelease();
            }
        }

        private async void UpdateTime(Int64 time)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                OnPropertyChanged("Time");
                // Assume position also changes when time does.
                // We could/should also watch OnPositionChanged event, but let's save us
                // the cost of another dispatched call.
                OnPropertyChanged("Position");
            });
        }

        public void UpdateTimeFromMF()
        {
            App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                OnPropertyChanged("Time");
                // Assume position also changes when time does.
                // We could/should also watch OnPositionChanged event, but let's save us
                // the cost of another dispatched call.
                OnPropertyChanged("Position");
            });
        }


        virtual public void CleanViewModel()
        {
            _mediaService.Stop();
            IsPlaying = false;
            TimeTotal = TimeSpan.Zero;
        }

        protected virtual void OnPlaybackStarting()
        {
            ProtectedDisplayCall(true);
        }

        protected virtual Task OnPlaybackStopped()
        {
            ProtectedDisplayCall(false);
            return Task.FromResult(0);
        }

        public async void OnLengthChanged(Int64 length)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                TimeTotal = TimeSpan.FromMilliseconds(length);
            });
        }

        protected virtual void OnStopped()
        {
            var em = _mediaService.MediaPlayer.eventManager();
            em.OnLengthChanged -= OnLengthChanged;
            em.OnStopped -= OnStopped;
            em.OnEndReached -= OnEndReached;
        }

        protected abstract void OnEndReached();

        protected void InitializePlayback(String mrl, Boolean isAudio)
        {
            _mediaService.SetMediaFile(mrl, isAudio);
            var em = _mediaService.MediaPlayer.eventManager();
            em.OnLengthChanged += OnLengthChanged;
            em.OnStopped += OnStopped;
            em.OnEndReached += OnEndReached;
        }

        #endregion

        #region Events

        protected async void PlayerStateChanged(object sender, MediaState e)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                IsPlaying = e == MediaState.Playing;
                OnPropertyChanged("IsPlaying");
            });
        }

        #endregion

    }
}
