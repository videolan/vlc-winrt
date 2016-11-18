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
using VLC.Helpers;
using VLC.Model;
using VLC.Commands.MediaPlayback;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Autofac;
using VLC.Services.RunTime;
using VLC.ViewModels.MusicVM;
using libVLCX;
using VLC.Utils;
using VLC.Commands.VideoPlayer;
using VLC.Commands.VideoLibrary;
using System.Linq;
using VLC.Commands.MusicPlayer;
using VLC.MediaMetaFetcher.Fetchers;
using VLC.Model.Video;
using VLC.Model.Stream;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using System.Collections.ObjectModel;

namespace VLC.ViewModels
{
    public sealed class MediaPlaybackViewModel : BindableBase
    {
        #region private props
        private MouseService _mouseService;
        private SystemMediaTransportControls _systemMediaTransportControls;
        private TimeSpan _timeTotal;

        private int _volume = 100;
        private int _speedRate = 100;
        private int _bufferingProgress;
        private Visibility _loadingMedia = Visibility.Collapsed;
        #endregion

        public PlaybackService PlaybackService { get { return Locator.PlaybackService; } }
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

        public FastSeekCommand FastSeekCommand { get; } = new FastSeekCommand();
        #endregion

        #region public props

        public IMediaItem CurrentMedia
        {
            get
            {
                if (PlaybackService.CurrentPlaylistIndex == -1)
                    return null;
                if (PlaybackService.Playlist.Count == 0)
                    return null;
                return PlaybackService.Playlist[PlaybackService.CurrentPlaylistIndex];
            }
        }

        public bool IsPlaying
        {
            get { return PlaybackService.IsPlaying && !PlaybackService.IsPaused; }
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
            get { return PlaybackService.AudioDelay; }
            set
            {
                PlaybackService.AudioDelay = value;
                OnPropertyChanged(nameof(AudioDelay));
            }
        }


        /// <summary>
        /// Gets and Sets the SpuDelay in MilliSeconds
        /// Warning : VLC API needs microseconds, hence the 1000 multiplication
        /// </summary>
        public long SpuDelay
        {
            get { return PlaybackService.SpuDelay; }
            set
            {
                PlaybackService.SpuDelay = value;
                OnPropertyChanged(nameof(SpuDelay));
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
            get { return PlaybackService.Time; }
            set { PlaybackService.Time = value; }
        }
        public float Position
        {
            get { return PlaybackService.Position; }
            set { PlaybackService.Position = value; }
        }


        private DictionaryKeyValue _currentSubtitle;
        public DictionaryKeyValue CurrentSubtitle
        {
            get { return _currentSubtitle; }
            set
            {
                if (value == null)
                    return;
                _currentSubtitle = value;
                PlaybackService.CurrentSubtitleTrack = value.Id;
            }
        }

        private DictionaryKeyValue _currentAudioTrack;
        public DictionaryKeyValue CurrentAudioTrack
        {
            get { return _currentAudioTrack; }
            set
            {
                if (value == null)
                    return;
                _currentAudioTrack = value;
                PlaybackService.CurrentAudioTrack = value.Id;
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
                if (value != null)
                    PlaybackService.SetCurrentChapter(value);
            }
        }
        #endregion

        #region public fields
        public ObservableCollection<DictionaryKeyValue> AudioTracks { get; } = new ObservableCollection<DictionaryKeyValue>();

        public ObservableCollection<DictionaryKeyValue> Subtitles { get; } = new ObservableCollection<DictionaryKeyValue>();

        public List<VLCChapterDescription> Chapters => PlaybackService.GetChapters();

        public Visibility LoadingMedia { get { return _loadingMedia; } set { SetProperty(ref _loadingMedia, value); } }
        #endregion

        #region constructors
        public MediaPlaybackViewModel()
        {
            _mouseService = App.Container.Resolve<MouseService>();
            PlaybackService.Playback_MediaPlaying += OnPlaying;
            PlaybackService.Playback_MediaPaused += OnPaused;
            PlaybackService.Playback_MediaTimeChanged += Playback_MediaTimeChanged;
            PlaybackService.Playback_MediaLengthChanged += Playback_MediaLengthChanged;
            PlaybackService.Playback_MediaBuffering += Playback_MediaBuffering;
            PlaybackService.Playback_MediaEndReached += Playback_MediaEndReach;
            PlaybackService.Playback_MediaFailed += Playback_MediaFailed;
            PlaybackService.Playback_MediaStopped += Playback_MediaStopped;
            PlaybackService.Playback_MediaTracksAdded += OnMediaTrackAdded;
            PlaybackService.Playback_MediaTracksDeleted += OnMediaTrackDeleted;
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
            Uri uri;
            try
            {
                uri = new Uri(streamMrl);
            }
            catch (UriFormatException ex)
            {
                var md = new MessageDialog(string.Format("{0} is invalid ({1})", streamMrl, ex.Message), "Invalid URI");
                await md.ShowAsync();
                return;
            }
            try
            {
                var stream = await Locator.MediaLibrary.LoadStreamFromDatabaseOrCreateOne(uri.ToString());
                LoadingMedia = Visibility.Visible;
                await PlaybackService.SetPlaylist(new List<IMediaItem> { stream }, true, true, stream);
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

            await PlaybackService.SetPlaylist(new List<IMediaItem> { trackItem }, true, true, trackItem);
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

            await PlaybackService.SetPlaylist(new List<IMediaItem> { video }, true, true, video);
        }

        public async Task UpdatePosition()
        {
            if (Locator.VideoPlayerVm.CurrentVideo != null)
            {
                Locator.VideoPlayerVm.CurrentVideo.TimeWatchedSeconds = (int)((double)Time / 1000); ;
                Locator.MediaLibrary.UpdateVideo(Locator.VideoPlayerVm.CurrentVideo);

                var file = await ApplicationData.Current.RoamingFolder.CreateFileAsync("roamVideo.txt", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteLinesAsync(file, new string[]
                {
                    Locator.VideoPlayerVm.CurrentVideo.Name,
                    Locator.VideoPlayerVm.CurrentVideo.TimeWatchedSeconds.ToString()
                });
            }
        }


        #endregion

        #region Events
        private async void OnPlaying()
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () =>
            {
                LoadingMedia = Visibility.Collapsed;
                OnPropertyChanged(nameof(IsPlaying));
            });
            if (_systemMediaTransportControls != null)
                _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Playing;
        }
        private async void OnPaused()
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () =>
            {
                OnPropertyChanged(nameof(IsPlaying));
            });
            if (_systemMediaTransportControls != null)
                _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Paused;
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

