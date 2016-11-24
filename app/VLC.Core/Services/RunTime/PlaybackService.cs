/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.UI.Core;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SQLite;
using VLC.Commands.MusicPlayer;
using VLC.Model.Music;
using System.Collections.Generic;
using System.Linq;
using VLC.Helpers;
using VLC.Model;
using VLC.Model.Video;
using VLC.Utils;
using System.Diagnostics;
using VLC.Services.RunTime;
using VLC.Model.Stream;
using Windows.Storage;
using System.IO;
using libVLCX;
using VLC.ViewModels;
using VLC.Database;
using Windows.Media.Devices;

namespace VLC.Services.RunTime
{
    public class PlaybackService
    {
        public event Action<IMediaItem> Playback_MediaSet;
        public event Action<ParsedStatus> Playback_MediaParsed;
        public event Action<TrackType, int, string> Playback_MediaTracksAdded;
        public event Action<TrackType, int> Playback_MediaTracksDeleted;
        public event TimeChanged Playback_MediaTimeChanged;
        public event EncounteredError Playback_MediaFailed;
        public event Stopped Playback_MediaStopped;
        public event LengthChanged Playback_MediaLengthChanged;
        public event Action Playback_MediaEndReached;
        public event Buffering Playback_MediaBuffering;
        public event Action<IMediaItem> Playback_MediaFileNotFound;
        public event Playing Playback_MediaPlaying;
        public event Paused Playback_MediaPaused;
        public event Opening Playback_Opening;

        public event Action OnPlaylistEndReached;
        public event Action OnPlaylistChanged;
        
        private List<VLCChapterDescription> _chapters = new List<VLCChapterDescription>();       

        public TaskCompletionSource<bool> PlayerInstanceReady { get; set; } = new TaskCompletionSource<bool>();
        private DialogService _dialogService = new DialogService();
        private PlaylistService _playlistService;

        public Instance Instance { get; private set; }
        // Contains the IAudioClient address, as a string.
        private AudioDeviceHandler AudioClient { get; set; }
        private String _audioDeviceID;
        private String AudioDeviceID
        {
            get
            {
                if (_audioDeviceID == null)
                    _audioDeviceID = MediaDevice.GetDefaultAudioRenderId(AudioDeviceRole.Default);
                return _audioDeviceID;
            }
            set { _audioDeviceID = value; }
        }
        private MediaPlayer _mediaPlayer;
        public Media CurrentMedia { get; private set; }
        public IMediaItem CurrentPlaybackMedia { get; private set; }

        public bool IsPlaying { get { return _mediaPlayer != null ? _mediaPlayer.isPlaying() : false; } }
        public bool IsPaused {  get { return _mediaPlayer.state() == MediaState.Paused; } }
        public ObservableCollection<IMediaItem> Playlist {  get { return _playlistService.Playlist; } }

