/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Globalization;
using System.Threading.Tasks;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Commands;
using VLC_WINRT.Utility.Commands.MusicPlayer;
using VLC_WINRT.Utility.Services.Interface;
using VLC_WINRT.Utility.Services.RunTime;
using Windows.System.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace VLC_WINRT.ViewModels
{
    public class MediaPlaybackViewModel : NavigateableViewModel, IDisposable
    {
        protected readonly IMediaService _mediaService;
        protected readonly HistoryService _historyService;
        protected VlcService _vlcPlayerService;

        protected bool _isPlaying;
        protected TimeSpan _timeTotal;
        protected string _title;
        protected TimeSpan _elapsedTime;
        protected string _fileToken;
        protected string _mrl;

        protected ActionCommand _skipAhead;
        protected ActionCommand _skipBack;
        protected PlayNextCommand _playNext;
        protected PlayPreviousCommand _playPrevious;
        protected PlayPauseCommand _playOrPause;
        protected StopVideoCommand _goBackCommand;

        protected readonly DisplayRequest _displayAlwaysOnRequest;
        protected readonly DispatcherTimer _sliderPositionTimer;

        protected MediaPlaybackViewModel(HistoryService historyService, IMediaService mediaService, VlcService mediaPlayerService)
        {
            _historyService = historyService;

            _mediaService = mediaService;
            _mediaService.StatusChanged += PlayerStateChanged;

            _vlcPlayerService = mediaPlayerService;

            _displayAlwaysOnRequest = new DisplayRequest();
            _sliderPositionTimer = new DispatcherTimer();
            _sliderPositionTimer.Tick += FirePositionUpdate;
            _sliderPositionTimer.Interval = TimeSpan.FromMilliseconds(16);

            _skipAhead = new ActionCommand(() => _mediaService.SkipAhead());
            _skipBack = new ActionCommand(() => _mediaService.SkipBack());
            _playNext = new PlayNextCommand();
            _playPrevious = new PlayPreviousCommand();
            _playOrPause = new PlayPauseCommand();
            _goBackCommand = new StopVideoCommand();
        }

        public void Dispose()
        {
            if (_vlcPlayerService != null)
            {
                _vlcPlayerService.StatusChanged -= PlayerStateChanged;
                _vlcPlayerService.Stop();
                _vlcPlayerService.Close();
                _vlcPlayerService = null;
            }
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

        private async Task UpdatePosition(object sender, object e)
        {
            OnPropertyChanged("PositionInSeconds");

            if (_timeTotal == TimeSpan.Zero)
            {
                double timeInMilliseconds = await _vlcPlayerService.GetLength();
                TimeTotal = TimeSpan.FromMilliseconds(timeInMilliseconds);
            }

            OnPropertyChanged("Position");
            ElapsedTime = TimeSpan.FromSeconds(PositionInSeconds);
        }

        virtual public void CleanViewModel()
        {
            _mediaService.Stop();
           
            IsPlaying = false;
          
            Title = null;
            _elapsedTime = TimeSpan.Zero;
            _timeTotal = TimeSpan.Zero;
        }

        public override Task OnNavigatedFrom(NavigationEventArgs e)
        {
            UpdateDate();
            _sliderPositionTimer.Stop();
            _mediaService.Stop();
            return base.OnNavigatedFrom(e);
        }

        private void UpdateDate()
        {
            if (!string.IsNullOrEmpty(_fileToken))
            {
                _historyService.UpdateMediaHistory(_fileToken, ElapsedTime);
            }
            OnPropertyChanged("Now");
        }

        protected virtual void OnPlaybackStarting()
        {
            _sliderPositionTimer.Start();
            ProtectedDisplayCall(true);
        }

        protected virtual void OnPlaybackStopped()
        {
            _sliderPositionTimer.Stop();
            ProtectedDisplayCall(false);
        }

        #region Events

        protected async void PlayerStateChanged(object sender, VlcService.MediaPlayerState e)
        {
            await DispatchHelper.InvokeAsync(() => IsPlaying = e == VlcService.MediaPlayerState.Playing);
        }
        private async void FirePositionUpdate(object sender, object e)
        {
            await UpdatePosition(this, e);
        }

        #endregion

        #region Properties
        public bool IsPlaying
        {
            get { return _isPlaying; }
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
            }
        }

        public string Title
        {
            get { return _title; }
            protected set { SetProperty(ref _title, value); }
        }

        public string Now
        {
            get { return DateTime.Now.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern); }
        }

        public PlayPauseCommand PlayOrPause
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

        public StopVideoCommand GoBack
        {
            get { return _goBackCommand; }
            set { SetProperty(ref _goBackCommand, value); }
        }

        public TimeSpan TimeTotal
        {
            get { return _timeTotal; }
            set { SetProperty(ref _timeTotal, value); }
        }

        public TimeSpan ElapsedTime
        {
            get { return _elapsedTime; }
            set { SetProperty(ref _elapsedTime, value); }
        }
        public double PositionInSeconds
        {
            get
            {
                if (_vlcPlayerService != null && _vlcPlayerService.CurrentState == VlcService.MediaPlayerState.Playing)
                {
                    return _mediaService.GetPosition() * TimeTotal.TotalSeconds;
                }
                return 0.0d;
            }
            set { _mediaService.SetPosition((float)(value / TimeTotal.TotalSeconds)); }
        }

        public double Position
        {
            get
            {
                if (_vlcPlayerService != null && _vlcPlayerService.CurrentState == VlcService.MediaPlayerState.Playing)
                {
                    return _mediaService.GetPosition() * 1000;
                }
                return 0.0d;

            }
            set
            {
                _mediaService.SetPosition((float)value / 1000);
            }
        }

        #endregion
    }
}
