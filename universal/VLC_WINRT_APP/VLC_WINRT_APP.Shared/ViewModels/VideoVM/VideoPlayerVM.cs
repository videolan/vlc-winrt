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
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Autofac;
using libVLCX;
using VLC_WINRT_APP.Commands.Video;
using VLC_WINRT_APP.DataRepository;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Services.Interface;
using VLC_WINRT_APP.Services.RunTime;
using VLC_WINRT_APP.Views.MainPages;

namespace VLC_WINRT_APP.ViewModels.VideoVM
{
    public class VideoPlayerVM : MediaPlaybackViewModel
    {
        #region private props
#if WINDOWS_APP
        private MouseService _mouseService;
#endif
        private VideoVM _currentVideo;
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
        private IDictionary<int, string> _subtitlesTracks;
        private List<DictionaryKeyValue> _audioTracks;
        #endregion

        #region public props

        public int SpeedRate
        {
            get { return _speedRate; }
            set
            {
                _speedRate = value;
                float r = (float)value / 100;
                SetRate(r);
            }
        }

        public VideoVM CurrentVideo
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


        public IDictionary<int, string> SubtitlesTracks
        {
            get { return _subtitlesTracks; }
            set { SetProperty(ref _subtitlesTracks, value); }
        }

        public List<DictionaryKeyValue> AudioTracks
        {
            get { return _audioTracks; }
            set { _audioTracks = value; }
        }

        #endregion

        #region constructors
        public VideoPlayerVM(IMediaService mediaService, VlcService mediaPlayerService)
            : base(mediaService, mediaPlayerService)
        {
            _subtitlesTracks = new Dictionary<int, string>();
            _audioTracks = new List<DictionaryKeyValue>();
#if WINDOWS_APP
            _mouseService = App.Container.Resolve<MouseService>();
#endif
            _setSubTitlesCommand = new SetSubtitleTrackCommand();
            _setAudioTrackCommand = new SetAudioTrackCommand();
            _openSubtitleCommand = new OpenSubtitleCommand();
            _goBackCommand = new StopVideoCommand();
            _lastVideosRepository.Load();
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

        public async void SetActiveVideoInfo(string mrl)
        {
            // Pause the music viewmodel
            Locator.MusicPlayerVM.CleanViewModel();

            IsRunning = true;
            _fileToken = mrl;
            _mrl = "file://" + mrl;
            _timeTotal = TimeSpan.Zero;
            _elapsedTime = TimeSpan.Zero;

            _vlcPlayerService.Open(_mrl);
            _vlcPlayerService.Play();
            await Task.Delay(500);
            if (_timeTotal == TimeSpan.Zero)
            {
                double timeInMilliseconds = await _vlcPlayerService.GetLength();
                TimeTotal = TimeSpan.FromMilliseconds(timeInMilliseconds);
            }
            OnPropertyChanged("TimeTotal");

            VideoVM media = await _lastVideosRepository.LoadViaToken(_fileToken);
            if (media == null)
            {
                PositionInSeconds = 0;
                _lastVideosRepository.Add(CurrentVideo);
            }
            else
            {
                PositionInSeconds = media.TimeWatched.TotalSeconds;
            }

            await Task.Delay(500);
            SubtitlesCount = await _vlcPlayerService.GetSubtitleCount();
            AudioTracksCount = await _vlcPlayerService.GetAudioTrackCount();
            await _vlcPlayerService.GetSubtitleDescription(_subtitlesTracks);

            IDictionary<int, string> tracks = new Dictionary<int, string>();
            await _vlcPlayerService.GetAudioTrackDescription(tracks);
            foreach (KeyValuePair<int, string> track in tracks)
            {
                _audioTracks.Add(new DictionaryKeyValue()
                {
                    Id = track.Key,
                    Name = track.Value,
                });
            }
            
            SpeedRate = 100;
            if(_audioTracks.Count > 1)
                CurrentAudioTrack = _audioTracks[1];
            _vlcPlayerService.MediaEnded += VlcPlayerServiceOnMediaEnded;
        }
        
        private void VlcPlayerServiceOnMediaEnded(object sender, Player player)
        {
            _vlcPlayerService.MediaEnded -= VlcPlayerServiceOnMediaEnded;
            App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => App.ApplicationFrame.Navigate(typeof (MainPageVideos)));
        }
        
        public void UpdatePosition()
        {
            if (double.IsNaN(PositionInSeconds))
                return;
            CurrentVideo.TimeWatched = TimeSpan.FromSeconds(PositionInSeconds);
            CurrentVideo.Duration = TimeTotal;
            _lastVideosRepository.Update(CurrentVideo);
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
        #endregion
    }
}
