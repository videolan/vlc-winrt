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
using System.Threading.Tasks;
using Windows.UI.Core;
using Autofac;
using VLC_WINRT_APP.Commands.Video;
using VLC_WINRT_APP.DataRepository;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Model.Video;
using VLC_WINRT_APP.Services.Interface;
using VLC_WINRT_APP.Services.RunTime;
using VLC_WINRT_APP.Views.MainPages;
using VLC_WINRT_APP.Helpers;
#if WINDOWS_APP
using libVLCX;
#endif

namespace VLC_WINRT_APP.ViewModels.VideoVM
{
    public class VideoPlayerVM : MediaPlaybackViewModel
    {
        #region private props
#if WINDOWS_APP
        private MouseService _mouseService;
#endif
        private Model.Video.VideoItem _currentVideo;
        private DictionaryKeyValue _currentSubtitle;
        private DictionaryKeyValue _currentAudioTrack;

        private int _subtitlesCount = 0;
        private int _audioTracksCount = 0;
        private SetSubtitleTrackCommand _setSubTitlesCommand;
        private OpenSubtitleCommand _openSubtitleCommand;
        private SetAudioTrackCommand _setAudioTrackCommand;
        protected StopVideoCommand _goBackCommand;

        private bool _isRunning;
        private int _speedRate;
        #endregion

        #region private fields
        private List<DictionaryKeyValue> _subtitlesTracks;
        private List<DictionaryKeyValue> _audioTracks;
        #endregion

        #region public props

        public int SpeedRate
        {
            get
            {
                return _speedRate;
            }
            set
            {
                if (value > 95 && value < 105)
                    value = 100;
                _speedRate = value;
                float r = (float)value / 100;
                SetRate(r);
            }
        }

        public Model.Video.VideoItem CurrentVideo
        {
            get { return _currentVideo; }
            set { SetProperty(ref _currentVideo, value); }
        }

        public DictionaryKeyValue CurrentSubtitle
        {
            get { return _currentSubtitle; }
            set
            {
                SetProperty(ref _currentSubtitle, value);
                SetSubtitleTrackCommand.Execute(value.Id);
            }
        }

        public DictionaryKeyValue CurrentAudioTrack
        {
            get { return _currentAudioTrack; }
            set
            {
                _currentAudioTrack = value;
                SetAudioTrackCommand.Execute(value.Id);
            }
        }

        public int SubtitlesCount
        {
            get { return _subtitlesCount; }
            set { SetProperty(ref _subtitlesCount, value); }
        }

        public int AudioTracksCount
        {
            get { return _audioTracksCount; }
            set { SetProperty(ref _audioTracksCount, value); }
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

        public SetAudioTrackCommand SetAudioTrackCommand
        {
            get { return _setAudioTrackCommand; }
            set { SetProperty(ref _setAudioTrackCommand, value); }
        }
        public StopVideoCommand GoBack
        {
            get { return _goBackCommand; }
            set { SetProperty(ref _goBackCommand, value); }
        }
        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
            set
            {
                SetProperty(ref _isRunning, value);
                OnPropertyChanged("IsRunning");
                OnPropertyChanged("PlayingType");
            }
        }
        #endregion

        #region public fields
        public LastVideosRepository _lastVideosRepository = new LastVideosRepository();


        public List<DictionaryKeyValue> AudioTracks
        {
            get { return _audioTracks; }
            set { _audioTracks = value; }
        }

        public List<DictionaryKeyValue> Subtitles
        {
            get { return _subtitlesTracks; }
            set { _subtitlesTracks = value; }
        }
        #endregion