        public Task Initialize()
        {
            return DispatchHelper.InvokeAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                var param = new List<string>
                {
                    "-I",
                    "dummy",
                    "--no-osd",
                    "--verbose=3",
                    "--no-stats",
                    "--avcodec-fast",
                    string.Format("--freetype-font={0}\\NotoSans-Regular.ttf",Windows.ApplicationModel.Package.Current.InstalledLocation.Path),
                    "--subsdec-encoding",
                    Locator.SettingsVM.SubtitleEncodingValue == "System" ? "" : Locator.SettingsVM.SubtitleEncodingValue,
                    "--aout=winstore",
                    string.Format("--keystore-file={0}\\keystore", ApplicationData.Current.LocalFolder.Path),
                };

                // So far, this NEEDS to be called from the main thread
                try
                {
                    Instance = new Instance(param, App.RootPage.SwapChainPanel);
                    Instance?.setDialogHandlers(
                        async (title, text) => await _dialogService.ShowErrorDialog(title, text),
                        async (dialog, title, text, defaultUserName, askToStore) => await _dialogService.ShowLoginDialog(dialog, title, text, defaultUserName, askToStore),
                        async (dialog, title, text, qType, cancel, action1, action2) => await _dialogService.ShowQuestionDialog(dialog, title, text, qType, cancel, action1, action2),
                        (dialog, title, text, intermidiate, position, cancel) => { },
                        async (dialog) => await _dialogService.CancelCurrentDialog(),
                        (dialog, position, text) => { }
                    );

                    // Audio device management also needs to be called from the main thread
                    AudioClient = new AudioDeviceHandler(AudioDeviceID);
                    MediaDevice.DefaultAudioRenderDeviceChanged += onDefaultAudioRenderDeviceChanged;
                    PlayerInstanceReady.TrySetResult(Instance != null);
                }
                catch (Exception e)
                {
                    LogHelper.Log("VLC Service : Couldn't create VLC Instance\n" + StringsHelper.ExceptionToString(e));
                    ToastHelper.Basic(Strings.FailStartVLCEngine);
                }
            });
        }

        private async void onDefaultAudioRenderDeviceChanged(object sender, DefaultAudioRenderDeviceChangedEventArgs args)
        {
            if (args.Role != AudioDeviceRole.Default || args.Id == AudioDeviceID)
                return;

            AudioDeviceID = args.Id;
            // If we don't have an instance yet, no need to fetch the audio client as it will be done upon
            // instance creation.
            if (Instance == null)
                return;
            await DispatchHelper.InvokeAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                // Always fetch the new audio client, as we always assign it when starting a new playback
                AudioClient = new AudioDeviceHandler(AudioDeviceID);
                // But if a playback is in progress, inform VLC backend that we changed device
                if (_mediaPlayer != null)
                    _mediaPlayer.outputDeviceSet(AudioClient.audioClient());
            });
        }


        public PlayingType PlayingType { get; set; }

        public MediaState PlayerState { get; private set; }

        public bool IsRunning { get; set; }
        
        #region ctors
        public PlaybackService()
        {
            _playlistService = new PlaylistService(this);
            _playlistService.OnPlaylistEndReached += () => OnPlaylistEndReached?.Invoke();
            _playlistService.OnPlaylistChanged += () => OnPlaylistChanged?.Invoke();
            _playlistService.OnCurrentMediaChanged += onCurrentMediaChanged;
        }

        #endregion

        public void Trim()
        {
            Instance?.Trim();
        }
        #region Playlist setup

        public bool CanGoNext => _playlistService.CanGoNext;

        public bool CanGoPrevious => _playlistService.CanGoPrevious;

        public bool IsShuffled => _playlistService.IsShuffled;

        public Task RestorePlaylist()
        {
            return _playlistService.Restore();
        }
        private async void onCurrentMediaChanged(IMediaItem media)
        {
            await SetMedia(media);
            Play();
            Playback_MediaSet?.Invoke(media);
        }

        public void ShufflePlaylist()
        {
            _playlistService.Shuffle();
        }

        public void SetPlaylistIndex(int index)
        {
            if (index == _playlistService.Index)
                return;
            _playlistService.Index = index;
        }

        public async Task RemoveMedia(IMediaItem media)
        {
            bool currentRemoved = CurrentPlaybackMedia == media;
            _playlistService.RemoveMedia(media);
            if (currentRemoved)
                await SetMedia(_playlistService.CurrentMedia);
        }

        public Task AddToPlaylist(IEnumerable<IMediaItem> toAdd)
        {
            return _playlistService.AddToPlaylist(toAdd);
        }

        public Task SetPlaylist(IEnumerable<IMediaItem> mediaItems, uint startingIndex = 0)
        {
            return _playlistService.SetPlaylist(mediaItems, startingIndex);
        }

        public Task ClearPlaylist()
        {
            return _playlistService.Clear();
        }

        public bool Next()
        {
            return _playlistService.Next();
        }
        public bool Previous()
        {
            if (Position < 0.05)
                return _playlistService.Previous();
            Position = 0;
            return true;
        }

        #endregion

        #region Playback methods

        private async Task SetMedia(IMediaItem media)
        {
            if (media == null)
                throw new ArgumentNullException(nameof(media), "Media is missing. Can't play");

            if (media is VideoItem || media is TrackItem)
            {
                StorageFile currentFile;
                try
                {
                    currentFile = media.File ?? await StorageFile.GetFileFromPathAsync(media.Path);
                }
                catch (Exception exception)
                {
                    Playback_MediaFileNotFound?.Invoke(media);
                    return;
                }
            }

            await LogHelper.ResetBackendFile();

            // Send the media we want to play
            await SetMediaFile(media);
            
            if (_mediaPlayer == null) return;
            CurrentPlaybackMedia = media;
            var mem = _mediaPlayer.media().eventManager();
            mem.OnParsedChanged += OnParsedStatus;

            if (media is VideoItem)
            {
                var video = (VideoItem)media;

                var roamFile = await ApplicationData.Current.RoamingFolder.TryGetItemAsync("roamVideo.txt");
                if (roamFile != null)
                {
                    var roamVideos = await FileIO.ReadLinesAsync(roamFile as StorageFile);
                    if (roamVideos.Any())
                    {
                        if (roamVideos[0] == media.Name)
                        {
                            int leftTime = 0;
                            if (int.TryParse(roamVideos[1], out leftTime))
                            {
                                video.TimeWatchedSeconds = leftTime;
                            }
                        }
                    }
                }

                if (video.TimeWatched != TimeSpan.FromSeconds(0))
                {
                    Time = (long)video.TimeWatched.TotalMilliseconds;
                }

                TileHelper.UpdateVideoTile();
            }
        }

        private async Task SetMediaFile(IMediaItem media)
        {
            if (media.VlcMedia != null)
            {
                CurrentMedia = media.VlcMedia;
            }
            else
            {
                var mrl_fromType = media.GetMrlAndFromType();
                LogHelper.Log("SetMRL: " + mrl_fromType.Item2);
                if (Instance == null)
                {
                    await Initialize();
                }
                await PlayerInstanceReady.Task;

                if (!PlayerInstanceReady.Task.Result)
                {
                    LogHelper.Log($"Couldn't play media {media.Name} as VLC failed to init");
                    return;
                }

                CurrentMedia = new Media(Instance, mrl_fromType.Item2, mrl_fromType.Item1);
            }

            // Default to audio playback, and switch to video when a video track is encountered
            PlayingType = PlayingType.Music;

            // Hardware decoding
            CurrentMedia.addOption(!Locator.SettingsVM.HardwareAccelerationEnabled ? ":avcodec-hw=none" : ":avcodec-hw=d3d11va");
            CurrentMedia.addOption(!Locator.SettingsVM.HardwareAccelerationEnabled ? ":avcodec-threads=0" : ":avcodec-threads=1");

            if (_mediaPlayer == null)
            {
                _mediaPlayer = new MediaPlayer(CurrentMedia);
                var em = _mediaPlayer.eventManager();

                em.OnBuffering += Playback_MediaBuffering;
                em.OnStopped += OnStopped;
                em.OnPlaying += OnPlaying;
                em.OnPaused += OnPaused;
                em.OnTimeChanged += Playback_MediaTimeChanged;
                em.OnEndReached += OnEndReached;
                em.OnEncounteredError += Playback_MediaFailed;
                em.OnLengthChanged += Playback_MediaLengthChanged;
                em.OnTrackAdded += OnTrackAdded;
                em.OnTrackDeleted += OnTrackDeleted;
                em.OnPlaying += Playback_MediaPlaying;
                em.OnPaused += Playback_MediaPaused;
                em.OnOpening += Playback_Opening;
            }
            else
                _mediaPlayer.setMedia(CurrentMedia);
            _mediaPlayer.outputDeviceSet(AudioClient.audioClient());
            SetEqualizer(Locator.SettingsVM.Equalizer);
        }

        /// <summary>
        /// Only this method should set the CurrentMedia property of TrackCollection.
        /// </summary>
        /// <param name="index"></param>
        

        public int Volume
        {
            get { return _mediaPlayer.volume(); }
            set { _mediaPlayer.setVolume(value); }
        }

        public long AudioDelay
        {
            get { return _mediaPlayer.audioDelay(); }
            set { _mediaPlayer.setAudioDelay(value); }
        }

        public long SpuDelay
        {
            get { return _mediaPlayer.spuDelay(); }
            set { _mediaPlayer.setSpuDelay(value * 1000); }
        }

        public long Time
        {
            get { return _mediaPlayer.time(); }
            set { _mediaPlayer.setTime(value); }
        }

        public float Position
        {
            get { return _mediaPlayer.position(); }
            set { _mediaPlayer.setPosition(value); }
        }

        public void OpenSubtitleMrl(string mrl)
        {
            _mediaPlayer.addSlave(SlaveType.Subtitle, mrl, true);
        }

        public void SetSpeedRate(float rate)
        {
            _mediaPlayer.setRate(rate);
        }

        public VLCChapterDescription GetCurrentChapter()
        {
            var currentChapter = _mediaPlayer?.chapter();
            if (_chapters?.Count > 0 && currentChapter.HasValue)
            {
                return _chapters[currentChapter.Value];
            }
            return null;
        }

        public void SetCurrentChapter(VLCChapterDescription chapter)
        {
            if (chapter == GetCurrentChapter())
                return;

            var selectCh = _chapters.FirstOrDefault(x => x.Duration == chapter.Duration && x.Name == chapter.Name && x.StarTime == chapter.StarTime);
            if (selectCh == null)
                return;

            var index = _chapters.IndexOf(selectCh);
            if (index > -1)
            {
                _mediaPlayer.setChapter(index);
            }
        }

        public async Task<MediaProperties> GetVideoProperties(MediaProperties mP, Media media)
        {
            if (Instance == null)
            {
                await Initialize();
            }
            await PlayerInstanceReady.Task;
            if (media == null)
                return mP;
            if (media.parsedStatus() != ParsedStatus.Done)
            {
                var res = await media.parseWithOptionsAsync(ParseFlags.FetchLocal | ParseFlags.Local | ParseFlags.Network, 5000);
                if (res != ParsedStatus.Done)
                    return mP;
            }

            mP.Title = media.meta(MediaMeta.Title);

            var showName = media.meta(MediaMeta.ShowName);
            if (string.IsNullOrEmpty(showName))
            {
                showName = media.meta(MediaMeta.Artist);
            }
            if (!string.IsNullOrEmpty(showName))
            {
                mP.ShowTitle = showName;
            }

            var episodeString = media.meta(MediaMeta.Episode);
            if (string.IsNullOrEmpty(episodeString))
            {
                episodeString = media.meta(MediaMeta.TrackNumber);
            }
            var episode = 0;
            if (!string.IsNullOrEmpty(episodeString) && int.TryParse(episodeString, out episode))
            {
                mP.Episode = episode;
            }

            var episodesTotal = 0;
            var episodesTotalString = media.meta(MediaMeta.TrackTotal);
            if (!string.IsNullOrEmpty(episodesTotalString) && int.TryParse(episodesTotalString, out episodesTotal))
            {
                mP.Episodes = episodesTotal;
            }

            var videoTrack = media.tracks().FirstOrDefault(x => x.type() == TrackType.Video);
            if (videoTrack != null)
            {
                mP.Width = videoTrack.width();
                mP.Height = videoTrack.height();
            }

            var durationLong = media.duration();
            var duration = TimeSpan.FromMilliseconds(durationLong);
            mP.Duration = duration;

            return mP;
        }

        public async Task<MediaProperties> GetMusicProperties(Media media)
        {
            if (Instance == null)
            {
                await Initialize();
            }
            await PlayerInstanceReady.Task;
            if (media == null)
                return null;
            if (media.parsedStatus() != ParsedStatus.Done)
            {
                var res = await media.parseWithOptionsAsync(ParseFlags.FetchLocal | ParseFlags.Local | ParseFlags.Network, 5000);
                if (res != ParsedStatus.Done)
                    return null;
            }

            var mP = new MediaProperties();
            mP.AlbumArtist = media.meta(MediaMeta.AlbumArtist);
            mP.Artist = media.meta(MediaMeta.Artist);
            mP.Album = media.meta(MediaMeta.Album);
            mP.Title = media.meta(MediaMeta.Title);
            mP.AlbumArt = media.meta(MediaMeta.ArtworkURL);
            var yearString = media.meta(MediaMeta.Date);
            var year = 0;
            if (int.TryParse(yearString, out year))
            {
                mP.Year = year;
            }

            var durationLong = media.duration();
            TimeSpan duration = TimeSpan.FromMilliseconds(durationLong);
            mP.Duration = duration;

            var trackNbString = media.meta(MediaMeta.TrackNumber);
            uint trackNbInt = 0;
            uint.TryParse(trackNbString, out trackNbInt);
            mP.Tracknumber = trackNbInt;

            var discNb = media.meta(MediaMeta.DiscNumber);
            if (discNb.Contains("/"))
            {
                // if discNb = "1/2"
                var discNumDen = discNb.Split('/');
                if (discNumDen.Any())
                    discNb = discNumDen[0];
            }
            int discNbInt = 1;
            int.TryParse(discNb, out discNbInt);
            mP.DiscNumber = discNbInt;

            var genre = media.meta(MediaMeta.Genre);
            mP.Genre = genre;

            return mP;
        }

        public async Task<Media> GetMediaFromPath(string filePath)
        {
            if (Instance == null)
            {
                await Initialize();
            }
            await PlayerInstanceReady.Task;
            if (string.IsNullOrEmpty(filePath))
                return null;
            return new Media(Instance, filePath, FromType.FromPath);
        }

        public List<VLCChapterDescription> GetChapters()
        {
            _chapters.Clear();
            var chapters = _mediaPlayer?.chapterDescription(-1);
            foreach (var c in chapters)
            {
                var vlcChapter = new VLCChapterDescription(c);
                _chapters.Add(vlcChapter);
            }
            return _chapters.ToList();
        }

        public int CurrentAudioTrack
        {
            get { return _mediaPlayer.audioTrack(); }
            set { _mediaPlayer.setAudioTrack(value); }
        }

        public int CurrentSubtitleTrack
        {
            get { return _mediaPlayer.spu(); }
            set { _mediaPlayer.setSpu(value); }
        }

        public void Stop()
        {
            if (PlayerState != MediaState.Ended && PlayerState != MediaState.NothingSpecial)
            {
                _mediaPlayer.stop();
            }
            TileHelper.ClearTile();
        }

        public void Pause()
        {
            _mediaPlayer?.pause();
        }

        public void Play()
        {
            _mediaPlayer?.play();
        }

        public void SetSizeVideoPlayer(uint x, uint y)
        {
            Instance?.UpdateSize(x, y);
        }

        public void SetEqualizer(VLCEqualizer vlcEq)
        {
            var eq = new Equalizer(vlcEq.Index);
            _mediaPlayer?.setEqualizer(eq);
        }

        public IList<VLCEqualizer> GetEqualizerPresets()
        {
            var presetCount = Equalizer.presetCount();
            var presets = new List<VLCEqualizer>();
            for (uint i = 0; i < presetCount; i++)
            {
                presets.Add(new VLCEqualizer(i));
            }
            return presets;
        }

        #endregion

        #region Playback events callbacks

        private void OnParsedStatus(ParsedStatus parsedStatus)
        {
            if (parsedStatus != ParsedStatus.Done)
                return;
            
            // Get chapters
            GetChapters();

            Playback_MediaParsed?.Invoke(parsedStatus);
        }

        private void OnTrackAdded(TrackType type, int trackId)
        {
            if (type == TrackType.Unknown)
                return;

            if (type == TrackType.Video)
                PlayingType = PlayingType.Video;

            IList<TrackDescription> tracks;
            if (type == TrackType.Audio)
                tracks = _mediaPlayer.audioTrackDescription();
            else if (type == TrackType.Subtitle)
                tracks = _mediaPlayer.spuDescription();
            else
                tracks = _mediaPlayer.videoTrackDescription();

            foreach (var t in tracks)
            {
                if (t.id() == trackId)
                {
                    Playback_MediaTracksAdded?.Invoke(type, trackId, t.name());
                    return;
                }
            }
            LogHelper.Log("Failed to find track description");
            Playback_MediaTracksAdded(type, trackId, "Unknown track");
        }

        private void OnTrackDeleted(TrackType trackType, int trackId)
        {
            Playback_MediaTracksDeleted?.Invoke(trackType, trackId);
        }

        private void OnEndReached()
        {
            Playback_MediaEndReached?.Invoke();
            TileHelper.ClearTile();
            PlayerState = MediaState.Ended;
        }

        private void OnStopped()
        {
            CurrentPlaybackMedia = null;
            PlayerState = MediaState.Stopped;
            PlayingType = PlayingType.NotPlaying;
            Playback_MediaStopped?.Invoke();
        }

        private void OnPaused()
        {
            PlayerStateChanged(this, MediaState.Paused);
        }

        private void OnPlaying()
        {
            PlayerStateChanged(this, MediaState.Playing);
        }

        private void PlayerStateChanged(object sender, MediaState e)
        {
            PlayerState = e;
        }
        
        #endregion
    }
}
