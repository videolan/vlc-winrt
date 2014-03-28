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
using System.Threading.Tasks;
using VLC_WINRT.Common;
using VLC_WINRT.Model;
using VLC_WINRT.Utility.Commands.VideoPlayer;
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT.ViewModels.MainPage;
#if NETFX_CORE
using libVLCX;
using VLC_WINRT.Views.Controls.MainPage;
#endif
using VLC_WINRT.Utility.Services.Interface;
#if WINDOWS_PHONE_APP
using VLC_WINPRT;
#endif


namespace VLC_WINRT.ViewModels.PlayVideo
{
    public class PlayVideoViewModel : MediaPlaybackViewModel
    {
        private ObservableCollection<Subtitle> _subtitles;
#if NETFX_CORE
        private MouseService _mouseService;
#endif
        private MediaViewModel _currentVideo;
        private Subtitle _currentSubtitle;
        private int _subtitlesCount = 0;
        private IDictionary<int, string> _subtitlesTracks;
        private SetSubtitleTrackCommand _setSubTitlesCommand;
        private OpenSubtitleCommand _openSubtitleCommand;
        private int _audioTracksCount = 0;
        private IDictionary<int, string> _audioTracks;
        private SetAudioTrackCommand _setAudioTrackCommand;

        public PlayVideoViewModel(HistoryService historyService, IMediaService mediaService, VlcService mediaPlayerService)
            : base(historyService, mediaService, mediaPlayerService)
        {
            _subtitles = new ObservableCollection<Subtitle>();
            _subtitlesTracks = new Dictionary<int, string>();
            _audioTracks = new Dictionary<int, string>();
#if NETFX_CORE
            _mouseService = App.Container.Resolve<MouseService>();
#endif
            _setSubTitlesCommand = new SetSubtitleTrackCommand();
            _setAudioTrackCommand = new SetAudioTrackCommand();
            _openSubtitleCommand = new OpenSubtitleCommand();
        }

        public MediaViewModel CurrentVideo
        {
            get { return _currentVideo; }
            set { SetProperty(ref _currentVideo, value); }
        }

        protected override void OnPlaybackStarting()
        {
#if NETFX_CORE
            _mouseService.HideMouse();
#endif
            base.OnPlaybackStarting();
        }

        protected override void OnPlaybackStopped()
        {
#if NETFX_CORE
            _mouseService.RestoreMouse();
#endif
            base.OnPlaybackStopped();
        }

        public Subtitle CurrentSubtitle
        {
            get { return _currentSubtitle; }
            set { SetProperty(ref _currentSubtitle, value); }
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
            _subtitlesCount = await _vlcPlayerService.GetSubtitleCount();
            _audioTracksCount = await _vlcPlayerService.GetAudioTrackCount();
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
            MediaControl.NextTrackPressed += async (sender, o) => await DispatchHelper.InvokeAsync(()=> SkipAhead.Execute(""));
            MediaControl.PreviousTrackPressed += async (sender, o) => await DispatchHelper.InvokeAsync(()=> SkipBack.Execute(""));
            MediaControl.PlayPauseTogglePressed += async (sender, o) => await DispatchHelper.InvokeAsync(() =>
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

            MediaControl.PlayPressed += async (sender, o) => await DispatchHelper.InvokeAsync(() =>
            {
                if (IsPlaying)
                {
                    IsPlaying = false;
                    _vlcPlayerService.Play();
                    MediaControl.IsPlaying = false;
                }
            });
            MediaControl.PausePressed += async (sender, o) => await DispatchHelper.InvokeAsync(() =>
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
        

        private async void VlcPlayerServiceOnMediaEnded(object sender, Player player)
        {
            UnRegisterMediaControlEvents();
            _vlcPlayerService.MediaEnded -= VlcPlayerServiceOnMediaEnded;
#if WINDOWS_PHONE_APP
            await DispatchHelper.InvokeAsync(() => App.ApplicationFrame.GoBack());
#endif
#if NETFX_CORE
            await DispatchHelper.InvokeAsync(() => App.RootPage.MainFrame.GoBack());
#endif
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

        public Task SetSizeVideoPlayer(uint x, uint y)
        {
            return _vlcPlayerService.SetSizeVideoPlayer(x, y);
        }

        public Task SetSubtitleTrack(int i)
        {
            return _vlcPlayerService.SetSubtitleTrack(i);
        }
        public Task SetAudioTrack(int i)
        {
            return _vlcPlayerService.SetAudioTrack(i);
        }

        public void OpenSubtitle(string mrl)
        {
            _vlcPlayerService.OpenSubtitle(mrl);
        }

        public Task SetRate(float rate)
        {
            return _vlcPlayerService.SetRate(rate);
        }
    }
}
