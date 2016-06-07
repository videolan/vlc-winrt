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
using Windows.UI.Core;
using VLC_WinRT.Helpers;
using VLC_WinRT.Model;
using VLC_WinRT.Services.Interface;
using VLC_WinRT.Commands.MediaPlayback;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Autofac;
using VLC_WinRT.Services.RunTime;
using VLC_WinRT.ViewModels.MusicVM;
#if WINDOWS_PHONE_APP
using Windows.Media.Playback;
#endif
using libVLCX;
using VLC_WinRT.Utils;
using VLC_WinRT.Commands.VideoPlayer;
using VLC_WinRT.Commands.VideoLibrary;
using VLC_WinRT.SharedBackground.Database;
using System.Linq;
using VLC_WinRT.Helpers.UIHelpers;
using VLC_WinRT.Commands.MusicPlayer;
using VLC_WinRT.MediaMetaFetcher.Fetchers;
using VLC_WinRT.Model.Video;
using VLC_WinRT.Model.Stream;
using Windows.UI.Popups;

namespace VLC_WinRT.ViewModels
{
    public sealed class MediaPlaybackViewModel : BindableBase, IDisposable
    {
        #region private props
        private MouseService _mouseService;
        private SystemMediaTransportControls _systemMediaTransportControls;
        private bool _isPlaying;
        private PlaybackService _playbackService;
        private TimeSpan _timeTotal;

        private int _volume = 100;
        private int _speedRate;
        private long _audioDelay;
        private long _spuDelay;
        private int _bufferingProgress;
        #endregion

        public PlaybackService PlaybackService
        {
            get
            {
                _playbackService = _playbackService ?? new PlaybackService();
                return _playbackService;
            }
        }
        public MouseService MouseService { get { return _mouseService; } }


        #region commands

        public PlayTrackCollCommand PlayTrackCollCommand { get; } = new PlayTrackCollCommand();

        public PlayPauseCommand PlayOrPauseCommand { get; } = new PlayPauseCommand();

        public PlayNextCommand PlayNextCommand { get; } = new PlayNextCommand();

        public PlayPreviousCommand PlayPreviousCommand { get; } = new PlayPreviousCommand();

        public OpenSubtitleCommand OpenSubtitleCommand { get; } = new OpenSubtitleCommand();

        public PickMediaCommand PickMediaCommand { get; } = new PickMediaCommand();

        public StopVideoCommand GoBack { get; } = new StopVideoCommand();

        public ChangePlaybackSpeedRateCommand ChangePlaybackSpeedRateCommand { get; } = new ChangePlaybackSpeedRateCommand();

        public ChangeVolumeCommand ChangeVolumeCommand { get; } = new ChangeVolumeCommand();

        public ChangeAudioDelayCommand ChangeAudioDelayCommand { get; } = new ChangeAudioDelayCommand();

        public ChangeSpuDelayCommand ChangeSpuDelayCommand { get; } = new ChangeSpuDelayCommand();
        #endregion

        #region public props

        public bool UseVlcLib { get; set; }

        public IMediaItem CurrentMedia
        {
            get
            {
                if (PlaybackService.CurrentMedia == -1)
                    return null;
                if (PlaybackService.Playlist.Count == 0)
                    return null;
                return PlaybackService.Playlist[PlaybackService.CurrentMedia];
            }
        }

        public bool IsPlaying
        {
            get
            {
                return _isPlaying;
            }
            private set
            {
                if (value != _isPlaying)
                {
                    SetProperty(ref _isPlaying, value);
                }
            }
        }

        public bool CanGoNext => PlaybackService.CanGoNext();
        public bool CanGoPrevious => PlaybackService.CanGoPrevious();

        public MediaState MediaState => PlaybackService.PlayerState;

