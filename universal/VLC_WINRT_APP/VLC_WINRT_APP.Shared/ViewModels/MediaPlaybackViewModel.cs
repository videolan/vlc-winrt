/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.System;
using System.Collections.Generic;
using VLC_WINRT_APP.Commands.Video;
using VLC_WINRT_APP.Views.MainPages;
using System.Diagnostics;
using Windows.Storage;
using System;
using Windows.UI.Core;
using VLC_WINRT_APP.Commands;
using VLC_WINRT_APP.Common;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Services.Interface;
using Windows.System.Display;
using VLC_WINRT_APP.Commands.MediaPlayback;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Autofac;
using libVLCX;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Services.RunTime;
using VLC_WINRT_APP.ViewModels.MusicVM;

#if WINDOWS_PHONE_APP
using Windows.Media.Playback;
using VLC_WINRT_APP.BackgroundAudioPlayer.Model;
#endif

namespace VLC_WINRT_APP.ViewModels
{
    public sealed class MediaPlaybackViewModel : BindableBase, IDisposable
    {
        #region private props
#if WINDOWS_APP
        private MouseService _mouseService;
#endif
        private bool _isPlaying;
        private MediaState _mediaState;
        private PlayingType _playingType;
        private TrackCollection _trackCollection;
        private TimeSpan _timeTotal;
        private ActionCommand _skipAhead;
        private ActionCommand _skipBack;
        private PlayNextCommand _playNext;
        private PlayPreviousCommand _playPrevious;
        private PlayPauseCommand _playOrPause;

        private int _currentSubtitle;
        private int _currentAudioTrack;

        private SetSubtitleTrackCommand _setSubTitlesCommand;
        private OpenSubtitleCommand _openSubtitleCommand;
        private SetAudioTrackCommand _setAudioTrackCommand;
        private StopVideoCommand _goBackCommand;

        private readonly DisplayRequest _displayAlwaysOnRequest;

        private int _volume = 100;
        private bool _isRunning;
        private int _speedRate;
        private bool _isStream;
        #endregion

        #region private fields
        private List<DictionaryKeyValue> _subtitlesTracks;
        private List<DictionaryKeyValue> _audioTracks;
        #endregion

        #region public props
        public bool UseVlcLib { get; set; }

