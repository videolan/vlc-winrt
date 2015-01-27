using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;
using libVLCX;
using VLC_WINRT.Common;
using VLC_WINRT_APP.BackgroundHelpers;
using VLC_WINRT_APP.Common;
using VLC_WINRT_APP.Helpers.MusicLibrary.Deezer;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.BackgroundHelpers;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MusicVM;

namespace VLC_WINRT_APP.Helpers.MusicPlayer
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
            var track = Locator.MusicPlayerVM.TrackCollection.Playlist.FirstOrDefault(x => x.Id == trackId);
            if (track != null)
            {
                await SetCurrentTrackPosition(Locator.MusicPlayerVM.TrackCollection.Playlist.IndexOf(track));
                await Task.Run(() => Locator.MusicPlayerVM.Play(false));
            }
        }

        ///
        /// Play a track from FilePicker
        public static async Task PlayTrackFromFilePicker(StorageFile file, TrackItem trackItem)
        {
            if (trackItem == null) return;
            await Locator.MusicPlayerVM.TrackCollection.ResetCollection();
            var backgroundTrack = BackgroundTaskTools.CreateBackgroundTrackItem(trackItem);
            await App.BackgroundAudioHelper.AddToPlaylist(backgroundTrack);
            await AddTrack(trackItem);
            await SetCurrentTrackPosition(0);
            await Task.Run(() => Locator.MusicPlayerVM.Play(false, file));
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
                await Locator.MusicPlayerVM.TrackCollection.ResetCollection();
            }
            if (Locator.MusicPlayerVM.TrackCollection.Playlist.FirstOrDefault(x => x.Id == trackItem.Id) == null)
            {
                var backgroundTrack = BackgroundTaskTools.CreateBackgroundTrackItem(trackItem);
                await App.BackgroundAudioHelper.AddToPlaylist(backgroundTrack);
            }
            await AddTrack(trackItem);
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
                await Locator.MusicPlayerVM.TrackCollection.ResetCollection();
            }
            var trackItems = await MusicLibraryVM._trackDataRepository.LoadTracksByAlbumId(albumId);
            var backgroundTracks = BackgroundTaskTools.CreateBackgroundTrackItemList(trackItems.ToList());
            await App.BackgroundAudioHelper.AddToPlaylist(backgroundTracks);

            foreach (var trackItem in trackItems)
            {
                await AddTrack(trackItem);
            }
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
                await Locator.MusicPlayerVM.TrackCollection.ResetCollection();
            }
            var trackItems = await MusicLibraryVM._trackDataRepository.LoadTracksByArtistId(artistId);
            var backgroundTracks = BackgroundTaskTools.CreateBackgroundTrackItemList(trackItems.ToList());
            await App.BackgroundAudioHelper.AddToPlaylist(backgroundTracks);
            foreach (var trackItem in trackItems)
            {
                await AddTrack(trackItem);
            }
            if (play)
                await PlayTrack(trackItems[0].Id);
        }

        public static async Task AddTrackCollectionToPlaylistAndPlay(ObservableCollection<TrackItem> trackCollection, bool play = true, int? index = null)
        {
            await Locator.MusicPlayerVM.TrackCollection.ResetCollection();
            await Locator.MusicPlayerVM.TrackCollection.SetPlaylist(trackCollection);

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
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Locator.MusicPlayerVM.TrackCollection.CurrentTrack = index);
        }

        static async Task AddTrack(TrackItem track)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (Locator.MusicPlayerVM.TrackCollection.Playlist.FirstOrDefault(x => x.Id == track.Id) == null)
                {
                    Locator.MusicPlayerVM.TrackCollection.Add(track, true);
                }
            });
        }
    }
}