        public int Volume
        {
            get
            {
                return _volume;
            }
            set
            {
                Debug.WriteLine("new volume set: " + value);
                if (value > 0 && value <= 100)
                {
                    PlaybackService.Volume = value;
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
                SetProperty(ref _speedRate, value);
                float r = (float)value / 100;
                PlaybackService.SetSpeedRate(r);
            }
        }

        /// <summary>
        /// Gets and Sets the AudioDelay in MilliSeconds
        /// Warning : VLC API needs microseconds, hence the 1000 multiplication
        /// </summary>
        public long AudioDelay
        {
            get
            {
                return _audioDelay;
            }
            set
            {
                PlaybackService.SetAudioDelay(value);
                SetProperty(ref _audioDelay, value);
            }
        }


        /// <summary>
        /// Gets and Sets the SpuDelay in MilliSeconds
        /// Warning : VLC API needs microseconds, hence the 1000 multiplication
        /// </summary>
        public long SpuDelay
        {
            get { return _spuDelay; }
            set
            {
                PlaybackService.SetSpuDelay(value);
                SetProperty(ref _spuDelay, value);
            }
        }

        public TimeSpan TimeTotal
        {
            get { return _timeTotal; }
            set { SetProperty(ref _timeTotal, value); }
        }

        public int BufferingProgress => _bufferingProgress;

        public bool IsBuffered => _bufferingProgress == 100;

        /**
         * Elasped time in milliseconds
         */
        public long Time
        {
            get
            {
                return PlaybackService.GetTime();
            }
            set
            {
                PlaybackService.SetTime(value);
            }
        }

        public float Position
        {
            get
            {
                return PlaybackService.GetPosition();
            }
            set
            {
                PlaybackService.SetPosition(value);
            }
        }

        public DictionaryKeyValue CurrentSubtitle
        {
            get
            {
                return PlaybackService.GetCurrentSubtitleTrack();
            }
            set
            {
                PlaybackService.SetSubtitleTrack(value);
            }
        }

        public DictionaryKeyValue CurrentAudioTrack
        {
            get
            {
                return PlaybackService.GetCurrentAudioTrack();
            }
            set
            {
                PlaybackService.SetAudioTrack(value);
            }
        }

        public VLCChapterDescription CurrentChapter
        {
            get
            {
                return PlaybackService.GetCurrentChapter();
            }
            set
            {
                PlaybackService.SetCurrentChapter(value);
            }
        }
        #endregion

        #region public fields
        public IEnumerable<DictionaryKeyValue> AudioTracks => PlaybackService.GetAudioTracks();

        public IEnumerable<DictionaryKeyValue> Subtitles => PlaybackService.GetSubtitleTracks();

        public IEnumerable<VLCChapterDescription> Chapters => PlaybackService.GetChapters();

        #endregion

        #region constructors
        public MediaPlaybackViewModel()
        {
            _mouseService = App.Container.Resolve<MouseService>();
            PlaybackService.Playback_StatusChanged += Playback_StatusChanged;
            PlaybackService.Playback_MediaTimeChanged += Playback_MediaTimeChanged;
            PlaybackService.Playback_MediaLengthChanged += Playback_MediaLengthChanged;
            PlaybackService.Playback_MediaBuffering += Playback_MediaBuffering;
            PlaybackService.Playback_MediaEndReached += Playback_MediaEndReach;
            PlaybackService.Playback_MediaFailed += Playback_MediaFailed;
            PlaybackService.Playback_MediaStopped += Playback_MediaStopped;
            PlaybackService.Playback_MediaTracksUpdated += Playback_MediaTracksUpdated;
            PlaybackService.Playback_MediaParsed += Playback_MediaParsed;

            PlaybackService.Playback_MediaSet += PlaybackService_MediaSet;
            App.Container.Resolve<NetworkListenerService>().InternetConnectionChanged += MediaPlaybackViewModel_InternetConnectionChanged;
        }

        private async void MediaPlaybackViewModel_InternetConnectionChanged(object sender, Model.Events.InternetConnectionChangedEventArgs e)
        {
            if (!e.IsConnected && IsPlaying && CurrentMedia is StreamMedia)
            {
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    GoBack.Execute(null);
                    var dialog = new MessageDialog(Strings.MediaCantBeRead.ToUpperFirstChar(), Strings.NoInternetConnection.ToUpperFirstChar());
                    await dialog.ShowAsync();
                });
            }
        }

        #endregion

        #region methods
        public async Task OpenFile(StorageFile file)
        {
            if (file == null) return;
            if (string.IsNullOrEmpty(file.Path))
            {
                // It's definitely a stream since it doesn't add a proper path but a FolderRelativeId
                // WARNING : Apps should use vlc://openstream/?from=url&url= for this matter
                var mrl = file.FolderRelativeId;
                var lastIndex = mrl.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase);
                if (lastIndex != -1)
                    mrl = mrl.Remove(0, lastIndex + "\\".Length);
                await PlayStream(mrl);
                return;
            }

