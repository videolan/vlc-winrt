using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VLC_WinRT.Model;
using VLC_WinRT.Model.Music;
using VLC_WinRT.Model.Video;
using VLC_WinRT.ViewModels;
using Windows.Storage.AccessCache;

namespace VLC_WinRT.Helpers
{
    public static class PlaylistHelper
    {
        #region Videos
        public static async Task AddVideoToPlaylist(this VideoItem videoVm, bool resetPlaylist = true)
        {
            if (resetPlaylist)
                await Locator.MediaPlaybackViewModel.TrackCollection.ResetCollection();
            await Locator.MediaPlaybackViewModel.TrackCollection.Add(videoVm, true);
            await Locator.MediaPlaybackViewModel.TrackCollection.SetCurrentTrackPosition(0);
        }

        public static async Task Play(this VideoItem videoVm, bool resetPlaylist = true)
        {
            await videoVm.AddVideoToPlaylist(resetPlaylist);
            LogHelper.Log("PLAYVIDEO: Settings videoVm as Locator.VideoPlayerVm.CurrentVideo");
            Locator.VideoPlayerVm.CurrentVideo = videoVm;
            await Task.Run(() => Locator.MediaPlaybackViewModel.SetMedia(Locator.VideoPlayerVm.CurrentVideo, false));
        }

        #endregion

        #region Music
        /// <summary>
        /// Play a track
        /// If the track is already in the Playlist, we set the CurrenTrack to reach this new track
        /// If not, we reset the Playlist, and set the CurrentTrack to 0
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        public static async Task PlayMusicTrack(int trackId)
        {
            var track = Locator.MediaPlaybackViewModel.TrackCollection.Playlist.FirstOrDefault(x => x.Id == trackId);
            if (track != null)
            {
                await Locator.MediaPlaybackViewModel.TrackCollection.SetCurrentTrackPosition(Locator.MediaPlaybackViewModel.TrackCollection.Playlist.IndexOf(track));
                await Locator.MediaPlaybackViewModel.SetMedia(Locator.MusicPlayerVM.CurrentTrack, false);
            }
        }

        /// Play a track from FilePicker
        public static async Task PlayTrackFromFilePicker(TrackItem trackItem)
        {
            if (trackItem == null) return;
            await Locator.MediaPlaybackViewModel.TrackCollection.ResetCollection();
            await Locator.MediaPlaybackViewModel.TrackCollection.Add(trackItem, true);
            await Locator.MediaPlaybackViewModel.TrackCollection.SetCurrentTrackPosition(0);
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
            await Locator.MediaPlaybackViewModel.TrackCollection.Add(trackItem, true);
            if (play)
                await PlayMusicTrack(trackItem.Id);
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
            var trackItems = Locator.MusicLibraryVM.MusicLibrary.LoadTracksByAlbumId(albumId);
            await Locator.MediaPlaybackViewModel.TrackCollection.Add(trackItems);
            if (play)
            {
                if (track != null)
                {
                    index = trackItems.IndexOf(trackItems.FirstOrDefault(x => x.Id == track.Id));
                }
                if (index != -1 && index < trackItems.Count)
                    await PlayMusicTrack(trackItems[index].Id);
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
            var trackItems = await Locator.MusicLibraryVM.MusicLibrary.LoadTracksByArtistId(artistId);
            await Locator.MediaPlaybackViewModel.TrackCollection.Add(trackItems);
            if (play)
                await PlayMusicTrack(trackItems[0].Id);
        }

        public static async Task AddTrackCollectionToPlaylistAndPlay(ObservableCollection<IVLCMedia> trackCollection, bool play = true, int? index = null)
        {
            await Locator.MediaPlaybackViewModel.TrackCollection.ResetCollection();
            await Locator.MediaPlaybackViewModel.TrackCollection.SetPlaylist(trackCollection);

            if (play)
            {
                if (!trackCollection.Any()) return;
                if (index >= trackCollection.Count) return;
                var finalindex = trackCollection[index ?? 0];
                if (finalindex is TrackItem)
                    await PlayMusicTrack(finalindex.Id);
                else if (finalindex is VideoItem)
                    await ((VideoItem)finalindex).Play(false);
            }
        }
        #endregion
    }
}
