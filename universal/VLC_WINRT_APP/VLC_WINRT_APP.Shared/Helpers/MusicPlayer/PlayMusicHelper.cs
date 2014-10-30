using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Helpers.MusicLibrary;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MusicVM;

namespace VLC_WINRT_APP.Helpers.MusicPlayer
{
    public static class PlayMusickHelper
    {
        public static async Task PlayTrackCollection(this TrackCollection trackCollection)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Locator.MusicPlayerVM.TrackCollection.ResetCollection();
                Locator.MusicPlayerVM.TrackCollection.Playlist = trackCollection.Playlist;
                SetCurrentTrackPosition(0);
            });
            Task.Run(() => Locator.MusicPlayerVM.Play());
        }

        /// <summary>
        /// Play a track
        /// If the track is already in the Playlist, we set the CurrenTrack to reach this new track
        /// If not, we reset the Playlist, and set the CurrentTrack to 0
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        public static async Task PlayTrack(this TrackItem track)
        {
            if (track == null) return;
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (!Locator.MusicPlayerVM.TrackCollection.Playlist.Contains(track))
                {
                    Locator.MusicPlayerVM.TrackCollection.ResetCollection();
                    Locator.MusicPlayerVM.AddTrack(track);
                }
                else
                {
                }
                SetCurrentTrackPosition(Locator.MusicPlayerVM.TrackCollection.Playlist.IndexOf(track));
            });
            Task.Run(() => Locator.MusicPlayerVM.Play());
        }

        /// <summary>
        /// Play an album from the start
        /// </summary>
        /// <param name="album"></param>
        /// <param name="index">
        /// Possibility to set the CurrentTrack index
        /// </param>
        /// <returns></returns>
        public static async Task PlayAlbum(this AlbumItem album, int index = 0)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await album.AddToQueue();
                SetCurrentTrackPosition(index);
            });
            Task.Run(() => Locator.MusicPlayerVM.Play());
        }

        public static async Task PlayArtist(this ArtistItem artist)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                foreach (AlbumItem album in artist.Albums)
                {
                    await album.AddToQueue(false);
                }
                SetCurrentTrackPosition(0);
            });
            Task.Run(() => Locator.MusicPlayerVM.Play());
        }

        /// <summary>
        /// Adds an album to the Playlist
        /// </summary>
        /// <param name="album"></param>
        /// <param name="resetQueue">Reset or not the current Playlist before adding new elements</param>
        /// <returns></returns>
        public static async Task AddToQueue(this AlbumItem album, bool resetQueue = true)
        {
            await DispatchHelper.InvokeAsync(() =>
            {
                if (resetQueue)
                {
                    Locator.MusicPlayerVM.TrackCollection.ResetCollection();
                }
                Locator.MusicPlayerVM.AddTrack(album.Tracks.ToList());
            });
        }

        /// <summary>
        /// Only this method should set the CurrentTrack property of TrackCollection.
        /// </summary>
        /// <param name="index"></param>
        public static void SetCurrentTrackPosition(int index)
        {
            Locator.MusicPlayerVM.TrackCollection.CurrentTrack = index;
        }
    }
}
