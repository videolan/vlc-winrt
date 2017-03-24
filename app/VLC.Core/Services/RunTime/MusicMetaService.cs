using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using VLC.Model.Music;
using VLC.MusicMetaFetcher;
using VLC.MusicMetaFetcher.Models.MusicEntities;
using VLC.Utils;
using VLC.ViewModels;
using VLC.Helpers;
using VLC.MediaMetaFetcher;

namespace VLC.Services.RunTime
{
    public sealed class MusicMetaService
    {
        readonly MusicMDFetcher musicMdFetcher = new MusicMDFetcher(App.DeezerAppID, App.ApiKeyLastFm);

        public async Task GetSimilarArtists(ArtistItem artist)
        {
            var artists = await musicMdFetcher.GetArtistSimilarsArtist(artist.Name);
            if (artists == null) return;
            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () =>
            {
                artist.IsOnlineRelatedArtistsLoaded = true;
                artist.OnlineRelatedArtists = artists;
            });
        }

        public async Task GetPopularAlbums(ArtistItem artist)
        {
            var albums = await musicMdFetcher.GetArtistTopAlbums(artist.Name);
            if (albums == null) return;
            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () =>
            {
                artist.IsOnlinePopularAlbumItemsLoaded = true;
                artist.OnlinePopularAlbumItems = albums;
            });
        }

        public async Task GetArtistBiography(ArtistItem artist)
        {
            var bio = await musicMdFetcher.GetArtistBiography(artist.Name);
            if (bio == null) return;
            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () =>
            {
                artist.Biography = bio;
            });
            Locator.MediaLibrary.Update(artist);
        }

        public async Task<bool> GetAlbumCover(AlbumItem album)
        {
            if (NetworkListenerService.IsConnected && !string.IsNullOrEmpty(album.Name))
            {
                var bytes = await musicMdFetcher.GetAlbumPictureFromInternet(album.Name, album.Artist);
                if (bytes == null)
                {
                    // TODO: Add a TriedWithNoSuccess flag in DB
                }
                var success = bytes != null && await SaveAlbumImageAsync(album, bytes);
                if (success)
                {
                    Locator.MediaLibrary.Update(album);
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> GetArtistPicture(ArtistItem artist)
        {
            if (NetworkListenerService.IsConnected && !string.IsNullOrEmpty(artist.Name))
            {
                var bytes = await musicMdFetcher.GetArtistPicture(artist.Name);
                if (bytes == null)
                {
                    // TODO: Add a TriedWithNoSuccess flag in DB
                }
                var success = bytes != null && await SaveArtistImageAsync(artist, bytes);
                if (success)
                {
                    Locator.MediaLibrary.Update(artist);
                    return true;
                }
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
                await album.ResetAlbumArt();
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
                await artist.ResetArtistPicture(true);
                return true;
            }
            return false;
        }
    }
}