            var token = StorageApplicationPermissions.FutureAccessList.Add(file);
            var fileType = VLCFileExtensions.FileTypeHelper(file.FileType);
            if (fileType == VLCFileExtensions.VLCFileType.Video)
            {
                await PlayVideoFile(file, token);
            }
            else if (fileType == VLCFileExtensions.VLCFileType.Audio)
            {
                await PlayAudioFile(file, token);
            }
            else if (fileType == VLCFileExtensions.VLCFileType.Subtitle)
            {
                if (IsPlaying && PlaybackService.PlayingType == PlayingType.Video)
                    OpenSubtitleCommand.Execute(file);
            }
        }

        /// <summary>
        /// Navigates to the Video Player with the request MRL as parameter
        /// </summary>
        /// <param name="mrl">The stream MRL to be played</param>
        /// <returns></returns>
        public async Task PlayStream(string streamMrl)
        {
            try
            {
                var stream = await Locator.MediaLibrary.LoadStreamFromDatabaseOrCreateOne(streamMrl);
                await Locator.MediaPlaybackViewModel.PlaybackService.SetPlaylist(new List<IMediaItem> { stream }, true, true, stream);
            }
            catch (Exception e)
            {
                LogHelper.Log(StringsHelper.ExceptionToString(e));
                return;
            }
        }

        /// <summary>
        /// Navigates to the Audio Player screen with the requested file a parameter.
        /// </summary>
        /// <param name="file">The file to be played.</param>
        /// <param name="token">Token is for files that are NOT in the sandbox, such as files taken from the filepicker from a sd card but not in the Video/Music folder.</param>
        public async Task PlayAudioFile(StorageFile file, string token = null)
        {
            var trackItem = await Locator.MediaLibrary.GetTrackItemFromFile(file, token);

            await Locator.MediaPlaybackViewModel.PlaybackService.SetPlaylist(new List<IMediaItem> { trackItem }, true, true, trackItem);
        }

        /// <summary>
        /// Navigates to the Video Player screen with the requested file a parameter.
        /// </summary>
        /// <param name="file">The file to be played.</param>
        /// <param name="token">Token is for files that are NOT in the sandbox, such as files taken from the filepicker from a sd card but not in the Video/Music folder.</param>
        public async Task PlayVideoFile(StorageFile file, string token = null)
        {
            var video = await MediaLibraryHelper.GetVideoItem(file);
            video.Id = -1;
            if (token != null)
                video.Token = token;

            await Locator.MediaPlaybackViewModel.PlaybackService.SetPlaylist(new List<IMediaItem> { video }, true, true, video);
        }

        public async Task UpdatePosition()
        {
            if (Locator.VideoPlayerVm.CurrentVideo != null)
            {
                Locator.VideoPlayerVm.CurrentVideo.TimeWatchedSeconds = (int)((double)Time / 1000); ;
                await Locator.MediaLibrary.UpdateVideo(Locator.VideoPlayerVm.CurrentVideo).ConfigureAwait(false);
            }
        }


        #endregion

        #region Events
        private async void Playback_StatusChanged(object sender, MediaState e)
        {
            try
            {
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () =>
                {
                    IsPlaying = e == MediaState.Playing || e == MediaState.Buffering;

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

        private async void Playback_MediaTimeChanged(long time)
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () =>
            {
                OnPropertyChanged(nameof(Time));
                // Assume position also changes when time does.
                // We could/should also watch OnPositionChanged event, but let's save us
                // the cost of another dispatched call.
                OnPropertyChanged(nameof(Position));
            });
        }

        private async void Playback_MediaLengthChanged(long length)
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (length < 0)
                    return;
                TimeTotal = TimeSpan.FromMilliseconds(length);
            });
        }

        private async void Playback_MediaStopped(IMediaService mediaService)
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                OnPropertyChanged(nameof(AudioTracks));
                OnPropertyChanged(nameof(Subtitles));
                OnPropertyChanged(nameof(CurrentAudioTrack));
                OnPropertyChanged(nameof(CurrentSubtitle));
            });
        }

        private async void Playback_MediaBuffering(int f)
        {
            _bufferingProgress = f;
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                OnPropertyChanged(nameof(BufferingProgress));
                OnPropertyChanged(nameof(IsBuffered));
            });
        }

        private async void Playback_MediaFailed(object sender, EventArgs e)
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                GoBack.Execute(null);
            });
        }

        private async void Playback_MediaEndReach()
        {
            switch (PlaybackService.PlayingType)
            {
                case PlayingType.Music:
                    break;
                case PlayingType.Video:
                    await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () =>
                    {
                        if (Locator.VideoPlayerVm.CurrentVideo != null)
                            Locator.VideoPlayerVm.CurrentVideo.TimeWatchedSeconds = 0;
                    });
                    if (Locator.VideoPlayerVm.CurrentVideo != null)
                        await Locator.MediaLibrary.UpdateVideo(Locator.VideoPlayerVm.CurrentVideo).ConfigureAwait(false);
                    break;
                case PlayingType.NotPlaying:
                    break;
                default:
                    break;
            }

            if (!PlaybackService.CanGoNext())
            {
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () =>
                {
                    if (PlaybackService.PlayingType == PlayingType.Video)
                    {
                        App.RootPage.StopCompositionAnimationOnSwapChain();
                    }
                    IsPlaying = false;
                    if (!Locator.NavigationService.GoBack_Default())
                    {
                        Locator.NavigationService.Go(Locator.SettingsVM.HomePage);
                    }
                });
            }
        }

        private async void Playback_MediaTracksUpdated(TrackType type, int trackId)
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                OnPropertyChanged(nameof(CurrentSubtitle));
                OnPropertyChanged(nameof(CurrentAudioTrack));

                if (type == TrackType.Video)
                {
                    if (PlaybackService.PlayingType == PlayingType.Video && Locator.NavigationService.CurrentPage != VLCPage.VideoPlayerPage)
                    {
                        Locator.NavigationService.Go(VLCPage.VideoPlayerPage);
                    }
                    Task.Run(async () =>
                    {
                        if (CurrentMedia is VideoItem)
                        {
                            var video = CurrentMedia as VideoItem;

                            var success = false;
                            if (video.IsSubtitlePreLoaded)
                                success = true;
                            else
                                success = await Locator.VideoMetaService.GetMovieSubtitle(video);

                            if (success)
                            {
                                var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(video.SubtitleUri));
                                OpenSubtitleCommand.Execute(file);
                            }
                        }
                    });
                }
            });
        }

        private async void Playback_MediaParsed(ParsedStatus parsedStatus)
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                OnPropertyChanged(nameof(Chapters));
                OnPropertyChanged(nameof(CurrentChapter));
            });
        }

        private async void PlaybackService_MediaSet(IMediaItem media)
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                OnPropertyChanged(nameof(CanGoPrevious));
                OnPropertyChanged(nameof(CanGoNext));
            });
        }
        #endregion

        #region MediaTransportControls

        public void SetMediaTransportControls(SystemMediaTransportControls systemMediaTransportControls)
        {
#if WINDOWS_PHONE_APP
            if (BackgroundAudioHelper.Instance?.CurrentState == MediaPlayerState.Playing)
            {

            }
            else
            {
                ForceMediaTransportControls(systemMediaTransportControls);
            }
#else
            ForceMediaTransportControls(systemMediaTransportControls);
#endif
        }

        void ForceMediaTransportControls(SystemMediaTransportControls systemMediaTransportControls)
        {
            try
            {
                _systemMediaTransportControls = systemMediaTransportControls;
                if (_systemMediaTransportControls != null)
                {
                    _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Closed;
                    _systemMediaTransportControls.ButtonPressed += SystemMediaTransportControlsOnButtonPressed;
                    _systemMediaTransportControls.IsEnabled = false;
                }
            }
            catch (Exception exception)
            { }
        }

        public async Task SetMediaTransportControlsInfo(string artistName, string albumName, string trackName, string albumUri)
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
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
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
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
                catch (Exception e)
                {
                    LogHelper.Log(StringsHelper.ExceptionToString(e));
                }
            });
        }

        private async void SystemMediaTransportControlsOnButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Pause:
                case SystemMediaTransportControlsButton.Play:
                    PlaybackService.Play();
                    break;
                case SystemMediaTransportControlsButton.Stop:
                    PlaybackService.Stop();
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    await PlaybackService.PlayPrevious();
                    break;
                case SystemMediaTransportControlsButton.Next:
                    await PlaybackService.PlayNext();
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

        public void Dispose()
        {
            PlaybackService.Stop();
        }
    }
}
