using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using VLC_WinRT.Model.Music;
using VLC_WinRT.ViewModels;
#if WINDOWS_PHONE_APP
using VLC_WinRT.BackgroundHelpers;
#endif

namespace VLC_WinRT.Helpers.MusicPlayer
{
    public static class PlayMusicHelper
    {
        /// <summary>
        /// Play a track
        /// If the track is already in the Playlist, we set the CurrenTrack to reach this new track
        /// If not, we reset the Playlist, and set the CurrentTrack to 0
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        public static async Task PlayTrack(int trackId)
        {
            var track = Locator.MediaPlaybackViewModel.TrackCollection.Playlist.FirstOrDefault(x => x.Id == trackId);
            if (track != null)
            {
                await SetCurrentTrackPosition(Locator.MediaPlaybackViewModel.TrackCollection.Playlist.IndexOf(track));
                await Task.Run(() => Locator.MediaPlaybackViewModel.SetMedia(Locator.MusicPlayerVM.CurrentTrack, false));
            }
        }

        ///
        /// Play a track from FilePicker
        public static async Task PlayTrackFromFilePicker(TrackItem trackItem)
        {
            if (trackItem == null) return;
            await Locator.MediaPlaybackViewModel.TrackCollection.ResetCollection();
#if WINDOWS_PHONE_APP
            var backgroundTrack = BackgroundTaskTools.CreateBackgroundTrackItem(trackItem);
            await App.BackgroundAudioHelper.AddToPlaylist(backgroundTrack);
#endif
            AddTrack(trackItem);
            await SetCurrentTrackPosition(0);
            await Task.Run(async () => await Locator.MediaPlaybackViewModel.SetMedia(Locator.MusicPlayerVM.CurrentTrack, false));
        }

        /// <summary>
        /// Adds a track to the Playlist
        /// </summary>
        /// <param name="album"></param>
        /// <param name="resetQueue"></param>
        /// <returns></returns>
        public static async Task AddTrackToPlaylist(TrackItem trackItem = null, bool resetQueue = true, bool play = true)
        {
            if (trackItem == null) return;
            if (resetQueue)
            {
                await Locator.MediaPlaybackViewModel.TrackCollection.ResetCollection();
            }
#if WINDOWS_PHONE_APP
            if (Locator.MediaPlaybackViewModel.TrackCollection.Playlist.FirstOrDefault(x => x.Id == trackItem.Id) == null)
            {
                var backgroundTrack = BackgroundTaskTools.CreateBackgroundTrackItem(trackItem);
                await App.BackgroundAudioHelper.AddToPlaylist(backgroundTrack);
            }
#endif
            AddTrack(trackItem);
            if (play)
                await PlayTrack(trackItem.Id);
        }

        /// <summary>
        /// Adds an album to the Playlist
        /// </summary>
        /// <param name="album"></param>
        /// <param name="resetQueue">Reset or not the current Playlist before adding new elements</param>
        /// <returns></returns>
        public static async Task AddAlbumToPlaylist(int albumId, bool resetQueue = true, bool play = true, TrackItem track = null, int index = 0)
        {
            if (resetQueue)
            {
                await Locator.MediaPlaybackViewModel.TrackCollection.ResetCollection();
            }
            var trackItems = await Locator.MusicLibraryVM._trackDataRepository.LoadTracksByAlbumId(albumId);
#if WINDOWS_PHONE_APP
            var backgroundTracks = BackgroundTaskTools.CreateBackgroundTrackItemList(trackItems);
            await App.BackgroundAudioHelper.AddToPlaylist(backgroundTracks);
#endif
            AddTracks(trackItems);
            if (play)
            {
                if (track != null)
                {
                    index = trackItems.IndexOf(trackItems.FirstOrDefault(x => x.Id == track.Id));
                }
                if (index != -1)
                    await PlayTrack(trackItems[index].Id);
            }
        }

        /// <summary>
        /// Adds
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="resetQueue"></param>
        /// <returns></returns>
        public static async Task AddArtistToPlaylist(int artistId, bool resetQueue = true, bool play = true)
        {
            if (resetQueue)
            {
                await Locator.MediaPlaybackViewModel.TrackCollection.ResetCollection();
            }
            var trackItems = await Locator.MusicLibraryVM._trackDataRepository.LoadTracksByArtistId(artistId);
#if WINDOWS_PHONE_APP
            var backgroundTracks = BackgroundTaskTools.CreateBackgroundTrackItemList(trackItems);
            await App.BackgroundAudioHelper.AddToPlaylist(backgroundTracks);
#endif
            AddTracks(trackItems);
            if (play)
                await PlayTrack(trackItems[0].Id);
        }

        public static async Task AddTrackCollectionToPlaylistAndPlay(ObservableCollection<TrackItem> trackCollection, bool play = true, int? index = null)
        {
            await Locator.MediaPlaybackViewModel.TrackCollection.ResetCollection();
            await Locator.MediaPlaybackViewModel.TrackCollection.SetPlaylist(trackCollection);

            if (play)
            {
                if (!trackCollection.Any()) return;
                if (index >= trackCollection.Count) return;
                var finalindex = trackCollection[index ?? 0];
                await PlayTrack(finalindex.Id);
            }
        }

        /// <summary>
        /// Only this method should set the CurrentTrack property of TrackCollection.
        /// </summary>
        /// <param name="index"></param>
        static async Task SetCurrentTrackPosition(int index)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Locator.MediaPlaybackViewModel.TrackCollection.CurrentTrack = index);
        }

        static void AddTrack(TrackItem track)
        {
            if (Locator.MediaPlaybackViewModel.TrackCollection.Playlist.FirstOrDefault(x => x.Id == track.Id) == null)
            {
                Locator.MediaPlaybackViewModel.TrackCollection.Add(track, true);
            }
        }

        static void AddTracks(IEnumerable<TrackItem> tracks)
        {
            foreach (var track in tracks.Where(track => Locator.MediaPlaybackViewModel.TrackCollection.Playlist.FirstOrDefault(x => x.Id == track.Id) == null))
            {
                Locator.MediaPlaybackViewModel.TrackCollection.Add(track, true);
            }
        }
    }
}
