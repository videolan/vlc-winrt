using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MusicVM;

namespace VLC_WINRT_APP.Helpers
{
    public static class PlayMusickHelper
    {
        public static async Task Play(this MusicLibraryVM.TrackItem track)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (track != null && !Locator.MusicPlayerVM.TrackCollection.Contains(track))
                {
                    Locator.MusicPlayerVM.ResetCollection();
                    Locator.MusicPlayerVM.AddTrack(track);
                }
                else
                {
                    Locator.MusicPlayerVM.CurrentTrack =
                        Locator.MusicPlayerVM.TrackCollection.IndexOf(track);
                    int index = Locator.MusicPlayerVM.CurrentPlayingArtist.CurrentAlbumItem.Tracks.IndexOf(track);
                    Locator.MusicPlayerVM.CurrentPlayingArtist.CurrentAlbumItem.CurrentTrackPosition = index;
                }
            });
            Locator.MusicPlayerVM.Play();
        }

        public static async Task Play(this MusicLibraryVM.AlbumItem album, int index = 0)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await album.AddToQueue();
                await SetIndex(index);
                await SetCurrentPlayingArtist(album);
                if (Locator.MusicPlayerVM.CurrentPlayingArtist != null)
                {
                    SetCurrentPlayingAlbum(album);
                    SetCurrentTrackPosition(index);
                }
                Task.Run(() => Locator.MusicPlayerVM.Play());
            });
        }

        public static async Task AddToQueue(this MusicLibraryVM.AlbumItem album, bool resetQueue = true)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (resetQueue)
                {
                    Locator.MusicPlayerVM.ResetCollection();
                }
                Locator.MusicPlayerVM.AddTrack(album.Tracks.ToList());
                SetCurrentPlayingArtist(album);
            });
        }

        public static async Task SetIndex(int index = 0)
        {
            Locator.MusicPlayerVM.CurrentTrack = index;
        }

        public static async Task SetCurrentPlayingArtist(MusicLibraryVM.AlbumItem album)
        {
            Locator.MusicPlayerVM.CurrentPlayingArtist =
                Locator.MusicLibraryVM.Artists.FirstOrDefault(x => x.Name == album.Artist);
        }

        public static void SetCurrentPlayingAlbum(MusicLibraryVM.AlbumItem album)
        {
            Locator.MusicPlayerVM.CurrentPlayingArtist.CurrentAlbumIndex =
                Locator.MusicPlayerVM.CurrentPlayingArtist.Albums.IndexOf(album);
        }

        public static void SetCurrentTrackPosition(int index)
        {
            Locator.MusicPlayerVM.CurrentPlayingArtist.CurrentAlbumItem.CurrentTrackPosition = index;
        }
    }
}
