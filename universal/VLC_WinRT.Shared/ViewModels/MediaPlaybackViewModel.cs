/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.Storage.AccessCache;
using Windows.Media;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Storage;
using System;
using System.IO;
using Windows.UI.Core;
using VLC_WinRT.Helpers;
using VLC_WinRT.Model;
using VLC_WinRT.Services.Interface;
using Windows.System.Display;
using VLC_WinRT.Commands.MediaPlayback;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Autofac;
using VLC_WinRT.Helpers.MusicLibrary;
using VLC_WinRT.Model.Music;
using VLC_WinRT.Model.Stream;
using VLC_WinRT.Model.Video;
using VLC_WinRT.Services.RunTime;
using VLC_WinRT.ViewModels.MusicVM;
using VLC_WinRT.Commands;
using VLC_WinRT.BackgroundAudioPlayer.Model;
#if WINDOWS_PHONE_APP
using Windows.Media.Playback;
#endif
using libVLCX;
using VLC_WinRT.Utils;
using WinRTXamlToolkit.Controls.Extensions;
using VLC_WinRT.Commands.VideoPlayer;
using VLC_WinRT.Commands.VideoLibrary;

namespace VLC_WinRT.ViewModels
{
    public sealed class MediaPlaybackViewModel : BindableBase, IDisposable
    {
        #region private props
        private MouseService _mouseService;
        private SystemMediaTransportControls _systemMediaTransportControls;
        private PlayerEngine _playerEngine;
        private bool _isPlaying;
        private MediaState _mediaState;
        private PlayingType _playingType;
        private TrackCollection _trackCollection;
        private TimeSpan _timeTotal;

        private int _currentSubtitle;
        private int _currentAudioTrack;

        private readonly DisplayRequest _displayAlwaysOnRequest = new DisplayRequest();

        private int _volume = 100;
        private bool _isRunning;
        private int _speedRate;
        private bool _isStream;
        private int _bufferingProgress;
        #endregion

        #region private fields
        private List<DictionaryKeyValue> _subtitlesTracks = new List<DictionaryKeyValue>();
        private List<DictionaryKeyValue> _audioTracks = new List<DictionaryKeyValue>();
        private bool _isBuffered;
        #endregion

        #region public props
        public MouseService MouseService { get { return _mouseService; } }
        public TaskCompletionSource<bool> ContinueIndexing { get; set; }
        public bool UseVlcLib { get; set; }
        
        public IVLCMedia CurrentMedia
        {
            get
            {
                if (TrackCollection.CurrentTrack == -1) return null;
                if (TrackCollection.CurrentTrack == TrackCollection.Playlist.Count)
                    TrackCollection.CurrentTrack--;
                if (TrackCollection.Playlist.Count == 0) return null;
                return TrackCollection.Playlist[TrackCollection.CurrentTrack];
            }
        }

        public IMediaService _mediaService
        {
            get
            {
                switch (_playerEngine)
                {
                    case PlayerEngine.VLC:
                        return Locator.VLCService;
                    case PlayerEngine.MediaFoundation:
                        return App.Container.Resolve<MFService>();
#if WINDOWS_PHONE_APP
                    case PlayerEngine.BackgroundMFPlayer:
                        return App.Container.Resolve<BGPlayerService>();
#endif
                    default:
                        //todo : Implement properly BackgroundPlayerService 
                        //todo : so we get rid ASAP of this default switch
                        return Locator.VLCService;
                }
            }
        }

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
                SetProperty(ref _speedRate, value);
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

        public PlayPauseCommand PlayOrPauseCommand { get; } = new PlayPauseCommand();

        public PlayNextCommand PlayNextCommand { get; } = new PlayNextCommand();

        public PlayPreviousCommand PlayPreviousCommand { get; } = new PlayPreviousCommand();

        public SetSubtitleTrackCommand SetSubtitleTrackCommand { get; } = new SetSubtitleTrackCommand();

        public OpenSubtitleCommand OpenSubtitleCommand { get; } = new OpenSubtitleCommand();

        public SetAudioTrackCommand SetAudioTrackCommand { get; } = new SetAudioTrackCommand();

        public PickVideoCommand PickVideoCommand { get; } = new PickVideoCommand();

        public StopVideoCommand GoBack { get; } = new StopVideoCommand();

        public TimeSpan TimeTotal
        {
            get { return _timeTotal; }
            set { SetProperty(ref _timeTotal, value); }
        }