        public readonly IMediaService _mediaService;
        public PlayingType PlayingType
        {
            get { return _playingType; }
            set { SetProperty(ref _playingType, value); }
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

        public MediaState MediaState
        {
            get { return _mediaState; }
            set { SetProperty(ref _mediaState, value); }
        }

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

        public bool IsStream
        {
            get { return _isStream; }
            set { SetProperty(ref _isStream, value); }
        }

        public TrackCollection TrackCollection
        {
            get
            {
                _trackCollection = _trackCollection ?? new TrackCollection(true);
                return _trackCollection;
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
                if (_mediaService.MediaPlayer != null)
                    return _mediaService.MediaPlayer.time();
                if (BackgroundMediaPlayer.Current != null && BackgroundMediaPlayer.Current.CurrentState != MediaPlayerState.Closed && BackgroundMediaPlayer.Current.CurrentState != MediaPlayerState.Stopped && !UseVlcLib)
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
                    catch
                    {
                    }
                }
                return 0;
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
                if (_mediaService.MediaPlayer != null)
                    return _mediaService.MediaPlayer.position();
                if (BackgroundMediaPlayer.Current != null && BackgroundMediaPlayer.Current.CurrentState != MediaPlayerState.Closed && BackgroundMediaPlayer.Current.CurrentState != MediaPlayerState.Stopped && !UseVlcLib)
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
                }
                return 0;
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

        public MediaPlaybackViewModel()
        {
            var mediaService = App.Container.Resolve<IMediaService>();
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

            _subtitlesTracks = new List<DictionaryKeyValue>();
            _audioTracks = new List<DictionaryKeyValue>();

            _setSubTitlesCommand = new SetSubtitleTrackCommand();
            _setAudioTrackCommand = new SetAudioTrackCommand();
            _openSubtitleCommand = new OpenSubtitleCommand();
            _goBackCommand = new StopVideoCommand();
#if WINDOWS_APP
            _mouseService = App.Container.Resolve<MouseService>();
#endif
        }
        #endregion

        #region methods
        private void privateDisplayCall(bool shouldActivate)
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


        public async Task CleanViewModel()
        {
            _mediaService.Stop();
            PlayingType = PlayingType.NotPlaying;
            IsPlaying = false;
            TimeTotal = TimeSpan.Zero;
            // music clean
#if WINDOWS_PHONE_APP
            if (BackgroundMediaPlayer.Current != null &&
                BackgroundMediaPlayer.Current.CurrentState != MediaPlayerState.Stopped)
            {
                BackgroundMediaPlayer.Current.Pause();
                await App.BackgroundAudioHelper.ResetCollection(ResetType.NormalReset);
            }
#endif
            await TrackCollection.ResetCollection();
            TrackCollection.IsRunning = false;
        }

        private void OnPlaybackStarting()
        {
            privateDisplayCall(true);
#if WINDOWS_APP
            // video playback only
            _mouseService.HideMouse();
#endif
        }

        private async Task OnPlaybackStopped()
        {
            privateDisplayCall(false);
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
        }

        public async void OnLengthChanged(Int64 length)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                TimeTotal = TimeSpan.FromMilliseconds(length);
            });
        }

        private void OnStopped()
        {
            var em = _mediaService.MediaPlayer.eventManager();
            em.OnTrackAdded -= OnTrackAdded;
            em.OnTrackDeleted -= OnTrackDeleted;
            em.OnLengthChanged -= OnLengthChanged;
            em.OnStopped -= OnStopped;
            em.OnEndReached -= OnEndReached;
            _mediaService.SetNullMediaPlayer();
        }

        public async Task InitializePlayback(String mrl, Boolean isAudio, Boolean isStream, StorageFile file = null)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                PlayingType = (isAudio) ? PlayingType.Music : PlayingType.Video;
                IsStream = isStream;
            });
            await _mediaService.SetMediaFile(mrl, isAudio, isStream, file);
            if (_mediaService.MediaPlayer == null) return;
            var em = _mediaService.MediaPlayer.eventManager();
            em.OnLengthChanged += OnLengthChanged;
            em.OnStopped += OnStopped;
            em.OnEndReached += OnEndReached;
        }

        async void OnEndReached()
        {
            switch (PlayingType)
            {
                case PlayingType.Music:
                    if (TrackCollection.Playlist.Count == 0 || !TrackCollection.CanGoNext)
                    {
                        // Playlist is finished
                        await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                        {
                            TrackCollection.IsRunning = false;
                            PlayingType = PlayingType.NotPlaying;
                            App.ApplicationFrame.Navigate(typeof(MainPageHome));
                        });
                    }
                    else
                    {
                        await PlayNext();
                    }
                    break;
                case PlayingType.Video:
                    await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                    {
                        if (Locator.VideoVm.CurrentVideo != null)
                            Locator.VideoVm.CurrentVideo.TimeWatched = TimeSpan.Zero;
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
                        IsPlaying = false;
                        PlayingType = PlayingType.NotPlaying;
                    });
                    if (Locator.VideoVm.CurrentVideo != null)
                        await Locator.VideoLibraryVM.VideoRepository.Update(Locator.VideoVm.CurrentVideo).ConfigureAwait(false);
                    break;
                case PlayingType.NotPlaying:
                    break;
                default:
                    break;
            }
        }

        public async Task PlayNext()
        {
            // for music only, will change in the future
            if (TrackCollection.CanGoNext)
            {
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    TrackCollection.CurrentTrack++;
                    await Locator.MusicPlayerVM.Play(false);
                });
            }
            else
            {
                TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            }
        }

        public async Task PlayPrevious()
        {
            // for music only, will change in the future
            if (TrackCollection.CanGoPrevious)
            {
                await DispatchHelper.InvokeAsync(() => TrackCollection.CurrentTrack--);
                await Locator.MusicPlayerVM.Play(false);
            }
            else
            {
                TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            }
        }

        public void SetSizeVideoPlayer(uint x, uint y)
        {
            _mediaService.SetSizeVideoPlayer(x, y);
        }

        public async Task UpdatePosition()
        {
            if (Locator.VideoVm.CurrentVideo != null)
            {
                Locator.VideoVm.CurrentVideo.TimeWatched = TimeSpan.FromMilliseconds(Time);
                await Locator.VideoLibraryVM.VideoRepository.Update(Locator.VideoVm.CurrentVideo).ConfigureAwait(false);
            }
        }

        public void SetSubtitleTrack(int i)
        {
            if (_mediaService.MediaPlayer != null) _mediaService.MediaPlayer.setSpu(i);
        }

        public void SetAudioTrack(int i)
        {
            if (_mediaService.MediaPlayer != null) _mediaService.MediaPlayer.setAudioTrack(i);
        }

        public void OpenSubtitle(string mrl)
        {
            if (_mediaService.MediaPlayer != null) _mediaService.MediaPlayer.setSubtitleFile(mrl);
        }

        public void SetRate(float rate)
        {
            if (_mediaService.MediaPlayer != null) _mediaService.MediaPlayer.setRate(rate);
        }

        public void Stop()
        {
            _mediaService.Stop();
        }
        #endregion

        #region Events

        private async void PlayerStateChanged(object sender, MediaState e)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                IsPlaying = e == MediaState.Playing;
                OnPropertyChanged("IsPlaying");
                MediaState = e;
            });
        }

        public async void OnTrackAdded(TrackType type, int trackId)
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

        public async void OnTrackDeleted(TrackType type, int trackId)
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
        #endregion
        public void Dispose()
        {
            _mediaService.Stop();
            _skipAhead = null;
            _skipBack = null;
        }
    }
}
