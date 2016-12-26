using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VLC.Database;
using VLC.Helpers;
using VLC.Model;
using VLC.Model.Music;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Services.RunTime
{
    public class PlaylistService
    {
        public event Action OnPlaylistChanged;
        public event Action OnPlaylistEndReached;
        public event Action<bool> OnRepeatChanged;
        // Parameters: The new current media, a boolean indicating if the playback should start automatically
        public event Action<IMediaItem, bool> OnCurrentMediaChanged;
        private BackgroundTrackDatabase BackgroundTrackRepository { get; set; } = new BackgroundTrackDatabase();
        private ObservableCollection<IMediaItem> _playlist;

        public ObservableCollection<IMediaItem> Playlist
        {
            get { return _playlist; }
        }

        private SmartCollection<IMediaItem> _nonShuffledPlaylist;

        private bool _repeat;
        public bool Repeat
        {
            get { return _repeat; }
            set
            {
                _repeat = value;
                OnRepeatChanged?.Invoke(value);
            }
        }
        private bool _isShuffled;
        public bool IsShuffled
        {
            get { return _isShuffled; }
            set
            {
                if (_isShuffled == value)
                    return;
                ToggleShuffle();
            }
        }

        private int _index;
        public int Index
        {
            get { return _index; }
            set
            {
                if (value < 0)
                    _index = _playlist.Count - 1;
                else if (value >= Playlist.Count)
                    _index = 0;
                else
                    _index = value;
                OnCurrentMediaChanged?.Invoke(CurrentMedia, false);
                var index = _isShuffled ?
                    _nonShuffledPlaylist.IndexOf(Playlist[_index]) : _index;
                ApplicationSettingsHelper.SaveSettingsValue(nameof(Index), index);
            }
        }

        public IMediaItem CurrentMedia { get { return _playlist.ElementAtOrDefault(_index); } }

        public PlaylistService(PlaybackService ps)
        {
            _playlist = new ObservableCollection<IMediaItem>();
            _index = 0;
            ps.Playback_MediaEndReached += async () =>
            {
                // Don't potentially change the media from a VLC thread
                await DispatchHelper.InvokeAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => OnMediaEndReached());
            };
        }

        public void SetCurrentMedia(IMediaItem media)
        {
            var idx = _playlist.IndexOf(media);
            if (idx < 0)
                return;
            Index = idx;
        }

        private void OnMediaEndReached()
        {
            if (CanGoNext == false)
            {
                if (!Repeat)
                {
                    OnPlaylistEndReached?.Invoke();
                    // Don't use the Index property, this would cause the listener to start playback immediately
                    _index = 0;
                    OnCurrentMediaChanged?.Invoke(CurrentMedia, true);
                    return;
                }
                Index = 0;
            }
            else
                Index++;
        }

        public async Task Clear()
        {
            await clear();
            await savePlaylistToBackgroundDB();
            OnPlaylistChanged?.Invoke();
        }

        private async Task clear()
        {
            await BackgroundTrackRepository.Clear();
            _playlist.Clear();
            _nonShuffledPlaylist?.Clear();
            _index = 0;
            _isShuffled = false;
        }

        public bool CanGoPrevious
        {
            get { return _index > 0 || Repeat; }
        }

        public bool CanGoNext
        {
            get { return _index < _playlist.Count - 1 || Repeat; }
        }

        private void ToggleShuffle()
        {
            if (!_isShuffled)
            {
                _nonShuffledPlaylist = new SmartCollection<IMediaItem>(Playlist);
                Random r = new Random();
                for (int i = 0; i < Playlist.Count; i++)
                {
                    if (i != _index)
                    {
                        int index1 = r.Next(i, Playlist.Count);
                        int index2 = r.Next(i, Playlist.Count);
                        if (index1 == _index)
                            index1 = (index1 + 1) % Playlist.Count;
                        if (index2 == _index)
                            index2 = (index2 + 2) % Playlist.Count;
                        Playlist.Move(index1, index2);
                    }
                }
                _isShuffled = true;
            }
            else
            {
                var index = _nonShuffledPlaylist.IndexOf(CurrentMedia);
                _playlist = _nonShuffledPlaylist;
                _index = index;
                _isShuffled = false;
            }
            OnPlaylistChanged?.Invoke();
        }

        public void RemoveMedia(IMediaItem media)
        {
            // CurrentMedia relies on the underlying playlist, so we can't check after removal
            if (_playlist.Remove(media) == true)
            {
                _nonShuffledPlaylist.Remove(media);
                OnPlaylistChanged?.Invoke();
            }
        }

        private async Task savePlaylistToBackgroundDB()
        {
            var trackItems = _playlist.OfType<TrackItem>();
            var backgroundTrackItems = new List<BackgroundTrackItem>();
            foreach (var track in trackItems)
            {
                backgroundTrackItems.Add(new BackgroundTrackItem()
                {
                    TrackId = track.Id
                });
            }
            await BackgroundTrackRepository.Add(backgroundTrackItems);
        }

        public async Task AddToPlaylist(IEnumerable<IMediaItem> toAdd)
        {
            foreach (var m in toAdd)
                _playlist.Add(m);
            OnPlaylistChanged?.Invoke();
            await savePlaylistToBackgroundDB();
        }

        public async Task SetPlaylist(IEnumerable<IMediaItem> mediaItems, int startingIndex = 0)
        {
            await clear();
            foreach (var m in mediaItems)
                _playlist.Add(m);
            OnPlaylistChanged?.Invoke();
            Index = startingIndex;
            await savePlaylistToBackgroundDB();
        }

        public async Task Restore()
        {
            if (!ApplicationSettingsHelper.Contains(nameof(Index)))
                return;

            var playlist = await BackgroundTrackRepository.LoadPlaylist();
            if (!playlist.Any())
                return;

            var trackIds = playlist.Select(node => node.TrackId);
            var restoredplaylist = new SmartCollection<IMediaItem>();
            foreach (int trackId in trackIds)
            {
                var trackItem = Locator.MediaLibrary.LoadTrackById(trackId);
                if (trackItem != null)
                    restoredplaylist.Add(trackItem);
            }

            if (restoredplaylist.Count == 0)
            {
                return;
            }
            await clear();
            _playlist = restoredplaylist;
            OnPlaylistChanged?.Invoke();
            _index = (int)ApplicationSettingsHelper.ReadSettingsValue(nameof(Index));
            OnCurrentMediaChanged?.Invoke(_playlist[_index], true);
        }

        public bool Next()
        {
            if (!CanGoNext)
                return false;
            Index++;
            return true;
        }

        public bool Previous()
        {
            if (!CanGoPrevious)
                return false;
            Index--;
            return true;
        }
    }
}