        public int BufferingProgress => _bufferingProgress;

        public bool IsBuffered
        {
            get { return _isBuffered; }
            set { SetProperty(ref _isBuffered, value); }
        }

        /**
         * Elasped time in milliseconds
         */
        public long Time
        {
            get
            {
                return _mediaService.GetTime();
            }
            set
            {
                _mediaService.SetTime(value);
            }
        }

        public float Position
        {
            get
            {
                return _mediaService.GetPosition();
            }
            set
            {
                _mediaService.SetPosition(value);
            }
        }

        public DictionaryKeyValue CurrentSubtitle
        {
            get
            {
                if (_currentSubtitle < 0 || _currentSubtitle >= Subtitles.Count)
                    return null;
                return Subtitles[_currentSubtitle];
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
            _mouseService = App.Container.Resolve<MouseService>();
        }
        #endregion

        #region methods
        public async Task OpenFile(StorageFile file)
        {
            if (file == null) return;
            await Locator.VLCService.PlayerInstanceReady.Task;
            var token = StorageApplicationPermissions.FutureAccessList.Add(file);
            if (VLCFileExtensions.FileTypeHelper(file.FileType) == VLCFileExtensions.VLCFileType.Video)
            {
                await PlayVideoFile(file, token);
            }
            else
            {
                await PlayAudioFile(file, token);
            }
        }

        /// <summary>
        /// Navigates to the Audio Player screen with the requested file a parameter.
        /// </summary>
        /// <param name="file">The file to be played.</param>
        /// <param name="token">Token is for files that are NOT in the sandbox, such as files taken from the filepicker from a sd card but not in the Video/Music folder.</param>
        public async Task PlayAudioFile(StorageFile file, string token = null)
        {
            Locator.NavigationService.Go(VLCPage.MusicPlayerPage);
            var trackItem = await MusicLibraryManagement.GetTrackItemFromFile(file, token);
            await PlaylistHelper.PlayTrackFromFilePicker(trackItem);
        }

        /// <summary>
        /// Navigates to the Video Player screen with the requested file a parameter.
        /// </summary>
        /// <param name="file">The file to be played.</param>
        /// <param name="token">Token is for files that are NOT in the sandbox, such as files taken from the filepicker from a sd card but not in the Video/Music folder.</param>
        public async Task PlayVideoFile(StorageFile file, string token = null)
        {
            Locator.NavigationService.Go(VLCPage.VideoPlayerPage);
            VideoItem videoVm = new VideoItem();
            await videoVm.Initialize(file);
            if (token != null)
                videoVm.Token = token;
            Locator.VideoVm.CurrentVideo = videoVm;
            await PlaylistHelper.Play(videoVm);
        }

        private void privateDisplayCall(bool shouldActivate)
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
            catch { }
        }

        private async void UpdateTime(Int64 time)
        {
            await UpdateTimeFromUIThread();
        }

        private async Task UpdateTimeFromUIThread()
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

