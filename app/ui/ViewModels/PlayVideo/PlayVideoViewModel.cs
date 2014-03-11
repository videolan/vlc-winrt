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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Windows.Media;
using Windows.System.Display;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using libVLCX;
using VLC_WINRT.Common;
using VLC_WINRT.Model;
using VLC_WINRT.Utility.Commands;
using VLC_WINRT.Utility.Commands.VideoPlayer;
using VLC_WINRT.Utility.IoC;
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT.ViewModels.MainPage;
using VLC_WINRT.Views.Controls.MainPage;

namespace VLC_WINRT.ViewModels.PlayVideo
{
    public class PlayVideoViewModel : NavigateableViewModel, IDisposable
    {
        private readonly DisplayRequest _displayAlwaysOnRequest;
        private readonly HistoryService _historyService;
        private readonly DispatcherTimer _sliderPositionTimer = new DispatcherTimer();
        private Subtitle _currentSubtitle;
        private TimeSpan _elapsedTime = TimeSpan.Zero;
        private string _fileToken;
        private StopVideoCommand _goBackCommand;
        private bool _isPlaying;
        private string _mrl;
        private PlayPauseCommand _playOrPause;
        private ActionCommand _skipAhead;
        private ActionCommand _skipBack;
        private ObservableCollection<Subtitle> _subtitles;
        private TimeSpan _timeTotal = TimeSpan.Zero;
        private string _title;
        private MediaPlayerService _vlcPlayerService;
        private MouseService _mouseService;
        private MediaViewModel _currentVideo;

        private int _subtitlesCount = 0;
        private IDictionary<int, string> _subtitlesTracks;
        private SetSubtitleTrackCommand _setSubTitlesCommand;
        private OpenSubtitleCommand _openSubtitleCommand;
        private int _audioTracksCount = 0;
        private IDictionary<int, string> _audioTracks;
        private SetAudioTrackCommand _setAudioTrackCommand;

        public PlayVideoViewModel()
        {
            _playOrPause = new PlayPauseCommand();
            _historyService = IoC.GetInstance<HistoryService>();
            _goBackCommand = new StopVideoCommand();
            _displayAlwaysOnRequest = new DisplayRequest();
            _subtitles = new ObservableCollection<Subtitle>();
            _subtitlesTracks = new Dictionary<int, string>();
            _audioTracks = new Dictionary<int, string>();
            _sliderPositionTimer.Tick += FirePositionUpdate;
            _sliderPositionTimer.Interval = TimeSpan.FromMilliseconds(16);

            _vlcPlayerService = IoC.GetInstance<MediaPlayerService>();
            _vlcPlayerService.StatusChanged += PlayerStateChanged;

            _mouseService = IoC.GetInstance<MouseService>();

            _skipAhead = new ActionCommand(() => _vlcPlayerService.SkipAhead());
            _skipBack = new ActionCommand(() => _vlcPlayerService.SkipBack());
            _setSubTitlesCommand = new SetSubtitleTrackCommand();
            _setAudioTrackCommand = new SetAudioTrackCommand();
            _openSubtitleCommand = new OpenSubtitleCommand();
        }

        public MediaViewModel CurrentVideo
        {
            get { return _currentVideo; }
            set { SetProperty(ref _currentVideo, value); }
        }

        public double PositionInSeconds
        {
            get
            {
                if (_vlcPlayerService != null &&
                    _vlcPlayerService.CurrentState == MediaPlayerService.MediaPlayerState.Playing)
                {
                    return _vlcPlayerService.GetPosition().Result * TimeTotal.TotalSeconds;
                }
                return 0.0d;
            }
            set { _vlcPlayerService.Seek((float)(value / TimeTotal.TotalSeconds)); }
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

        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                SetProperty(ref _isPlaying, value);
                if (value)
                {
                    _sliderPositionTimer.Start();
                    _mouseService.HideMouse();
                    ProtectedDisplayCall(true);
                }
                else
                {
                    _sliderPositionTimer.Stop();
                    _mouseService.RestoreMouse();
                    ProtectedDisplayCall(false);
                }
            }
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

        public string Title
        {
            get { return _title; }
            private set { SetProperty(ref _title, value); }
        }

        public StopVideoCommand GoBack
        {
            get { return _goBackCommand; }
            set { SetProperty(ref _goBackCommand, value); }
        }

