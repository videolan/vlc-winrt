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
using VLC_WinRT.Commands.MusicPlayer;
using VLC_WinRT.Model.Music;
using System.Collections.Generic;
using System.Linq;
using VLC_WinRT.BackgroundHelpers;
using VLC_WinRT.BackgroundAudioPlayer.Model;
using VLC_WinRT.Helpers;
using VLC_WinRT.Model;
using VLC_WinRT.Model.Video;
using VLC_WinRT.Utils;
using System.Diagnostics;
using VLC_WinRT.Services.RunTime;
using VLC_WinRT.Model.Stream;
using VLC_WinRT.Services.Interface;
using Windows.Storage;
using System.IO;
using libVLCX;
using VLC_WinRT.SharedBackground.Database;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Services.RunTime
{
    public class PlaybackService
    {
        public event EventHandler<MediaState> Playback_StatusChanged;
        public event Action<IMediaItem> Playback_MediaSet;
        public event Action<ParsedStatus> Playback_MediaParsed;
        public event Action<TrackType, int> Playback_MediaTracksUpdated;
        public event TimeChanged Playback_MediaTimeChanged;
        public event EventHandler Playback_MediaFailed;
        public event Action<IMediaService> Playback_MediaStopped;
        public event Action<long> Playback_MediaLengthChanged;
        public event Action Playback_MediaEndReached;
        public event Action<int> Playback_MediaBuffering;
        public event Action<IMediaItem> Playback_MediaFileNotFound;

        private PlayerEngine _playerEngine;

        private List<DictionaryKeyValue> _subtitlesTracks = new List<DictionaryKeyValue>();
        private List<DictionaryKeyValue> _audioTracks = new List<DictionaryKeyValue>();
        private List<VLCChapterDescription> _chapters = new List<VLCChapterDescription>();
        private int _currentSubtitle;
        private int _currentAudioTrack;

        private SmartCollection<IMediaItem> _mediasCollection;
        private SmartCollection<IMediaItem> _nonShuffledPlaylist;
        private bool _repeat;

        public BackgroundTrackDatabase BackgroundTrackRepository { get; set; } = new BackgroundTrackDatabase();
        private IMediaService _mediaService
        {
            get
            {
                switch (_playerEngine)
                {
                    case PlayerEngine.VLC:
                        return Locator.VLCService;
#if WINDOWS_PHONE_APP
                    case PlayerEngine.BackgroundMFPlayer:
                        return Locator.BGPlayerService;
#endif
                    default:
                        //todo : Implement properly BackgroundPlayerService 
                        //todo : so we get rid ASAP of this default switch
                        return Locator.VLCService;
                }
            }
        }

        public bool UseVlcLib;

        public int CurrentMedia { get; private set; }

        public PlayingType PlayingType { get; set; }

        public bool Repeat
        {
            get { return _repeat; }
            set
            {
                _repeat = value;
#if WINDOWS_PHONE_APP
                if (_mediaService is BGPlayerService)
                {
                    ((BGPlayerService)_mediaService).SetRepeat(value);
                }
#endif
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
            get { return _mediasCollection; }
            private set { _mediasCollection = value; }
        }

        public SmartCollection<IMediaItem> NonShuffledPlaylist
        {
            get { return _nonShuffledPlaylist; }
            set { _nonShuffledPlaylist = value; }
        }

        #endregion

        #region ctors
        public PlaybackService()
        {
            Task.Run(() => RestorePlaylist());
            _mediasCollection = new SmartCollection<IMediaItem>();
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
            await App.BackgroundAudioHelper.ResetCollection(ResetType.NormalReset);
            Playlist.Clear();
            CurrentMedia = -1;
            NonShuffledPlaylist?.Clear();
            IsShuffled = false;
            Playback_MediaSet(null);
            IsRunning = false;
        }

        public async Task Shuffle()
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
                await SetPlaylist(Playlist, true, false, null);
            }
            else
            {
                Playlist.Clear();
                await SetPlaylist(NonShuffledPlaylist, true, false, null);
            }
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
                foreach (var track in trackItems)
                {
                    track.Index = count;
                    count++;
                }

                Playlist.AddRange(mediaItems);

                var backgroundTracks = BackgroundTaskTools.CreateBackgroundTrackItemList(trackItems);
                await App.BackgroundAudioHelper.AddToPlaylist(backgroundTracks);

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
                await SetMedia(mediaInPlaylist, true, play);
            }
            return true;
        }

        private async Task RestorePlaylist()
        {
            var playlist = BackgroundTrackRepository.LoadPlaylist();
            if (!playlist.Any())
            {
                return;
            }

            var trackIds = playlist.Select(node => node.Id);
            Playlist = new SmartCollection<IMediaItem>();
            foreach (int trackId in trackIds)
            {
                var trackItem = await Locator.MediaLibrary.LoadTrackById(trackId);
                if (trackItem != null)
                    Playlist.Add(trackItem);
            }

#if WINDOWS_PHONE_APP
                    // TODO : this shouldn't be here
                    var milliseconds = BackgroundAudioHelper.Instance?.NaturalDuration.TotalMilliseconds;
                    if (milliseconds != null && milliseconds.HasValue && double.IsNaN(milliseconds.Value))
                        OnLengthChanged((long)milliseconds);
#endif
            if (!ApplicationSettingsHelper.Contains(BackgroundAudioConstants.CurrentTrack))
                return;
            var index = (int)ApplicationSettingsHelper.ReadSettingsValue(BackgroundAudioConstants.CurrentTrack);
            if (Playlist.Any())
            {
                if (index == -1)
                {
                    // Background Audio was terminated
                    // We need to reset the playlist, or set the current track 0.
                    ApplicationSettingsHelper.SaveSettingsValue(BackgroundAudioConstants.CurrentTrack, 0);
                    index = 0;
                }
                SetCurrentMediaPosition(index);
            }

            App.BackgroundAudioHelper.RestorePlaylist();
            await SetPlaylist(null, false, false, Playlist[CurrentMedia]);
        }

        private async Task SetMedia(IMediaItem media, bool forceVlcLib = false, bool autoPlay = true)
        {
            if (media == null)
                throw new ArgumentNullException(nameof(media), "Media is missing. Can't play");
            UseVlcLib = forceVlcLib;

            if (media is VideoItem)
            {
                var video = (VideoItem)media;
                await InitializePlayback(video, autoPlay);

                if (video.TimeWatched != TimeSpan.FromSeconds(0))
                {
                    SetTime((long)video.TimeWatched.TotalMilliseconds);
                }
#if WINDOWS_PHONE_APP
                    try
                    {
                        var messageDictionary = new ValueSet();
                        messageDictionary.Add(BackgroundAudioConstants.ClearUVC, "");
                        BackgroundMediaPlayer.SendMessageToBackground(messageDictionary);
                    }
                    catch
                    {
                    }
#else

                //await SetMediaTransportControlsInfo(string.IsNullOrEmpty(video.Name) ? Strings.Video : video.Name);
#endif
#if WINDOWS_UWP
                TileHelper.UpdateVideoTile();
#else
                TileHelper.UpdateMediumTileWithVideoInfo();
#endif
            }
            else if (media is TrackItem)
            {
                var track = (TrackItem)media;
                StorageFile currentTrackFile;
                try
                {
                    currentTrackFile = track.File ?? await StorageFile.GetFileFromPathAsync(track.Path);
                }
                catch (Exception exception)
                {
                    Playback_MediaFileNotFound?.Invoke(media);
                    return;
                }

                await InitializePlayback(track, autoPlay);

                ApplicationSettingsHelper.SaveSettingsValue(BackgroundAudioConstants.CurrentTrack, CurrentMedia);
            }
            else if (media is StreamMedia)
            {
                UseVlcLib = true;
                await InitializePlayback(media, autoPlay);
            }
        }

        private async Task InitializePlayback(IMediaItem media, bool autoPlay)
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
#if WINDOWS_PHONE_APP
                        ToastHelper.Basic(Strings.FailFilePlayBackground, false, "background");