        public async Task CleanViewModel()
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                _mediaService.Stop();
                PlayingType = PlayingType.NotPlaying;
                IsPlaying = false;
                TimeTotal = TimeSpan.Zero;
#if WINDOWS_PHONE_APP
                // music clean
                if (BackgroundMediaPlayer.Current != null &&
                    BackgroundMediaPlayer.Current.CurrentState != MediaPlayerState.Stopped)
                {
                    BackgroundMediaPlayer.Current.Pause();
                    await App.BackgroundAudioHelper.ResetCollection(ResetType.NormalReset);
                }
#endif
                await TrackCollection.ResetCollection();
                TrackCollection.IsRunning = false;
            });
        }

        private void OnPlaybackStarting()
        {
            if (Locator.NavigationService.CurrentPage == VLCPage.VideoPlayerPage ||
                Locator.NavigationService.CurrentPage == VLCPage.MusicPlayerPage)
            {
                privateDisplayCall(true);
                // video playback only
                _mouseService.HideMouse();
            }
        }

        private void OnPlaybackStopped()
        {
            if (Locator.NavigationService.CurrentPage == VLCPage.VideoPlayerPage ||
                Locator.NavigationService.CurrentPage == VLCPage.MusicPlayerPage)
            {
                privateDisplayCall(false);
                _mouseService.RestoreMouse();
            }
        }

        public async void OnLengthChanged(Int64 length)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (length < 0) return;
                TimeTotal = TimeSpan.FromMilliseconds(length);
            });
        }

        private async void OnStopped(IMediaService mediaService)
        {
            Debug.WriteLine("OnStopped event called from " + mediaService);
            mediaService.MediaFailed -= _mediaService_MediaFailed;
            mediaService.StatusChanged -= PlayerStateChanged;
            mediaService.TimeChanged -= UpdateTime;

            mediaService.OnLengthChanged -= OnLengthChanged;
            mediaService.OnStopped -= OnStopped;
            mediaService.OnEndReached -= OnEndReached;
            mediaService.OnBuffering -= MediaServiceOnOnBuffering;

            if (mediaService is VLCService)
            {
                var vlcService = (VLCService)mediaService;
                var em = vlcService.MediaPlayer.eventManager();
                em.OnTrackAdded -= OnTrackAdded;
                em.OnTrackDeleted -= OnTrackDeleted;

                _audioTracks.Clear();
                _subtitlesTracks.Clear();
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    CurrentAudioTrack = null;
                    CurrentSubtitle = null;
                    OnPropertyChanged("AudioTracks");
                    OnPropertyChanged("Subtitles");
                    OnPropertyChanged("CurrentAudioTrack");
                    OnPropertyChanged("CurrentSubtitle");
                });
            }
            else if (mediaService is MFService)
            {
                var mfService = (MFService)mediaService;
                mfService.Instance.Source = null;
            }
            mediaService.SetNullMediaPlayer();
        }

        public async Task SetMedia(IVLCMedia media, bool forceVlcLib = false, bool autoPlay = true)
        {
            if (media == null)
                throw new ArgumentNullException("media", "Media parameter is missing. Can't play anything");
            Stop();
            UseVlcLib = forceVlcLib;

            if (media is VideoItem)
            {
                // First things first: we need to pause the slideshow here before any action is done by VLC
                Locator.Slideshow.IsPaused = true;
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    PlayingType = PlayingType.Video;
                    IsStream = false;
                    if (Locator.NavigationService.CurrentPage == VLCPage.VideoPlayerPage)
                    {
                        Locator.NavigationService.GoBack_Default();
                    }
                    Locator.NavigationService.Go(VLCPage.VideoPlayerPage);
                });
                var video = (VideoItem)media;
                await Locator.MediaPlaybackViewModel.InitializePlayback(video, autoPlay);
                await Locator.VideoVm.TryUseSubtitleFromFolder();

                if (video.TimeWatched != TimeSpan.FromSeconds(0))
                    await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Locator.MediaPlaybackViewModel.Time = (Int64)video.TimeWatched.TotalMilliseconds);

                await SetMediaTransportControlsInfo(string.IsNullOrEmpty(video.Name) ? "Video" : video.Name);
                UpdateTileHelper.UpdateMediumTileWithVideoInfo();
            }
            else if (media is TrackItem)
            {
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    IsStream = false;
                    PlayingType = PlayingType.Music;
                });
                var track = (TrackItem)media;
                StorageFile currentTrackFile;
                try
                {
                    currentTrackFile = track.File ?? await StorageFile.GetFileFromPathAsync(track.Path);
                }
                catch (Exception exception)
                {
                    await MusicLibraryManagement.RemoveTrackFromCollectionAndDatabase(track);
                    await Task.Delay(500);

                    if (TrackCollection.CanGoNext)
                    {
                        await PlayNext();
                    }
                    else
                    {
                        await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => Locator.NavigationService.GoBack_Specific());
                    }
                    return;
                }
                await Locator.MediaPlaybackViewModel.InitializePlayback(track, autoPlay);
                if (_playerEngine != PlayerEngine.BackgroundMFPlayer)
                {
                    await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        await Locator.MusicPlayerVM.SetCurrentArtist();
                        await Locator.MusicPlayerVM.SetCurrentAlbum();
                        await Locator.MusicPlayerVM.UpdatePlayingUI();
                        await Locator.MusicPlayerVM.Scrobble();
#if WINDOWS_APP
                        await Locator.MusicPlayerVM.UpdateWindows8UI();
#endif
                        if (Locator.MusicPlayerVM.CurrentArtist != null)
                        {
                            Locator.MusicPlayerVM.CurrentArtist.PlayCount++;
                            await Locator.MusicLibraryVM._artistDatabase.Update(Locator.MusicPlayerVM.CurrentArtist);
                        }
                    });
                }
                ApplicationSettingsHelper.SaveSettingsValue(BackgroundAudioConstants.CurrentTrack, TrackCollection.CurrentTrack);
            }
            else if (media is StreamMedia)
            {
                UseVlcLib = true;
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Locator.VideoVm.CurrentVideo = null;
                    Locator.MediaPlaybackViewModel.PlayingType = PlayingType.Video;
                    IsStream = true;
                });
                await Locator.MediaPlaybackViewModel.InitializePlayback(media, autoPlay);
            }
        }

        public async Task InitializePlayback(IVLCMedia media, bool autoPlay)
        {
            // First set the player engine
            // For videos AND music, we have to try first with Microsoft own player
            // Then we register to Failed callback. If it doesn't work, we set ForceVlcLib to true
            if (UseVlcLib)
                _playerEngine = PlayerEngine.VLC;
            else
            {
                var path = "";
                if (!string.IsNullOrEmpty(media.Path))
                    path = Path.GetExtension(media.Path);
                else if (media.File != null)
                    path = media.File.FileType;
                if (media is TrackItem)
                {
                    if (VLCFileExtensions.MFSupported.Contains(path.ToLower()))
                    {
#if WINDOWS_PHONE_APP
                        _playerEngine = PlayerEngine.BackgroundMFPlayer;
#else
                        _playerEngine = PlayerEngine.VLC;
#endif
                    }
                    else
                    {
                        ToastHelper.Basic("This file might not play in background", false, "background");
                        _playerEngine = PlayerEngine.VLC;
                        _mediaService.Stop();
                    }
                }
                else
                {
                    _playerEngine = PlayerEngine.VLC; 
                }
            }

            // Now, ensure the chosen Player is ready to play something
            await Locator.MediaPlaybackViewModel._mediaService.PlayerInstanceReady.Task;

            _mediaService.MediaFailed += _mediaService_MediaFailed;
            _mediaService.StatusChanged += PlayerStateChanged;
            _mediaService.TimeChanged += UpdateTime;

            // Send the media we want to play
            await _mediaService.SetMediaFile(media);

            _mediaService.OnLengthChanged += OnLengthChanged;
            _mediaService.OnStopped += OnStopped;
            _mediaService.OnEndReached += OnEndReached;
            _mediaService.OnBuffering += MediaServiceOnOnBuffering;

            switch (_playerEngine)
            {
                case PlayerEngine.VLC:
                    var vlcService = (VLCService)_mediaService;
                    if (vlcService.MediaPlayer == null) return;
                    var em = vlcService.MediaPlayer.eventManager();
                    em.OnTrackAdded += Locator.MediaPlaybackViewModel.OnTrackAdded;
                    em.OnTrackDeleted += Locator.MediaPlaybackViewModel.OnTrackDeleted;

                    if (!autoPlay) return;
                    vlcService.Play();
                    break;
                case PlayerEngine.MediaFoundation:
                    var mfService = (MFService)_mediaService;
                    if (mfService == null) return;

                    if (!autoPlay) return;
                    _mediaService.Play();
                    break;
                case PlayerEngine.BackgroundMFPlayer:
                    if (!autoPlay) return;
                    _mediaService.Play(CurrentMedia.Id);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => SpeedRate = 100);
        }

        private async void MediaServiceOnOnBuffering(int f)
        {
            _bufferingProgress = f;
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                IsBuffered = f == 100;
                OnPropertyChanged("BufferingProgress");
            });
        }

        async void _mediaService_MediaFailed(object sender, EventArgs e)
        {
            if (sender is MFService)
            {
                // MediaFoundation failed to open the media, switching to VLC player
                await SetMedia(CurrentMedia, true);
            }
            else
            {
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    var md = new MessageDialog("Your media cannot be read.", "We're sorry");
                    await md.ShowAsyncQueue();
                    // ensure we call Stop so we unregister all events
                    Stop();
                });
            }
        }


        async void OnEndReached()
        {
            bool canGoNext = TrackCollection.Playlist.Count > 0 && TrackCollection.CanGoNext;
            if (!canGoNext)
            {
                // Playlist is finished
                if (TrackCollection.Repeat)
                {
                    // ... One More Time!
                    await StartAgain();
                    return;
                }
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    TrackCollection.IsRunning = false;
                    IsPlaying = false;
                    PlayingType = PlayingType.NotPlaying;
                    if (!Locator.NavigationService.GoBack_Default())
                    {
                        Locator.MainVM.GoToPanelCommand.Execute(0);
                    }
                });
            }
            switch (PlayingType)
            {
                case PlayingType.Music:
                    break;
                case PlayingType.Video:
                    await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                    {
                        if (Locator.VideoVm.CurrentVideo != null)
                            Locator.VideoVm.CurrentVideo.TimeWatchedSeconds = 0;
                    });
                    if (Locator.VideoVm.CurrentVideo != null)
                        await Locator.VideoLibraryVM.VideoRepository.Update(Locator.VideoVm.CurrentVideo).ConfigureAwait(false);
                    break;
                case PlayingType.NotPlaying:
                    break;
                default:
                    break;
            }
            if (canGoNext)
                await PlayNext();
        }

        public async Task StartAgain()
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                TrackCollection.CurrentTrack = 0;
                await Locator.MediaPlaybackViewModel.SetMedia(CurrentMedia, false);
            });
        }
        
        public async Task PlayNext()
        {
            if (TrackCollection.CanGoNext)
            {
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    TrackCollection.CurrentTrack++;
                    await Locator.MediaPlaybackViewModel.SetMedia(CurrentMedia, false);
                });
            }
            else
            {
                UpdateTileHelper.ClearTile();
            }
        }

        public async Task PlayPrevious()
        {
            if (TrackCollection.CanGoPrevious)
            {
                await DispatchHelper.InvokeAsync(() => TrackCollection.CurrentTrack--);
                await Locator.MediaPlaybackViewModel.SetMedia(CurrentMedia, false);
            }
            else
            {
                UpdateTileHelper.ClearTile();
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
                Locator.VideoVm.CurrentVideo.TimeWatchedSeconds = (int)((double)Time/1000);;
                await Locator.VideoLibraryVM.VideoRepository.Update(Locator.VideoVm.CurrentVideo).ConfigureAwait(false);
            }
        }

        public void SetSubtitleTrack(int i)
        {
            switch (_playerEngine)
            {
                case PlayerEngine.VLC:
                    var vlcService = (VLCService)_mediaService;
                    vlcService.MediaPlayer?.setSpu(i);
                    break;
                case PlayerEngine.MediaFoundation:
                    break;
                case PlayerEngine.BackgroundMFPlayer:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetAudioTrack(int i)
        {
            switch (_playerEngine)
            {
                case PlayerEngine.VLC:
                    var vlcService = (VLCService)_mediaService;
                    vlcService.MediaPlayer?.setAudioTrack(i);
                    break;
                case PlayerEngine.MediaFoundation:
                    break;
                case PlayerEngine.BackgroundMFPlayer:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OpenSubtitleMrl(string mrl)
        {
            switch (_playerEngine)
            {
                case PlayerEngine.VLC:
                    var vlcService = (VLCService)_mediaService;
                    vlcService.MediaPlayer?.setSubtitleFile(mrl);
                    break;
                case PlayerEngine.MediaFoundation:
                    break;
                case PlayerEngine.BackgroundMFPlayer:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetRate(float rate)
        {
            switch (_playerEngine)
            {
                case PlayerEngine.VLC:
                    _mediaService.SetSpeedRate(rate);
                    break;
                case PlayerEngine.MediaFoundation:
                    _mediaService.SetSpeedRate(rate);
                    break;
                case PlayerEngine.BackgroundMFPlayer:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Stop()
        {
            _mediaService.Stop();
        }
        #endregion

        #region Events
        private async void PlayerStateChanged(object sender, MediaState e)
        {
            try
            {
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    IsPlaying = e == MediaState.Playing || e == MediaState.Buffering;
                    MediaState = e;

                    switch (MediaState)
                    {
                        case MediaState.NothingSpecial:
                            break;
                        case MediaState.Opening:
                            break;
                        case MediaState.Buffering:
                            break;
                        case MediaState.Playing:
                            if (_systemMediaTransportControls != null)
                                _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                            break;
                        case MediaState.Paused:
                            if (_systemMediaTransportControls != null)
                                _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Paused;
                            break;
                        case MediaState.Stopped:
                            break;
                        case MediaState.Ended:
                            break;
                        case MediaState.Error:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                });
            }
            catch { }
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
                source = ((VLCService)_mediaService).MediaPlayer.audioTrackDescription();
            }
            else
            {
                target = _subtitlesTracks;
                source = ((VLCService)_mediaService).MediaPlayer.spuDescription();
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

        #region MediaTransportControls

        public void SetMediaTransportControls(SystemMediaTransportControls systemMediaTransportControls)
        {
#if WINDOWS_APP
            ForceMediaTransportControls(systemMediaTransportControls);
#elif WINDOWS_PHONE_APP
            if (BackgroundMediaPlayer.Current != null &&
                BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Playing)
            {

            }
            else
            {
                ForceMediaTransportControls(systemMediaTransportControls);
            }
#endif
        }

        void ForceMediaTransportControls(SystemMediaTransportControls systemMediaTransportControls)
        {
            try
            {
                _systemMediaTransportControls = systemMediaTransportControls;
                _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Closed;
                _systemMediaTransportControls.ButtonPressed += SystemMediaTransportControlsOnButtonPressed;
                _systemMediaTransportControls.IsEnabled = false;
            }
            catch (Exception exception)
            { }
        }

        public async Task SetMediaTransportControlsInfo(string artistName, string albumName, string trackName, string albumUri)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                try
                {
                    if (_systemMediaTransportControls == null) return;
                    _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                    _systemMediaTransportControls.IsEnabled = true;
                    _systemMediaTransportControls.IsPauseEnabled = true;
                    _systemMediaTransportControls.IsPlayEnabled = true;

                    SystemMediaTransportControlsDisplayUpdater updater = _systemMediaTransportControls.DisplayUpdater;
                    updater.Type = MediaPlaybackType.Music;
                    // Music metadata.
                    updater.MusicProperties.AlbumArtist = artistName;
                    updater.MusicProperties.Artist = artistName;
                    updater.MusicProperties.Title = trackName;

                    // Set the album art thumbnail.
                    // RandomAccessStreamReference is defined in Windows.Storage.Streams

                    if (albumUri != null && !string.IsNullOrEmpty(albumUri))
                    {
                        updater.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(albumUri));
                    }

                    // Update the system media transport controls.
                    updater.Update();
                }
                catch (Exception exception)
                { }
            });
        }

        public async Task SetMediaTransportControlsInfo(string title)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                try
                {
                    if (_systemMediaTransportControls == null) return;
                    LogHelper.Log("PLAYVIDEO: Updating SystemMediaTransportControls");
                    SystemMediaTransportControlsDisplayUpdater updater = _systemMediaTransportControls.DisplayUpdater;
                    updater.Type = MediaPlaybackType.Video;
                    _systemMediaTransportControls.IsPreviousEnabled = false;
                    _systemMediaTransportControls.IsNextEnabled = false;
                    //Video metadata
                    updater.VideoProperties.Title = title;
                    //TODO: add full thumbnail suport
                    updater.Thumbnail = null;
                    updater.Update();
                }
                catch (Exception exception)
                {
                    ExceptionHelper.CreateMemorizedException("MediaPlaybackViewModel.SetMediaTransportControls(title)", exception);
                }
            });
        }

        private async void SystemMediaTransportControlsOnButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Pause:
                case SystemMediaTransportControlsButton.Play:
                    _mediaService.Pause();
                    break;
                case SystemMediaTransportControlsButton.Stop:
                    Stop();
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    if (Locator.MediaPlaybackViewModel.PlayingType == PlayingType.Music)
                        await Locator.MediaPlaybackViewModel.PlayPrevious();
                    break;
                case SystemMediaTransportControlsButton.Next:
                    if (Locator.MediaPlaybackViewModel.PlayingType == PlayingType.Music)
                        await Locator.MediaPlaybackViewModel.PlayNext();
                    break;
            }
        }

        public void SystemMediaTransportControlsBackPossible(bool backPossible)
        {
            try
            {
                if (_systemMediaTransportControls != null) _systemMediaTransportControls.IsPreviousEnabled = backPossible;
            }
            catch { }
        }

        public void SystemMediaTransportControlsNextPossible(bool nextPossible)
        {
            try
            {
                if (_systemMediaTransportControls != null) _systemMediaTransportControls.IsNextEnabled = nextPossible;
            }
            catch
            {
            }
        }
        #endregion

        public void Pause()
        {
            _mediaService.Pause();
        }

        public void Dispose()
        {
            _mediaService.Stop();
        }
    }
}