        public Subtitle CurrentSubtitle
        {
            get { return _currentSubtitle; }
            set { SetProperty(ref _currentSubtitle, value); }
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

        public ObservableCollection<Subtitle> Subtitles
        {
            get { return _subtitles; }
            private set { SetProperty(ref _subtitles, value); }
        }

        public int SubtitlesCount
        {
            get { return _subtitlesCount; }
            set { SetProperty(ref _subtitlesCount, value); }
        }

        public IDictionary<int, string> SubtitlesTracks
        {
            get { return _subtitlesTracks; }
            set { SetProperty(ref _subtitlesTracks, value); }
        }

        public SetSubtitleTrackCommand SetSubtitleTrackCommand
        {
            get { return _setSubTitlesCommand; }
            set { SetProperty(ref _setSubTitlesCommand, value); }
        }

        public OpenSubtitleCommand OpenSubtitleCommand
        {
            get { return _openSubtitleCommand; }
            set { SetProperty(ref _openSubtitleCommand, value); }
        }
        public int AudioTracksCount
        {
            get { return _audioTracksCount; }
            set { SetProperty(ref _audioTracksCount, value); }
        }

        public IDictionary<int, string> AudioTracks
        {
            get { return _audioTracks; }
            set { SetProperty(ref _audioTracks, value); }
        }

        public SetAudioTrackCommand SetAudioTrackCommand
        {
            get { return _setAudioTrackCommand; }
            set { SetProperty(ref _setAudioTrackCommand, value); }
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

        private void FirePositionUpdate(object sender, object e)
        {
            UpdatePosition(this, e);
        }

        private void PlayerStateChanged(object sender, MediaPlayerService.MediaPlayerState e)
        {
            if (e == MediaPlayerService.MediaPlayerState.Playing)
            {
                IsPlaying = true;
            }
            else
            {
                IsPlaying = false;
            }
        }

        public void RegisterPanel()
        {

        }

        public async void SetActiveVideoInfo(string token, string title)
        {
            // Pause the music viewmodel
            Locator.MusicPlayerVM.CleanViewModel();

            _fileToken = token;
            _mrl = "file://" + token;
            Title = title;
            _timeTotal = TimeSpan.Zero;
            _elapsedTime = TimeSpan.Zero;

            _vlcPlayerService.Open(_mrl);
            OnPropertyChanged("TimeTotal");

            _vlcPlayerService.Play();
            await Task.Delay(300);
            SubtitlesCount = await _vlcPlayerService.GetSubtitleCount();
            AudioTracksCount = await _vlcPlayerService.GetAudioTrackCount();
            await _vlcPlayerService.GetSubtitleDescription(SubtitlesTracks);
            await _vlcPlayerService.GetAudioTrackDescription(AudioTracks);
            _vlcPlayerService.MediaEnded += VlcPlayerServiceOnMediaEnded;
            RegisterMediaControlEvents();
        }

        public void SetActiveVideoInfo(string mrl)
        {
            // Pause the music viewmodel
            Locator.MusicPlayerVM.CleanViewModel();

            _fileToken = null;
            _mrl = mrl;
            _timeTotal = TimeSpan.Zero;
            _elapsedTime = TimeSpan.Zero;

            _vlcPlayerService.Open(_mrl);
            _vlcPlayerService.Play();
            _vlcPlayerService.MediaEnded += VlcPlayerServiceOnMediaEnded;
            RegisterMediaControlEvents();
        }

        void RegisterMediaControlEvents()
        {
            MediaControl.IsPlaying = true;
            MediaControl.ArtistName = "";
            MediaControl.TrackName = Title;
            MediaControl.NextTrackPressed += (sender, o) => DispatchHelper.Invoke(()=> SkipAhead.Execute(""));
            MediaControl.PreviousTrackPressed += (sender, o) => DispatchHelper.Invoke(()=> SkipBack.Execute(""));
            MediaControl.PlayPauseTogglePressed += (sender, o) => DispatchHelper.Invoke(() =>
            {
                if (IsPlaying)
                {
                    IsPlaying = false;
                    _vlcPlayerService.Pause();
                    MediaControl.IsPlaying = false;
                }
                else
                {
                    IsPlaying = true;
                    _vlcPlayerService.Play();
                    MediaControl.IsPlaying = true;
                }
            });

            MediaControl.PlayPressed += (sender, o) => DispatchHelper.Invoke(() =>
            {
                if (IsPlaying)
                {
                    IsPlaying = false;
                    _vlcPlayerService.Play();
                    MediaControl.IsPlaying = false;
                }
            });
            MediaControl.PausePressed += (sender, o) => DispatchHelper.Invoke(() =>
            {
                if (!IsPlaying)
                {
                    IsPlaying = true;
                    _vlcPlayerService.Pause();
                    MediaControl.IsPlaying = true;
                }
            });
        }

        public void UnRegisterMediaControlEvents()
        {
            MediaControl.IsPlaying = false;
            MediaControl.ArtistName = "";
            MediaControl.TrackName = "";
            MediaControl.NextTrackPressed += null;
            MediaControl.PreviousTrackPressed += null;
            MediaControl.PlayPressed += null;
            MediaControl.PausePressed += null;
        }
        

        private void VlcPlayerServiceOnMediaEnded(object sender, Player player)
        {
            UnRegisterMediaControlEvents();
            _vlcPlayerService.MediaEnded -= VlcPlayerServiceOnMediaEnded;
            DispatchHelper.Invoke(() => App.RootPage.MainFrame.GoBack());
        }

        public override void OnNavigatedFrom()
        {
            UpdateDate();
            _sliderPositionTimer.Stop();
            _vlcPlayerService.Stop();
        }

        private void UpdateDate()
        {
            if (!string.IsNullOrEmpty(_fileToken))
            {
                _historyService.UpdateMediaHistory(_fileToken, ElapsedTime);
            }

            OnPropertyChanged("Now");
        }

        private void ProtectedDisplayCall(bool shouldActivate)
        {
            if (_displayAlwaysOnRequest == null) return;
            try
            {
                if (shouldActivate)
                {
                    _displayAlwaysOnRequest.RequestActive();
                }
                else
                {
                    _displayAlwaysOnRequest.RequestRelease();
                }
            }

            catch (ArithmeticException badMathEx)
            {
                //  Work around for platform bug 
                Debug.WriteLine("display request failed again");
                Debug.WriteLine(badMathEx.ToString());
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

            ElapsedTime = TimeSpan.FromSeconds(PositionInSeconds);
        }

        public async Task SetSizeVideoPlayer(uint x, uint y)
        {
            _vlcPlayerService.SetSizeVideoPlayer(x, y);
        }

        public async Task SetSubtitleTrack(int i)
        {
            _vlcPlayerService.SetSubtitleTrack(i);
        }
        public async Task SetAudioTrack(int i)
        {
            _vlcPlayerService.SetAudioTrack(i);
        }

        public async Task OpenSubtitle(string mrl)
        {
            _vlcPlayerService.OpenSubtitle(mrl);
        }
    }
}
