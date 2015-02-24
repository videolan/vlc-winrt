/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.Storage;
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
using libVLCX;
using System.Diagnostics;
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
        private int _currentSubtitle;
        private int _currentAudioTrack;

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
            get
            {
                if (_currentSubtitle < 0 || _currentSubtitle >= Subtitles.Count)
                    return null;
                return AudioTracks[_currentSubtitle];
            }
            set
            {
                _currentSubtitle = Subtitles.IndexOf(value);
                if (value != null)
                    SetSubtitleTrackCommand.Execute(value.Id);
            }
        }

        public DictionaryKeyValue CurrentAudioTrack
        {
            get 
            {
                if (_currentAudioTrack == -1 || _currentAudioTrack >= AudioTracks.Count) 
                    return null;
                return AudioTracks[_currentAudioTrack];
            }
            set
            {
                _currentAudioTrack = AudioTracks.IndexOf(value);
                if (value != null)
                    SetAudioTrackCommand.Execute(value.Id);
            }
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

        protected override async Task OnPlaybackStopped()
        {
#if WINDOWS_APP
            _mouseService.RestoreMouse();
#endif
            _audioTracks.Clear();
            _subtitlesTracks.Clear();
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                CurrentAudioTrack = null;
                CurrentSubtitle = null;
            });
            await base.OnPlaybackStopped();
        }

        private async void OnTrackAdded(TrackType type, int trackId)
        {
            if (type == TrackType.Unknown || type == TrackType.Video)
                return;
            List<DictionaryKeyValue> target;
            IList<TrackDescription> source;
            if (type == TrackType.Audio)
            {
                target = _audioTracks;
                source = _mediaService.MediaPlayer.audioTrackDescription();
            }
            else
            {
                target = _subtitlesTracks;
                source = _mediaService.MediaPlayer.spuDescription();
            }

            target.Clear();
            foreach (var t in source)
            {
                target.Add(new DictionaryKeyValue()
                {
                    Id = t.id(),
                    Name = t.name(),
                });
            }
            // This assumes we have a "Disable" track for both subtitles & audio
            if (type == TrackType.Subtitle && CurrentSubtitle == null && _subtitlesTracks.Count > 1)
            {
                _currentSubtitle = 1;
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => OnPropertyChanged("CurrentSubtitle"));
            }
            else if (type == TrackType.Audio && CurrentAudioTrack == null && _audioTracks.Count > 1)
            {
                _currentAudioTrack = 1;
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => OnPropertyChanged("CurrentAudioTrack"));
            }
        }

        private async void OnTrackDeleted(TrackType type, int trackId)
        {
            if (type == TrackType.Unknown || type == TrackType.Video)
                return;
            List<DictionaryKeyValue> target;
            if (type == TrackType.Audio)
                target = _audioTracks;
            else
                target = _subtitlesTracks;

            target.RemoveAll((t) => t.Id == trackId);
            if (target.Count > 0)
                return;
            if (type == TrackType.Subtitle)
            {
                _currentSubtitle = -1;
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => OnPropertyChanged("CurrentSubtitle"));
            }
            else
            {
                _currentAudioTrack = -1;
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => OnPropertyChanged("CurrentAudioTrack"));
            }
        }

        public async Task SetActiveVideoInfo(VideoItem media, string streamMrl = null, StorageFile file = null)
        {
            if (media == null && string.IsNullOrEmpty(streamMrl))
                return;
            // Pause the music viewmodel
            await Locator.MusicPlayerVM.CleanViewModel();

            IsRunning = true;
            OnPropertyChanged("IsRunning");
            IsPlaying = true;
            OnPropertyChanged("IsPlaying");

            _timeTotal = TimeSpan.Zero;
            LogHelper.Log("PLAYVIDEO: Initializing playback");
            if (media != null)
            {
                var path = media.Token ?? media.FilePath;
                await InitializePlayback(path, false, false, file);
            }
            else
                await InitializePlayback(streamMrl, false, true);
            var em = _mediaService.MediaPlayer.eventManager();
            em.OnTrackAdded += OnTrackAdded;
            em.OnTrackDeleted += OnTrackDeleted;
            _mediaService.Play();
            LogHelper.Log("PLAYVIDEO: Play() method called");

            if (media != null && media.TimeWatched != null && media.TimeWatched != TimeSpan.FromSeconds(0))
                Time = (Int64)media.TimeWatched.TotalMilliseconds;

            SpeedRate = 100;
            await _mediaService.SetMediaTransportControlsInfo(CurrentVideo != null ? CurrentVideo.Title : "Video");
            UpdateTileHelper.UpdateMediumTileWithVideoInfo();
        }

        protected override async void OnEndReached()
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                if (CurrentVideo != null)
                    CurrentVideo.TimeWatched = TimeSpan.Zero;
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
            if (CurrentVideo != null)
                await Locator.VideoLibraryVM.VideoRepository.Update(CurrentVideo).ConfigureAwait(false);
        }

        protected override void OnStopped()
        {
            var em = _mediaService.MediaPlayer.eventManager();
            em.OnTrackAdded -= OnTrackAdded;
            em.OnTrackDeleted -= OnTrackDeleted;
            base.OnStopped();
        }

        public async Task UpdatePosition()
        {
            if (CurrentVideo != null)
            {
                CurrentVideo.TimeWatched = TimeSpan.FromMilliseconds(Time);
                await Locator.VideoLibraryVM.VideoRepository.Update(CurrentVideo).ConfigureAwait(false);
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
