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
        public event EventHandler<MediaState> Playback_StatusChanged;
        public event Action<IMediaItem> Playback_MediaSet;
        public event Action<ParsedStatus> Playback_MediaParsed;
        public event Action<TrackType, int> Playback_MediaTracksUpdated;
        public event TimeChanged Playback_MediaTimeChanged;
        public event EncounteredError Playback_MediaFailed;
        public event Stopped Playback_MediaStopped;
        public event LengthChanged Playback_MediaLengthChanged;
        public event Action Playback_MediaEndReached;
        public event Buffering Playback_MediaBuffering;
        public event Action<IMediaItem> Playback_MediaFileNotFound;

        private List<DictionaryKeyValue> _subtitlesTracks = new List<DictionaryKeyValue>();
        private List<DictionaryKeyValue> _audioTracks = new List<DictionaryKeyValue>();
        private List<VLCChapterDescription> _chapters = new List<VLCChapterDescription>();
        private int _currentSubtitle;
        private int _currentAudioTrack;

        private SmartCollection<IMediaItem> _playlist;
        private SmartCollection<IMediaItem> _nonShuffledPlaylist;
        private bool _repeat;

        public BackgroundTrackDatabase BackgroundTrackRepository { get; set; } = new BackgroundTrackDatabase();
        public TaskCompletionSource<bool> PlayerInstanceReady { get; set; } = new TaskCompletionSource<bool>();
        private DialogService _dialogService = new DialogService();

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
        public MediaPlayer MediaPlayer { get; private set; }

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
                if (MediaPlayer != null)
                    MediaPlayer.outputDeviceSet(AudioClient.audioClient());
            });
        }

        public int CurrentMedia { get; private set; }

        public PlayingType PlayingType { get; set; }

        public bool Repeat
        {
            get { return _repeat; }
            set
            {
                _repeat = value;
            }
        }

        public MediaState PlayerState { get; private set; }

        public bool CanGoPrevious()
        {
            var previous = (CurrentMedia > 0);
            Locator.MediaPlaybackViewModel.SystemMediaTransportControlsBackPossible(previous);
            return previous;
        }

        public bool CanGoNext()
        {
            var next = (Playlist.Count != 1) && (CurrentMedia < Playlist.Count - 1);
            Locator.MediaPlaybackViewModel.SystemMediaTransportControlsNextPossible(next);
            Debug.WriteLine("Can go next:" + next);
            return next;
        }

        public bool IsRunning { get; set; }

        public bool IsShuffled { get; set; }

        #region public fields
        public SmartCollection<IMediaItem> Playlist
        {
            get { return _playlist; }
            private set { _playlist = value; }
        }

        private SmartCollection<IMediaItem> NonShuffledPlaylist
        {
            get { return _nonShuffledPlaylist; }
            set { _nonShuffledPlaylist = value; }
        }

        #endregion

        #region ctors
        public PlaybackService()
        {
            Task.Run(() => RestorePlaylist());
            _playlist = new SmartCollection<IMediaItem>();
            InitializePlaylist();
        }
        #endregion

        #region Playlist setup
        public void InitializePlaylist()
        {
            Playlist.Clear();
            CurrentMedia = -1;
        }

        public async Task ResetCollection()
        {
            await BackgroundTrackRepository.Clear();
            Playlist.Clear();
            CurrentMedia = -1;
            NonShuffledPlaylist?.Clear();
            IsShuffled = false;
            Playback_MediaSet(null);
            IsRunning = false;
        }

        public void Shuffle()
        {
            if (IsShuffled)
            {
                NonShuffledPlaylist = new SmartCollection<IMediaItem>(Playlist);
                Random r = new Random();
                for (int i = 0; i < Playlist.Count; i++)
                {
                    if (i > CurrentMedia)
                    {
                        int index1 = r.Next(i, Playlist.Count);
                        int index2 = r.Next(i, Playlist.Count);
                        Playlist.Move(index1, index2);
                    }
                }
                var pl = Playlist.ToList<IMediaItem>();
            }
            else
            {
                IMediaItem m = Playlist[CurrentMedia];

                Playlist.Clear();
                Playlist.AddRange(NonShuffledPlaylist);

                CurrentMedia = Playlist.IndexOf(m);
            }
        }

        public void Trim()
        {
            Instance?.Trim();
        }

        public async Task<bool> SetPlaylist(IEnumerable<IMediaItem> mediaItems, bool reset, bool play, IMediaItem media)
        {
            if (reset)
            {
                await ResetCollection();
            }

            if (mediaItems != null)
            {
                var count = (uint)Playlist.Count;
                var trackItems = mediaItems.OfType<TrackItem>();
                var backgroundTrackItems = new List<BackgroundTrackItem>();
                foreach (var track in trackItems)
                {
                    backgroundTrackItems.Add(new BackgroundTrackItem()
                    {
                        TrackId = track.Id
                    });
                    track.Index = count;
                    count++;
                }

                Playlist.AddRange(mediaItems);

                await BackgroundTrackRepository.Add(backgroundTrackItems);

                IsRunning = true;
            }

            if (media != null && Playlist.Any())
            {
                var mediaInPlaylist = Playlist.FirstOrDefault(x => x.Path == media.Path);

                if (mediaInPlaylist == null)
                    return false;

                var mediaIndex = Playlist.IndexOf(mediaInPlaylist);

                if (mediaIndex < 0)
                    return false;

                SetCurrentMediaPosition(mediaIndex);
                Playback_MediaSet.Invoke(mediaInPlaylist);
                await SetMedia(mediaInPlaylist, play);
            }
            return true;
        }

        private async Task RestorePlaylist()
        {
            try
            {
                var playlist = await BackgroundTrackRepository.LoadPlaylist();
                if (!playlist.Any())
                {
                    return;
                }

                var trackIds = playlist.Select(node => node.TrackId);
                var restoredplaylist = new SmartCollection<IMediaItem>();
                foreach (int trackId in trackIds)
                {
                    var trackItem = Locator.MediaLibrary.LoadTrackById(trackId);
                    if (trackItem != null)
                        restoredplaylist.Add(trackItem);
                }

                if (!ApplicationSettingsHelper.Contains(nameof(CurrentMedia)))
                    return;
                var index = (int)ApplicationSettingsHelper.ReadSettingsValue(nameof(CurrentMedia));
                if (restoredplaylist.Any())
                {
                    if (index == -1)
                    {
                        // Background Audio was terminated
                        // We need to reset the playlist, or set the current track 0.
                        ApplicationSettingsHelper.SaveSettingsValue(nameof(CurrentMedia), 0);
                        index = 0;
                    }
                    SetCurrentMediaPosition(index);
                }

                if (CurrentMedia >= restoredplaylist.Count || CurrentMedia == -1)
                    CurrentMedia = 0;

                if (restoredplaylist.Any())
                    await SetPlaylist(restoredplaylist, true, false, restoredplaylist[CurrentMedia]);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to restore the playlist");
            }
        }

        private async Task SetMedia(IMediaItem media, bool autoPlay = true)
        {
            if (media == null)
                throw new ArgumentNullException(nameof(media), "Media is missing. Can't play");

            if (Playlist.ElementAt(CurrentMedia) != null)
                Stop();

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

            if (media is VideoItem)
            {
                var video = (VideoItem)media;
                await InitializePlayback(video, autoPlay);

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
                    SetTime((long)video.TimeWatched.TotalMilliseconds);
                }

                TileHelper.UpdateVideoTile();
            }
            else if (media is TrackItem)
            {
                var track = (TrackItem)media;
                await InitializePlayback(track, autoPlay);

                int index = IsShuffled ?
                    NonShuffledPlaylist.IndexOf(Playlist[CurrentMedia]): CurrentMedia;
                ApplicationSettingsHelper.SaveSettingsValue(nameof(CurrentMedia), index);
            }
            else if (media is StreamMedia)
            {
                await InitializePlayback(media, autoPlay);
            }
        }

        private async Task SetMediaFile(IMediaItem media)
        {
            Media vlcMedia = null;
            if (media.VlcMedia != null)
            {
                vlcMedia = media.VlcMedia;
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

                vlcMedia = new Media(Instance, mrl_fromType.Item2, mrl_fromType.Item1);
            }

            // Hardware decoding
            vlcMedia.addOption(!Locator.SettingsVM.HardwareAccelerationEnabled ? ":avcodec-hw=none" : ":avcodec-hw=d3d11va");
            vlcMedia.addOption(!Locator.SettingsVM.HardwareAccelerationEnabled ? ":avcodec-threads=0" : ":avcodec-threads=1");

            MediaPlayer = new MediaPlayer(vlcMedia);
            LogHelper.Log("PLAYWITHVLC: MediaPlayer instance created");
            MediaPlayer.outputDeviceSet(AudioClient.audioClient());
            SetEqualizer(Locator.SettingsVM.Equalizer);
            var em = MediaPlayer.eventManager();

            em.OnOpening += OnOpening;
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
        }

        private async Task InitializePlayback(IMediaItem media, bool autoPlay)
        {
            // First set the player engine
            // For videos AND music, we have to try first with Microsoft own player
            // Then we register to Failed callback. If it doesn't work, we set ForceVlcLib to true
            if (autoPlay)
            {
                // Reset the libVLC log file
                await LogHelper.ResetBackendFile();
            }

            // Send the media we want to play
            await SetMediaFile(media);


            if (MediaPlayer == null) return;
            var mem = MediaPlayer.media().eventManager();
            mem.OnParsedChanged += OnParsedStatus;
            if (!autoPlay)
                return;
            Play();

            MediaPlayer.setRate(1);
        }

        #endregion

        #region Playback methods
        /// <summary>
        /// Only this method should set the CurrentMedia property of TrackCollection.
        /// </summary>
        /// <param name="index"></param>
        public void SetCurrentMediaPosition(int index)
        {
            CurrentMedia = index;
        }


        public async Task StartAgain()
        {
            SetCurrentMediaPosition(0);
            await SetPlaylist(null, false, true, Playlist[CurrentMedia]);
        }

        public async Task PlayNext()
        {
            if (CanGoNext())
            {
                SetCurrentMediaPosition(CurrentMedia + 1);
                await SetPlaylist(null, false, true, Playlist[CurrentMedia]);
            }
        }

        public async Task PlayPrevious()
        {
            if (CanGoPrevious())
            {
                SetCurrentMediaPosition(CurrentMedia - 1);
                await SetPlaylist(null, false, true, Playlist[CurrentMedia]);
            }
        }

        public int Volume
        {
            get { return MediaPlayer.volume(); }
            set { MediaPlayer.setVolume(value); }
        }

        public void SetSubtitleTrack(int i)
        {
            MediaPlayer.setSpu(i);
        }

        public void SetAudioTrack(int i)
        {
            MediaPlayer.setAudioTrack(i);
        }

        public void SetAudioDelay(long delay)
        {
            MediaPlayer.setAudioDelay(delay * 1000);
        }

        public void SetSpuDelay(long delay)
        {
            MediaPlayer.setSpuDelay(delay * 1000);
        }

        public void SetTime(long time)
        {
            MediaPlayer.setTime(time);
        }

        public long GetTime()
        {
            return MediaPlayer.time();
        }

        public void SetPosition(float pos)
        {
            MediaPlayer.setPosition(pos);
        }

        public float GetPosition()
        {
            return MediaPlayer.position();
        }

        public void OpenSubtitleMrl(string mrl)
        {
            MediaPlayer.addSlave(SlaveType.Subtitle, mrl, true);
        }

        public void SetSpeedRate(float rate)
        {
            MediaPlayer.setRate(rate);
        }

        public VLCChapterDescription GetCurrentChapter()
        {
            var currentChapter = MediaPlayer?.chapter();
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
                MediaPlayer.setChapter(index);
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
            var chapters = MediaPlayer?.chapterDescription(-1);
            foreach (var c in chapters)
            {
                var vlcChapter = new VLCChapterDescription(c);
                _chapters.Add(vlcChapter);
            }
            return _chapters.ToList();
        }

        public DictionaryKeyValue GetCurrentAudioTrack()
        {
            if (_currentAudioTrack == -1 || _currentAudioTrack >= _audioTracks.Count)
                return null;

            return _audioTracks[_currentAudioTrack];
        }

        public void SetAudioTrack(DictionaryKeyValue audioTrack)
        {
            _currentAudioTrack = _audioTracks.IndexOf(audioTrack);
            if (audioTrack != null)
            {
                MediaPlayer.setAudioTrack(audioTrack.Id);
            }
        }

        public List<DictionaryKeyValue> GetAudioTracks()
        {
            return _audioTracks.ToList();
        }

        public DictionaryKeyValue GetCurrentSubtitleTrack()
        {
            if (_currentSubtitle < 0 || _currentSubtitle >= _subtitlesTracks.Count)
                return null;
            return _subtitlesTracks[_currentSubtitle];
        }

        public void SetSubtitleTrack(DictionaryKeyValue subTrack)
        {
            _currentSubtitle = _subtitlesTracks.IndexOf(subTrack);
            if (subTrack != null)
            {
                MediaPlayer.setSpu(subTrack.Id);
            }
        }

        public List<DictionaryKeyValue> GetSubtitleTracks()
        {
            return _subtitlesTracks.ToList();
        }

        public void Stop()
        {
            _currentAudioTrack = -1;
            _currentSubtitle = -1;

            _audioTracks.Clear();
            _subtitlesTracks.Clear();

            if (PlayerState != MediaState.Ended && PlayerState != MediaState.NothingSpecial)
            {
                MediaPlayer.stop();
            }
            TileHelper.ClearTile();
        }

        public void Pause()
        {
            MediaPlayer?.pause();
        }

        public void Play()
        {
            MediaPlayer?.play();
        }

        public void SetSizeVideoPlayer(uint x, uint y)
        {
            Instance?.UpdateSize(x, y);
        }

        void SetPlaybackTypeFromTracks()
        {
            var videoTrack = MediaPlayer.media().tracks().FirstOrDefault(x => x.type() == TrackType.Video);

            if (videoTrack == null)
            {
                PlayingType = PlayingType.Music;
            }
            else
            {
                PlayingType = PlayingType.Video;
            }
        }

        public void SetEqualizer(VLCEqualizer vlcEq)
        {
            var eq = new Equalizer(vlcEq.Index);
            MediaPlayer?.setEqualizer(eq);
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

            // Get subtitle delay etc
            if (MediaPlayer != null)
            {
                SetAudioDelay(MediaPlayer.audioDelay());
                SetSpuDelay(MediaPlayer.spuDelay());
            }

            if (MediaPlayer == null)
                return;

            SetPlaybackTypeFromTracks();

            Playback_MediaParsed?.Invoke(parsedStatus);
        }

        private void OnTrackDeleted(TrackType type, int trackId)
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
            }
            else
            {
                _currentAudioTrack = -1;
            }

            Playback_MediaTracksUpdated?.Invoke(type, trackId);
        }

        private void OnTrackAdded(TrackType type, int trackId)
        {
            if (type == TrackType.Unknown)
                return;

            if (type == TrackType.Video)
            {
                PlayingType = PlayingType.Video;
            }

            List<DictionaryKeyValue> target;
            IList<TrackDescription> source;
            if (type == TrackType.Audio)
            {
                target = _audioTracks;
                source = MediaPlayer?.audioTrackDescription();
            }
            else
            {
                target = _subtitlesTracks;
                source = MediaPlayer?.spuDescription();
            }

            target?.Clear();
            foreach (var t in source)
            {
                target?.Add(new DictionaryKeyValue()
                {
                    Id = t.id(),
                    Name = t.name(),
                });
            }

            // This assumes we have a "Disable" track for both subtitles & audio
            if (type == TrackType.Subtitle && _subtitlesTracks?.Count > 1)
            {
                _currentSubtitle = 1;
            }
            else if (type == TrackType.Audio && _audioTracks?.Count > 1)
            {
                _currentAudioTrack = 1;
            }

            Playback_MediaTracksUpdated?.Invoke(type, trackId);
        }

        private async void OnEndReached()
        {
            Playback_MediaEndReached?.Invoke();
            TileHelper.ClearTile();
            PlayerState = MediaState.Ended;
            if (!CanGoNext())
            {
                // Playlist is finished
                if (Repeat)
                {
                    // ... One More Time!
                    await StartAgain();
                    return;
                }
                PlayingType = PlayingType.NotPlaying;
                IsRunning = false;
            }
            else
            {
                await PlayNext();
            }
        }

        private void OnStopped()
        {
            PlayerState = MediaState.Stopped;
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

        private void OnOpening()
        {
            PlayerStateChanged(this, MediaState.Opening);
        }

        private void PlayerStateChanged(object sender, MediaState e)
        {
            Playback_StatusChanged?.Invoke(sender, e);
            PlayerState = e;
        }
        
        #endregion
    }
}