#endif
                        _playerEngine = PlayerEngine.VLC;
                        Stop();
                    }
                }
                else
                {
                    _playerEngine = PlayerEngine.VLC;
                }
            }

            _mediaService.MediaFailed += MediaFailed;
            _mediaService.StatusChanged += PlayerStateChanged;
            _mediaService.TimeChanged += UpdateTime;

            if (autoPlay)
            {
                // Reset the libVLC log file
                await LogHelper.ResetBackendFile();
            }

            // Send the media we want to play
            await _mediaService.SetMediaFile(media);

            _mediaService.OnLengthChanged += OnLengthChanged;
            _mediaService.OnStopped += OnStopped;
            _mediaService.OnEndReached += OnEndReached;
            _mediaService.OnBuffering += OnBuffering;

            switch (_playerEngine)
            {
                case PlayerEngine.VLC:
                    var vlcService = (VLCService)_mediaService;
                    if (vlcService.MediaPlayer == null) return;
                    var em = vlcService.MediaPlayer.eventManager();
                    em.OnTrackAdded += OnTrackAdded;
                    em.OnTrackDeleted += OnTrackDeleted;
                    var mem = vlcService.MediaPlayer.media().eventManager();
                    mem.OnParsedChanged += OnParsedStatus;
                    if (!autoPlay)
                        return;
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
                    _mediaService.Play(Playlist[CurrentMedia].Id);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            SetSpeedRate(1);
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
            get { return _mediaService.GetVolume(); }
            set { _mediaService.SetVolume(value); }
        }

        public void SetSubtitleTrack(int i)
        {
            switch (_playerEngine)
            {
                case PlayerEngine.VLC:
                    var vlcService = (VLCService)_mediaService;
                    vlcService.SetSubtitleTrack(i);
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
                    vlcService.SetAudioTrack(i);
                    break;
                case PlayerEngine.MediaFoundation:
                    break;
                case PlayerEngine.BackgroundMFPlayer:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetAudioDelay(long delay)
        {
            if (_mediaService is VLCService)
            {
                (_mediaService as VLCService).SetAudioDelay(delay * 1000);
            }
        }

        public void SetSpuDelay(long delay)
        {
            if (_mediaService is VLCService)
            {
                (_mediaService as VLCService).SetSpuDelay(delay * 1000);
            }
        }

        public void SetTime(long time)
        {
            _mediaService.SetTime(time);
        }

        public long GetTime()
        {
            return _mediaService.GetTime();
        }

        public void SetPosition(float pos)
        {
            _mediaService.SetPosition(pos);
        }

        public float GetPosition()
        {
            return _mediaService.GetPosition();
        }

        public void OpenSubtitleMrl(string mrl)
        {
            switch (_playerEngine)
            {
                case PlayerEngine.VLC:
                    var vlcService = (VLCService)_mediaService;
                    vlcService.SetSubtitleFile(mrl);
                    break;
                case PlayerEngine.MediaFoundation:
                    break;
                case PlayerEngine.BackgroundMFPlayer:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetSpeedRate(float rate)
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

        public VLCChapterDescription GetCurrentChapter()
        {
            if (!(_mediaService is VLCService))
                return null;
            var vlcService = (VLCService)_mediaService;
            var currentChapter = vlcService.MediaPlayer?.chapter();
            if (_chapters?.Count > 0 && currentChapter.HasValue)
            {
                return _chapters[currentChapter.Value];
            }
            return null;
        }

        public void SetCurrentChapter(VLCChapterDescription chapter)
        {
            if (!(_mediaService is VLCService))
                return;

            var vlcService = (VLCService)_mediaService;

            if (chapter == GetCurrentChapter())
                return;
            var index = _chapters.IndexOf(chapter);
            if (index > -1)
            {
                vlcService.MediaPlayer.setChapter(index);
            }
        }

        public IEnumerable<VLCChapterDescription> GetChapters()
        {
            return _chapters;
        }

        public DictionaryKeyValue GetCurrentAudioTrack()
        {
            if (_currentAudioTrack == -1 || _currentAudioTrack >= _audioTracks.Count)
                return null;

            return _audioTracks[_currentAudioTrack];
        }

        public void SetAudioTrack(DictionaryKeyValue audioTrack)
        {
            if (!(_mediaService is VLCService))
                return;

            _currentAudioTrack = _audioTracks.IndexOf(audioTrack);
            if (audioTrack != null)
            {
                var vlcService = (VLCService)_mediaService;
                vlcService.SetAudioTrack(audioTrack.Id);
            }
        }

        public IEnumerable<DictionaryKeyValue> GetAudioTracks()
        {
            return _audioTracks;
        }
        
        public DictionaryKeyValue GetCurrentSubtitleTrack()
        {
            if (_currentSubtitle < 0 || _currentSubtitle >= _subtitlesTracks.Count)
                return null;
            return _subtitlesTracks[_currentSubtitle];
        }

        public void SetSubtitleTrack(DictionaryKeyValue subTrack)
        {
            if (!(_mediaService is VLCService))
                return;

            _currentSubtitle = _subtitlesTracks.IndexOf(subTrack);
            if (subTrack != null)
            {
                var vlcService = (VLCService)_mediaService;
                vlcService.SetSubtitleTrack(subTrack.Id);
            }
        }

        public IEnumerable<DictionaryKeyValue> GetSubtitleTracks()
        {
            return _subtitlesTracks;
        }

        public void Stop()
        {
            _mediaService.MediaFailed -= MediaFailed;
            _mediaService.StatusChanged -= PlayerStateChanged;
            _mediaService.TimeChanged -= UpdateTime;

            _mediaService.OnLengthChanged -= OnLengthChanged;
            _mediaService.OnStopped -= OnStopped;
            _mediaService.OnEndReached -= OnEndReached;
            _mediaService.OnBuffering -= OnBuffering;

            if (_mediaService is VLCService)
            {
                var vlcService = (VLCService)_mediaService;
                if (vlcService.MediaPlayer != null)
                {
                    var em = vlcService.MediaPlayer.eventManager();
                    em.OnTrackAdded -= OnTrackAdded;
                    em.OnTrackDeleted -= OnTrackDeleted;
                }

                _currentAudioTrack = -1;
                _currentSubtitle = -1;

                _audioTracks.Clear();
                _subtitlesTracks.Clear();
            }
            else if (_mediaService is MFService)
            {
                var mfService = (MFService)_mediaService;
                mfService.Instance.Source = null;
            }

            if (PlayerState != MediaState.Stopped)
            {
                _mediaService.Stop();
            }
#if STARTS
#else
            TileHelper.ClearTile();
#endif
        }

        public void Pause()
        {
            _mediaService?.Pause();
        }

        public void Play()
        {
            _mediaService?.Play();
        }

        public void SetSizeVideoPlayer(uint x, uint y)
        {
            _mediaService?.SetSizeVideoPlayer(x, y);
        }

        void SetPlaybackTypeFromTracks()
        {
            var videoTrack = Locator.VLCService.MediaPlayer.media().tracks().FirstOrDefault(x => x.type() == TrackType.Video);

            if (videoTrack == null)
            {
                PlayingType = PlayingType.Music;
            }
            else
            {
                PlayingType = PlayingType.Video;
            }
        }
        #endregion

        #region Playback events callbacks

        private void OnParsedStatus(ParsedStatus parsedStatus)
        {
            if (parsedStatus != ParsedStatus.Done)
                return;

            if (!(_mediaService is VLCService)) return;
            var vlcService = (VLCService)_mediaService;
            var mP = vlcService?.MediaPlayer;
            // Get chapters
            _chapters.Clear();
            var chapters = mP?.chapterDescription(-1);
            foreach (var c in chapters)
            {
                var vlcChapter = new VLCChapterDescription(c);
                _chapters.Add(vlcChapter);
            }

            // Get subtitle delay etc
            if (mP != null)
            {
                SetAudioDelay(mP.audioDelay());
                SetSpuDelay(mP.spuDelay());
            }

            if (vlcService.MediaPlayer == null)
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
                source = ((VLCService)_mediaService).MediaPlayer?.audioTrackDescription();
            }
            else
            {
                target = _subtitlesTracks;
                source = ((VLCService)_mediaService).MediaPlayer?.spuDescription();
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

        private void OnBuffering(int progress)
        {
            Playback_MediaBuffering?.Invoke(progress);
        }

        private async void OnEndReached()
        {
            Playback_MediaEndReached?.Invoke();
            TileHelper.ClearTile();
            PlayerState = MediaState.Stopped;
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

        private void OnStopped(IMediaService mediaService)
        {
            Debug.WriteLine("OnStopped event called from " + mediaService);
            
            PlayerState = MediaState.Stopped;
            Playback_MediaStopped?.Invoke(mediaService);
        }

        private void OnLengthChanged(long length)
        {
            Playback_MediaLengthChanged?.Invoke(length);
        }

        private void UpdateTime(long time)
        {
            Playback_MediaTimeChanged?.Invoke(time);
        }

        private void PlayerStateChanged(object sender, MediaState e)
        {
            Playback_StatusChanged?.Invoke(sender, e);
            PlayerState = e;
        }

        private void MediaFailed(object sender, EventArgs e)
        {
            Stop();
            Playback_MediaFailed?.Invoke(sender, e);
        }
        #endregion
    }
}
