using System.Threading.Tasks;
using Windows.UI.Core;
using VLC.Model.Music;
using VLC.MusicMetaFetcher;
using VLC.Utils;
using VLC.Helpers;
using VLC.MediaMetaFetcher;
using VLC.Model.Library;

namespace VLC.Services.RunTime
{
    public sealed class MusicMetaService : MetaService
    {
        private readonly MusicMDFetcher _musicMdFetcher = new MusicMDFetcher(App.DeezerAppID, App.ApiKeyLastFm);

        public MusicMetaService(MediaLibrary mediaLibrary, NetworkListenerService networkListenerService) 
            : base(mediaLibrary, networkListenerService)
        {
        }
        
        public async Task GetSimilarArtists(ArtistItem artist)
        {
            var artists = await _musicMdFetcher.GetArtistSimilarsArtist(artist.Name);
            if (artists == null) return;
            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () =>
            {
                artist.IsOnlineRelatedArtistsLoaded = true;
                artist.OnlineRelatedArtists = artists;
            });
        }

        public async Task GetPopularAlbums(ArtistItem artist)
        {
            var albums = await _musicMdFetcher.GetArtistTopAlbums(artist.Name);
            if (albums == null) return;
            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () =>
            {
                artist.IsOnlinePopularAlbumItemsLoaded = true;
                artist.OnlinePopularAlbumItems = albums;
            });
        }

        public async Task GetArtistBiography(ArtistItem artist)
        {
            var bio = await _musicMdFetcher.GetArtistBiography(artist.Name);
            if (bio == null) return;
            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () =>
            {
                artist.Biography = bio;
            });
            MediaLibrary.Update(artist);
        }

        public async Task<bool> GetAlbumCover(AlbumItem album)
        {
            if (NetworkListenerService.IsConnected && !string.IsNullOrEmpty(album.Name))
            {
                var bytes = await _musicMdFetcher.GetAlbumPictureFromInternet(album.Name, album.Artist);
                if (bytes == null)
                {
                    // TODO: Add a TriedWithNoSuccess flag in DB
                }
                var success = bytes != null && await SaveAlbumImageAsync(album, bytes);
                if (success)
                {
                    MediaLibrary.Update(album);
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> GetArtistPicture(ArtistItem artist)
        {
            if (NetworkListenerService.IsConnected && !string.IsNullOrEmpty(artist.Name))
            {
                var bytes = await _musicMdFetcher.GetArtistPicture(artist.Name);
                if (bytes == null)
                {
                    // TODO: Add a TriedWithNoSuccess flag in DB
                }
                var success = bytes != null && await SaveArtistImageAsync(artist, bytes);
                if (!success) return false;
                MediaLibrary.Update(artist);
                return true;
            }
            return false;
        }

        public async Task<bool> SaveAlbumImageAsync(AlbumItem album, byte[] img)
        {
            if (await FetcherHelpers.SaveBytes(album.Id, "albumPic", img, "jpg", false))
            {
                await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () =>
                {
                    album.AlbumCoverUri = $"albumPic/{album.Id}.jpg";
                });
                return true;
            }
            return false;
        }


        public async Task<bool> SaveArtistImageAsync(ArtistItem artist, byte[] img)
        {
            if (await FetcherHelpers.SaveBytes(artist.Id, "artistPic-original", img, "jpg", false))
            {
                // saving full hd max img
                var stream = await ImageHelper.ResizedImage(img, 1920, 1080);
                await FetcherHelpers.SaveBytes(artist.Id, "artistPic-fullsize", stream, "jpg", false);

                stream = await ImageHelper.ResizedImage(img, 250, 250);
                await FetcherHelpers.SaveBytes(artist.Id, "artistPic-thumbnail", stream, "jpg", false);

                await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () => { artist.IsPictureLoaded = true; });
                return true;
            }
            return false;
        }
    }
}
