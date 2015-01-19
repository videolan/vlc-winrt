using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
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
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var track = Locator.MusicPlayerVM.TrackCollection.Playlist.FirstOrDefault(x => x.Id == trackId);
                if (track != null)
                {
                    SetCurrentTrackPosition(Locator.MusicPlayerVM.TrackCollection.Playlist.IndexOf(track));
                    Task.Run(() => Locator.MusicPlayerVM.Play(false));
                }
            });
        }

        /// <summary>
        /// Adds a track to the Playlist
        /// </summary>
        /// <param name="album"></param>
        /// <param name="resetQueue"></param>
        /// <returns></returns>
        public static async Task AddTrackToPlaylist(TrackItem trackItem = null, bool resetQueue = true, bool play = true)
        {
            await DispatchHelper.InvokeAsync(async () =>
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
                AddTrack(trackItem);
                if (play)
                    await PlayTrack(trackItem.Id);
            });
        }

        /// <summary>
        /// Adds an album to the Playlist
        /// </summary>
        /// <param name="album"></param>
        /// <param name="resetQueue">Reset or not the current Playlist before adding new elements</param>
        /// <returns></returns>
        public static async Task AddAlbumToPlaylist(int albumId, bool resetQueue = true, bool play = true, TrackItem track = null, int index = 0)
        {
            await DispatchHelper.InvokeAsync(async () =>
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
                    AddTrack(trackItem);
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
            });
        }

        /// <summary>
        /// Adds
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="resetQueue"></param>
        /// <returns></returns>
        public static async Task AddArtistToPlaylist(int artistId, bool resetQueue = true, bool play = true)
        {
            await DispatchHelper.InvokeAsync(async () =>
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
                    AddTrack(trackItem);
                }
                if (play)
                    await PlayTrack(trackItems[0].Id);
            });
        }

        public static async Task AddTrackCollectionToPlaylistAndPlay(ObservableCollection<TrackItem> trackCollection)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await Locator.MusicPlayerVM.TrackCollection.ResetCollection();
                await Locator.MusicPlayerVM.TrackCollection.SetPlaylist(trackCollection);
                await PlayTrack(trackCollection[0].Id);
            });
        }

        /// <summary>
        /// Only this method should set the CurrentTrack property of TrackCollection.
        /// </summary>
        /// <param name="index"></param>
        static void SetCurrentTrackPosition(int index)
        {
            Locator.MusicPlayerVM.TrackCollection.CurrentTrack = index;
        }

        static void AddTrack(TrackItem track)
        {
            if (Locator.MusicPlayerVM.TrackCollection.Playlist.FirstOrDefault(x => x.Id == track.Id) == null)
            {
                Locator.MusicPlayerVM.TrackCollection.Add(track, true);
            }
        }
    }
}