        private async void Playback_MediaStopped()
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, async () =>
            {
                AudioTracks.Clear();
                Subtitles.Clear();
                OnPropertyChanged(nameof(AudioTracks));
                OnPropertyChanged(nameof(Subtitles));
                OnPropertyChanged(nameof(CurrentAudioTrack));
                OnPropertyChanged(nameof(CurrentSubtitle));
                OnPropertyChanged(nameof(IsPlaying));
                await ClearMediaTransportControls();
            });
        }

        private async void Playback_MediaBuffering(float f)
        {
            _bufferingProgress = (int)f;
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                OnPropertyChanged(nameof(BufferingProgress));
                OnPropertyChanged(nameof(IsBuffered));
            });
        }

        private async void Playback_MediaFailed()
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, async () =>
            {
                if (CurrentMedia is StreamMedia)
                {
                    await Locator.MediaLibrary.RemoveStreamFromCollectionAndDatabase(CurrentMedia as StreamMedia);
                }
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
                        Locator.MediaLibrary.UpdateVideo(Locator.VideoPlayerVm.CurrentVideo);
                    break;
                case PlayingType.NotPlaying:
                    break;
                default:
                    break;
            }

            if (!PlaybackService.CanGoNext() && !PlaybackService.Repeat)
            {
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () =>
                {
                    if (PlaybackService.PlayingType == PlayingType.Video)
                    {
                        App.RootPage.StopCompositionAnimationOnSwapChain();
                    }
                    if (!Locator.NavigationService.GoBack_Default())
                    {
                        Locator.NavigationService.Go(Locator.SettingsVM.HomePage);
                    }
                });
            }
        }
        
        private async void OnMediaTrackAdded(TrackType type, int trackId, string name)
        {
            var item = new DictionaryKeyValue
            {
                Id = trackId,
                Name = name
            };
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (type == TrackType.Audio)
                {
                    if (AudioTracks.Count == 0)
                        AudioTracks.Add(new DictionaryKeyValue { Id = -1, Name = "Disabled" });
                    AudioTracks.Add(item);
                    if (_currentAudioTrack == null)
                        _currentAudioTrack = item;
                    OnPropertyChanged(nameof(AudioTracks));
                    OnPropertyChanged(nameof(CurrentAudioTrack));
                }
                else if (type == TrackType.Subtitle)
                {
                    if (Subtitles.Count == 0)
                        Subtitles.Add(new DictionaryKeyValue { Id = -1, Name = "Disabled" });
                    Subtitles.Add(item);
                    if (_currentSubtitle == null)
                        _currentSubtitle = item;
                    OnPropertyChanged(nameof(Subtitles));
                    OnPropertyChanged(nameof(CurrentSubtitle));
                }
                else
                {
                    Locator.NavigationService.Go(VLCPage.VideoPlayerPage);
                }
            });
        }

        private async void OnMediaTrackDeleted(TrackType type, int trackId)
        {
            if (type == TrackType.Video)
                return;
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                ObservableCollection<DictionaryKeyValue> target;
                if (type == TrackType.Audio)
                {
                    target = AudioTracks;
                    OnPropertyChanged(nameof(CurrentAudioTrack));
                }
                else
                {
                    target = Subtitles;
                    OnPropertyChanged(nameof(CurrentSubtitle));
                }
                var res = target.FirstOrDefault((item) => item.Id == trackId);
                if (res != null)
                    target.Remove(res);
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
            _systemMediaTransportControls = systemMediaTransportControls;
            if (_systemMediaTransportControls != null)
            {
                _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Closed;
                _systemMediaTransportControls.ButtonPressed += SystemMediaTransportControlsOnButtonPressed;
                _systemMediaTransportControls.IsEnabled = true;
            }
        }

        private SystemMediaTransportControlsDisplayUpdater CommonTransportControlInit()
        {
            _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Playing;
            _systemMediaTransportControls.IsEnabled = true;
            _systemMediaTransportControls.IsPauseEnabled = true;
            _systemMediaTransportControls.IsPlayEnabled = true;
            _systemMediaTransportControls.PlaybackRate = 1;
            _systemMediaTransportControls.IsStopEnabled = true;
            _systemMediaTransportControls.IsFastForwardEnabled = true;
            _systemMediaTransportControls.IsRewindEnabled = true;

            var updater = _systemMediaTransportControls.DisplayUpdater;
            updater.ClearAll();
            updater.AppMediaId = "VLC Media Player";
            return updater;
        }

        public async Task SetMediaTransportControlsInfo(string artistName, string albumName, string trackName, string albumUri)
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                var updater = CommonTransportControlInit();
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
            });
        }

        public async Task SetMediaTransportControlsInfo(string title)
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                LogHelper.Log("PLAYVIDEO: Updating SystemMediaTransportControls");
                var updater = CommonTransportControlInit();
                updater.Type = MediaPlaybackType.Video;
                //Video metadata
                updater.VideoProperties.Title = title;
                //TODO: add full thumbnail suport
                updater.Thumbnail = null;
                updater.Update();
            });
        }

        public async Task ClearMediaTransportControls()
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (_systemMediaTransportControls == null) return;
                LogHelper.Log("PLAYVIDEO: Updating SystemMediaTransportControls");
                SystemMediaTransportControlsDisplayUpdater updater = _systemMediaTransportControls.DisplayUpdater;
                updater.ClearAll();
            });
        }

        private async void SystemMediaTransportControlsOnButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Pause:
                    PlaybackService.Pause();
                    break;
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
                case SystemMediaTransportControlsButton.FastForward:
                    FastSeekCommand.Execute(30000);
                    break;
                case SystemMediaTransportControlsButton.Rewind:
                    FastSeekCommand.Execute(-30000);
                    break;
            }
        }

        public void SystemMediaTransportControlsBackPossible(bool backPossible)
        {
            if (_systemMediaTransportControls != null)
                _systemMediaTransportControls.IsPreviousEnabled = backPossible;
        }

        public void SystemMediaTransportControlsNextPossible(bool nextPossible)
        {
            if (_systemMediaTransportControls != null)
                _systemMediaTransportControls.IsNextEnabled = nextPossible;
        }
        #endregion
    }
}
