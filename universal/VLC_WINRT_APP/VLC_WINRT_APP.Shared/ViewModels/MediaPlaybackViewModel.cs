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
using Windows.System;
using System.Collections.Generic;
using VLC_WINRT_APP.Commands.Video;
using VLC_WINRT_APP.Views.MainPages;
using System.Diagnostics;
using Windows.Storage;
using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using Windows.UI.Core;
using VLC_WINRT_APP.Commands;
using VLC_WINRT_APP.Common;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Services.Interface;
using Windows.System.Display;
using VLC_WINRT_APP.Commands.MediaPlayback;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Autofac;
using libVLCX;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Helpers.MusicLibrary;
using VLC_WINRT_APP.Helpers.MusicPlayer;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.Model.Stream;
using VLC_WINRT_APP.Model.Video;
using VLC_WINRT_APP.Services.RunTime;
using VLC_WINRT_APP.ViewModels.MusicVM;
using VLC_WINRT_APP.Views.MusicPages;
using VLC_WINRT_APP.Views.VideoPages;
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
        private SystemMediaTransportControls _systemMediaTransportControls;
        private PlayerEngine _playerEngine;
        private bool _isPlaying;
        private MediaState _mediaState;
        private PlayingType _playingType;
        private TrackCollection _trackCollection;
        private TimeSpan _timeTotal;
        private ActionCommand _skipAhead;
        private ActionCommand _skipBack;
        private PlayNextCommand _playNext = new PlayNextCommand();
        private PlayPreviousCommand _playPrevious = new PlayPreviousCommand();
        private PlayPauseCommand _playOrPause = new PlayPauseCommand();

        private int _currentSubtitle;
        private int _currentAudioTrack;

        private SetSubtitleTrackCommand _setSubTitlesCommand = new SetSubtitleTrackCommand();
        private OpenSubtitleCommand _openSubtitleCommand = new OpenSubtitleCommand();
        private SetAudioTrackCommand _setAudioTrackCommand = new SetAudioTrackCommand();
        private StopVideoCommand _goBackCommand = new StopVideoCommand();

        private readonly DisplayRequest _displayAlwaysOnRequest = new DisplayRequest();

        private int _volume = 100;
        private bool _isRunning;
        private int _speedRate;
        private bool _isStream;
        private int _bufferingProgress;

        private IVLCMedia _currentMedia;
        #endregion

        #region private fields
        private List<DictionaryKeyValue> _subtitlesTracks = new List<DictionaryKeyValue>();
        private List<DictionaryKeyValue> _audioTracks = new List<DictionaryKeyValue>();
        private bool _isBuffered;

        #endregion

        #region public props
        public TaskCompletionSource<bool> ContinueIndexing { get; set; }
        public bool UseVlcLib { get; set; }

        public IMediaService _mediaService
        {
            get
            {
                switch (_playerEngine)
                {
                    case PlayerEngine.VLC:
                        return App.Container.Resolve<VLCService>();
                    case PlayerEngine.MediaFoundation:
                        return App.Container.Resolve<MFService>();
                    default:
                        //todo : Implement properly BackgroundPlayerService 
                        //todo : so we get rid ASAP of this default switch
                        return App.Container.Resolve<VLCService>();
                }
                return null;
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

        public int BufferingProgress
        {
            get { return _bufferingProgress; }
        }

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
                switch (_playerEngine)
                {
                    case PlayerEngine.VLC:
                        return _mediaService.GetTime();
                    case PlayerEngine.MediaFoundation:
                        return _mediaService.GetTime();
                    case PlayerEngine.BackgroundMFPlayer:
                        // CurrentState keeps failing OnCanceled, even though it actually has a state.
                        // The error is useless for now, so try catch and ignore.
#if WINDOWS_PHONE_APP
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
#endif
                        return 0;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            set
            {
                switch (_playerEngine)
                {
                    case PlayerEngine.VLC:
                        _mediaService.SetTime(value);
                        break;
                    case PlayerEngine.MediaFoundation:
                        _mediaService.SetTime(value);
                        break;
                    case PlayerEngine.BackgroundMFPlayer:
                        // Same as with "Time", BackgroundMediaPlayer.Current.CurrentState sometimes blows up on OnCancelled. So for now, throw it in a try catch.
                        // This really seems like an WinRT API bug to me...
#if WINDOWS_PHONE_APP
                        try
                        {
                            BackgroundMediaPlayer.Current.Position = TimeSpan.FromMilliseconds(value);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Error with position : " + ex.Message);
                        }
#endif
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public float Position
        {
            get
            {
                switch (_playerEngine)
                {
                    case PlayerEngine.VLC:
                        return _mediaService.GetPosition();
                    case PlayerEngine.MediaFoundation:
                        return _mediaService.GetPosition();
                    case PlayerEngine.BackgroundMFPlayer:
#if WINDOWS_PHONE_APP
                        switch (BackgroundMediaPlayer.Current.CurrentState)
                        {
                            case MediaPlayerState.Playing:
                                var pos = (BackgroundMediaPlayer.Current.Position.TotalMilliseconds / BackgroundMediaPlayer.Current.NaturalDuration.TotalMilliseconds);
                                float posfloat = (float)pos;
                                return posfloat;
                            case MediaPlayerState.Closed:
                                break;
                        }
#endif
                        return 0;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            set
            {
                // We shouldn't be able to set the position without a running playback.
                switch (_playerEngine)
                {
                    case PlayerEngine.VLC:
                        _mediaService.SetPosition(value);
                        break;
                    case PlayerEngine.MediaFoundation:
                        _mediaService.SetPosition(value);
                        break;
                    case PlayerEngine.BackgroundMFPlayer:
#if WINDOWS_PHONE_APP
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
#endif
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
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

        public IVLCMedia CurrentMedia { get { return _currentMedia; } }

        #endregion
        #region constructors

        public MediaPlaybackViewModel()
        {
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
#if WINDOWS_APP
            _mouseService = App.Container.Resolve<MouseService>();
#endif
        }
        #endregion

        #region methods

        public async Task OpenFile(StorageFile file)
        {
            if (file == null) return;
            if (VLCFileExtensions.FileTypeHelper(file.FileType) == VLCFileExtensions.VLCFileType.Video)
            {
                var token = StorageApplicationPermissions.FutureAccessList.Add(file);
                await PlayVideoFile(file, token);
            }
            else
            {
                await PlayAudioFile(file);
            }
        }

        /// <summary>
        /// Navigates to the Audio Player screen with the requested file a parameter.
        /// </summary>
        /// <param name="file">The file to be played.</param>
        public async Task PlayAudioFile(StorageFile file)
        {
            if (App.ApplicationFrame.CurrentSourcePageType != typeof(MusicPlayerPage))
                App.ApplicationFrame.Navigate(typeof(MusicPlayerPage));
            var trackItem = await GetInformationsFromMusicFile.GetTrackItemFromFile(file);
            await PlayMusicHelper.PlayTrackFromFilePicker(trackItem);
        }

        /// <summary>
        /// Navigates to the Video Player screen with the requested file a parameter.
        /// </summary>
        /// <param name="file">The file to be played.</param>
        /// <param name="token">Token is for files that are NOT in the sandbox, such as files taken from the filepicker from a sd card but not in the Video/Music folder.</param>
        public async Task PlayVideoFile(StorageFile file, string token = null)
        {
            App.ApplicationFrame.Navigate(typeof(VideoPlayerPage));
            VideoItem videoVm = new VideoItem();
            await videoVm.Initialize(file);
            if (token != null)
                videoVm.Token = token;
            Locator.VideoVm.CurrentVideo = videoVm;
            await Locator.MediaPlaybackViewModel.SetMedia(videoVm, false);
        }


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
            await UpdateTimeFromUIThread();
        }

        public async void UpdateTimeFromMF()
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
            privateDisplayCall(true);
#if WINDOWS_APP
            // video playback only
            _mouseService.HideMouse();
#endif
        }

        private void OnPlaybackStopped()
        {
            privateDisplayCall(false);
#if WINDOWS_APP
            _mouseService.RestoreMouse();
#endif
        }

        public async void OnLengthChanged(Int64 length)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
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
            _mediaService.OnBuffering -= MediaServiceOnOnBuffering;

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
                });
            }
            else if (mediaService is MFService)
            {
                var mfService = (MFService)mediaService;
                mfService.Instance.Source = null;
            }
            mediaService.SetNullMediaPlayer();
        }

        public async Task SetMedia(IVLCMedia media, bool forceVlcLib = false)
        {
            if (media == null)
                throw new ArgumentNullException("media", "Media parameter is missing. Can't play anything");
            Stop();
            _currentMedia = media;
            UseVlcLib = forceVlcLib;

            if (media is VideoItem)
            {
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    PlayingType = PlayingType.Video;
                    IsStream = false;
                });
                var video = (VideoItem)media;
                await Locator.MediaPlaybackViewModel.InitializePlayback(video);

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
                var currentTrackFile = track.File ?? await StorageFile.GetFileFromPathAsync(track.Path);
#if WINDOWS_PHONE_APP
                bool playWithLibVlc = !VLCFileExtensions.MFSupported.Contains(currentTrackFile.FileType.ToLower()) || Locator.MediaPlaybackViewModel.UseVlcLib;
                if (!playWithLibVlc)
                {
                    _playerEngine = PlayerEngine.BackgroundMFPlayer;
                    App.BackgroundAudioHelper.PlayAudio(track.Id);
                }
                else
#endif
                {
#if WINDOWS_PHONE_APP
                    Locator.MediaPlaybackViewModel.UseVlcLib = true;
                    ToastHelper.Basic("Can't enable background audio", false, "background");
                    if (BackgroundMediaPlayer.Current != null &&
                        BackgroundMediaPlayer.Current.CurrentState != MediaPlayerState.Stopped)
                    {
                        BackgroundMediaPlayer.Current.Pause();
                    }
#endif
                    await Locator.MediaPlaybackViewModel.InitializePlayback(track);
                    await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        await Locator.MusicPlayerVM.SetCurrentArtist();
                        await Locator.MusicPlayerVM.SetCurrentAlbum();
                        await Locator.MusicPlayerVM.UpdatePlayingUI();
#if WINDOWS_APP
                        await Locator.MusicPlayerVM.UpdateWindows8UI();
#endif
                    });
                }
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
                await Locator.MediaPlaybackViewModel.InitializePlayback(media);
            }
        }

        public async Task InitializePlayback(IVLCMedia media)
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
                _playerEngine = VLCFileExtensions.MFSupported.Contains(path.ToLower()) ? PlayerEngine.MediaFoundation : PlayerEngine.VLC;
            }
            // else WindowsPhone backgroundmediaplayer etc. (this is a todo )

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
                    vlcService.Play();
                    break;
                case PlayerEngine.MediaFoundation:
                    var mfService = (MFService)_mediaService;
                    if (mfService == null) return;
                    _mediaService.Play();
                    break;
                case PlayerEngine.BackgroundMFPlayer:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // todo: implement this thing
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
                await SetMedia(_currentMedia, true);
            }
            else
            {
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    var md = new MessageDialog("Your media cannot be read.", "We're sorry");
                    await md.ShowAsync();
                    // ensure we call Stop so we unregister all events
                    Stop();
                });
            }
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
                    await Locator.MediaPlaybackViewModel.SetMedia(Locator.MusicPlayerVM.CurrentTrack, false);
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
                await Locator.MediaPlaybackViewModel.SetMedia(Locator.MusicPlayerVM.CurrentTrack, false);
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
            switch (_playerEngine)
            {
                case PlayerEngine.VLC:
                    var vlcService = (VLCService)_mediaService;
                    if (vlcService.MediaPlayer != null) vlcService.MediaPlayer.setSpu(i);
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
                    if (vlcService.MediaPlayer != null) vlcService.MediaPlayer.setAudioTrack(i);
                    break;
                case PlayerEngine.MediaFoundation:
                    break;
                case PlayerEngine.BackgroundMFPlayer:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OpenSubtitle(string mrl)
        {
            switch (_playerEngine)
            {
                case PlayerEngine.VLC:
                    var vlcService = (VLCService)_mediaService;
                    if (vlcService.MediaPlayer != null) vlcService.MediaPlayer.setSubtitleFile(mrl);
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
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                IsPlaying = e == MediaState.Playing;
                OnPropertyChanged("IsPlaying");
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
#else
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
                {}
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
                    else
                        Locator.MediaPlaybackViewModel.SkipBack.Execute("");
                    break;
                case SystemMediaTransportControlsButton.Next:
                    if (Locator.MediaPlaybackViewModel.PlayingType == PlayingType.Music)
                        await Locator.MediaPlaybackViewModel.PlayNext();
                    else
                        Locator.MediaPlaybackViewModel.SkipAhead.Execute("");
                    break;
            }
        }

        public void SystemMediaTransportControlsBackPossible(bool backPossible)
        {
            if (_systemMediaTransportControls != null) _systemMediaTransportControls.IsPreviousEnabled = backPossible;
        }

        public void SystemMediaTransportControlsNextPossible(bool nextPossible)
        {
            if (_systemMediaTransportControls != null) _systemMediaTransportControls.IsNextEnabled = nextPossible;
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
