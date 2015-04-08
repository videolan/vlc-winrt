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
        public static async Task Play(this VideoItem videoVm)
        {
            if (string.IsNullOrEmpty(videoVm.Token))
            {
                string token = StorageApplicationPermissions.FutureAccessList.Add(videoVm.File);
                LogHelper.Log("PLAYVIDEO: Getting video path token");
                videoVm.Token = token;
            }
            Locator.NavigationService.Go(VLCPage.VideoPlayerPage);
            LogHelper.Log("PLAYVIDEO: Settings videoVm as Locator.VideoVm.CurrentVideo");
            Locator.VideoVm.CurrentVideo = videoVm;
            await Locator.MediaPlaybackViewModel.TrackCollection.ResetCollection();
            await Locator.MediaPlaybackViewModel.TrackCollection.Add(videoVm, true);
            await Locator.MediaPlaybackViewModel.TrackCollection.SetCurrentTrackPosition(0);
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
        public static async Task PlayTrack(int trackId)
        {
            var track = Locator.MediaPlaybackViewModel.TrackCollection.Playlist.FirstOrDefault(x => x.Id == trackId);
            if (track != null)
            {
                await Locator.MediaPlaybackViewModel.TrackCollection.SetCurrentTrackPosition(Locator.MediaPlaybackViewModel.TrackCollection.Playlist.IndexOf(track));
                await Task.Run(() => Locator.MediaPlaybackViewModel.SetMedia(Locator.MusicPlayerVM.CurrentTrack, false));
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
            await Locator.MediaPlaybackViewModel.TrackCollection.Add(trackItems);
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
            await Locator.MediaPlaybackViewModel.TrackCollection.Add(trackItems);
            if (play)
                await PlayTrack(trackItems[0].Id);
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
                await PlayTrack(finalindex.Id);
            }
        }
        #endregion
    }
}