        #region constructors
        public VideoPlayerVM(IMediaService mediaService)
            : base(mediaService)
        {
            _subtitlesTracks = new List<DictionaryKeyValue>();
            _audioTracks = new List<DictionaryKeyValue>();
#if WINDOWS_APP
            _mouseService = App.Container.Resolve<MouseService>();
#endif
            _setSubTitlesCommand = new SetSubtitleTrackCommand();
            _setAudioTrackCommand = new SetAudioTrackCommand();
            _openSubtitleCommand = new OpenSubtitleCommand();
            _goBackCommand = new StopVideoCommand();
            _lastVideosRepository.Load().ContinueWith((t) => {
                LogHelper.Log(t);
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
        #endregion

        #region methods

        protected override void OnPlaybackStarting()
        {
#if WINDOWS_APP
            _mouseService.HideMouse();
#endif
            base.OnPlaybackStarting();
        }

        protected override void OnPlaybackStopped()
        {
#if WINDOWS_APP
            _mouseService.RestoreMouse();
#endif
            base.OnPlaybackStopped();
        }

        public async Task SetActiveVideoInfo(string mrl, bool isStream = false)
        {
            // Pause the music viewmodel
            Locator.MusicPlayerVM.CleanViewModel();

            IsRunning = true;
            OnPropertyChanged("IsRunning");
            IsPlaying = true;
            OnPropertyChanged("IsPlaying");
            _fileToken = mrl;
            _mrl = (isStream) ? mrl : "file://" + mrl;

            _timeTotal = TimeSpan.Zero;
            _elapsedTime = TimeSpan.Zero;

            _mediaService.SetMediaFile(_mrl, isAudioMedia: false);
            _mediaService.Play();

            await Task.Delay(1000);
            double timeInMilliseconds = _mediaService.GetLength();
            TimeTotal = TimeSpan.FromMilliseconds(timeInMilliseconds);
            VideoItem media = await _lastVideosRepository.LoadViaToken(_fileToken);
            if (media == null)
            {
                if (CurrentVideo != null && CurrentVideo.Duration > TimeSpan.FromMinutes(1))
                {
                    await _lastVideosRepository.Add(CurrentVideo);
                }
            }
            else
            {
                Time = (Int64)media.TimeWatched.TotalMilliseconds;
            }
            OnPropertyChanged("Time");
            OnPropertyChanged("TimeTotal");
            SubtitlesCount = _mediaService.MediaPlayer.spuCount();
            AudioTracksCount = _mediaService.MediaPlayer.audioTrackCount();


            var subtitles = _mediaService.MediaPlayer.spuDescription();
            _subtitlesTracks.Clear();
            foreach (var sub in subtitles)
            {
                _subtitlesTracks.Add(new DictionaryKeyValue()
                {
                    Id = sub.id(),
                    Name = sub.name(),
                });
            }

            var audioTracks = _mediaService.MediaPlayer.audioTrackDescription();
            _audioTracks.Clear();
            foreach (var track in audioTracks)
            {
                _audioTracks.Add(new DictionaryKeyValue()
                {
                    Id = track.id(),
                    Name = track.name(),
                });
            }

            SpeedRate = 100;
            if (_audioTracks.Count > 1)
                CurrentAudioTrack = _audioTracks[1];
            if (_subtitlesTracks.Count > 1)
                CurrentSubtitle = _subtitlesTracks[0];
            _mediaService.MediaEnded += VlcPlayerServiceOnMediaEnded;
            await _mediaService.SetMediaTransportControlsInfo(CurrentVideo != null ? CurrentVideo.Title : "Video");
        }

        private async void VlcPlayerServiceOnMediaEnded(object sender, object e)
        {
            _mediaService.MediaEnded -= VlcPlayerServiceOnMediaEnded;
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                if (App.ApplicationFrame.CanGoBack)
                    App.ApplicationFrame.GoBack();
                else
                {
#if WINDOWS_APP
                    App.ApplicationFrame.Navigate(typeof(MainPageVideos));
#else
                    Locator.MainVM.GoToPanelCommand.Execute(0);
#endif
                    }
                Locator.VideoVm.IsRunning = false;
                OnPropertyChanged("PlayingType");
            });
        }

        public void UpdatePosition()
        {
            if (CurrentVideo != null)
            {
                CurrentVideo.TimeWatched = TimeSpan.FromMilliseconds(Time);
                CurrentVideo.Duration = TimeTotal;
                _lastVideosRepository.Update(CurrentVideo);
            }
        }

        public void SetSizeVideoPlayer(uint x, uint y)
        {
            _mediaService.SetSizeVideoPlayer(x, y);
        }

        public void SetSubtitleTrack(int i)
        {
            _mediaService.MediaPlayer.setSpu(i);
        }

        public void SetAudioTrack(int i)
        {
            _mediaService.MediaPlayer.setAudioTrack(i);
        }

        public void OpenSubtitle(string mrl)
        {
            _mediaService.MediaPlayer.setSubtitleFile(mrl);
        }

        public void SetRate(float rate)
        {
            _mediaService.MediaPlayer.setRate(rate);
        }
        #endregion
    }
}
