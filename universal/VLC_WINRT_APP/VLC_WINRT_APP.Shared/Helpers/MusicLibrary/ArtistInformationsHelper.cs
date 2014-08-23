/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using VLC_WINRT.Common;
using Windows.Storage;
using Windows.Storage.Streams;
using VLC_WINRT_APP.Common;
using VLC_WINRT_APP.ViewModels.MusicVM;

namespace VLC_WINRT_APP.Helpers.MusicLibrary
{
    public static class ArtistInformationsHelper
    {
        private static async Task<bool> DownloadArtistPictureFromDeezer(MusicLibraryVM.ArtistItem artist)
        {
            var deezerClient = new DeezerClient();
            var deezerArtist = await deezerClient.GetArtistInfo(artist.Name);
            if (deezerArtist == null) return false;
            if (deezerArtist.Images == null) return false;
            try
            {
                var clientPic = new HttpClient();
                HttpResponseMessage responsePic = await clientPic.GetAsync(deezerArtist.Images.LastOrDefault().Url);
                string uri = responsePic.RequestMessage.RequestUri.AbsoluteUri;
                // A cheap hack to avoid using Deezers default image for bands.
                if (uri.Equals("http://cdn-images.deezer.com/images/artist//400x400-000000-80-0-0.jpg"))
                {
                    return false;
                }
                byte[] img = await responsePic.Content.ReadAsByteArrayAsync();
                InMemoryRandomAccessStream streamWeb = new InMemoryRandomAccessStream();
                DataWriter writer = new DataWriter(streamWeb.GetOutputStreamAt(0));
                writer.WriteBytes(img);
                await writer.StoreAsync();
                StorageFolder artistPic = await ApplicationData.Current.LocalFolder.CreateFolderAsync("artistPic",
                    CreationCollisionOption.OpenIfExists);
                string fileName = artist.Name + "_" + "dPi";
                var file = await artistPic.CreateFileAsync(fileName + ".jpg", CreationCollisionOption.OpenIfExists);
                var raStream = await file.OpenAsync(FileAccessMode.ReadWrite);

                using (var thumbnailStream = streamWeb.GetInputStreamAt(0))
                {
                    using (var stream = raStream.GetOutputStreamAt(0))
                    {
                        await RandomAccessStream.CopyAsync(thumbnailStream, stream);
                    }
                }
                StorageFolder appDataFolder = ApplicationData.Current.LocalFolder;
                string supposedPictureUriLocal = appDataFolder.Path + "\\artistPic\\" + artist.Name + "_" + "dPi" + ".jpg";
                await DispatchHelper.InvokeAsync(() => artist.Picture = supposedPictureUriLocal);
                return true;
            }
            catch (Exception)
            {
                Debug.WriteLine("Error getting or saving art from deezer.");
                return false;
            }
        }
        private static async Task<bool> DownloadAlbumPictureFromDeezer(MusicLibraryVM.AlbumItem album)
        {
            var deezerClient = new DeezerClient();
            var deezerAlbum = await deezerClient.GetAlbumInfo(album.Name, album.Artist);
            if (deezerAlbum == null) return false;
            if (deezerAlbum.Images == null) return false;
            try
            {
                var clientPic = new HttpClient();
                HttpResponseMessage responsePic = await clientPic.GetAsync(deezerAlbum.Images.LastOrDefault().Url);
                string uri = responsePic.RequestMessage.RequestUri.AbsoluteUri;
                // A cheap hack to avoid using Deezers default image for bands.
                if (uri.Equals("http://cdn-images.deezer.com/images/album//400x400-000000-80-0-0.jpg"))
                {
                    return false;
                }
                byte[] img = await responsePic.Content.ReadAsByteArrayAsync();
                var result = await SaveAlbumImageAsync(album, img);
                return result;
            }
            catch (Exception)
            {
                Debug.WriteLine("Error getting or saving art from deezer.");
                return false;
            }
        }
        private static async Task<bool> DownloadAlbumPictureFromLastFm(MusicLibraryVM.AlbumItem album)
        {
            var lastFmClient = new LastFmClient();
            var lastFmAlbum = await lastFmClient.GetAlbumInfo(album.Name, album.Artist);
            if (lastFmAlbum == null) return false;
            if (lastFmAlbum.Images == null || lastFmAlbum.Images.Count == 0) return false;
            try
            {
                if (string.IsNullOrEmpty(lastFmAlbum.Images.LastOrDefault().Url)) return false;
                var clientPic = new HttpClient();
                HttpResponseMessage responsePic = await clientPic.GetAsync(lastFmAlbum.Images.LastOrDefault().Url);
                byte[] img = await responsePic.Content.ReadAsByteArrayAsync();
                var result = await SaveAlbumImageAsync(album, img);
                return result;
            }
            catch (Exception)
            {
                Debug.WriteLine("Error getting or saving art from lastFm.");
                return false;
            }
            return false;
        }

        private static async Task<bool> DownloadArtistPictureFromLastFm(MusicLibraryVM.ArtistItem artist)
        {
            var lastFmClient = new LastFmClient();
            var lastFmArtist = await lastFmClient.GetArtistInfo(artist.Name);
            if (lastFmArtist == null) return false;
            try
            {
                var clientPic = new HttpClient();
                var imageElement = lastFmArtist.Images.LastOrDefault(node => !string.IsNullOrEmpty(node.Url));
                if (imageElement == null) return false;
                HttpResponseMessage responsePic = await clientPic.GetAsync(imageElement.Url);
                byte[] img = await responsePic.Content.ReadAsByteArrayAsync();
                InMemoryRandomAccessStream streamWeb = new InMemoryRandomAccessStream();

                DataWriter writer = new DataWriter(streamWeb.GetOutputStreamAt(0));
                writer.WriteBytes(img);

                await writer.StoreAsync();

                StorageFolder artistPic = await ApplicationData.Current.LocalFolder.CreateFolderAsync("artistPic",
                    CreationCollisionOption.OpenIfExists);
                string fileName = artist.Name + "_" + "dPi";

                var file = await artistPic.CreateFileAsync(fileName + ".jpg", CreationCollisionOption.OpenIfExists);
                var raStream = await file.OpenAsync(FileAccessMode.ReadWrite);

                using (var thumbnailStream = streamWeb.GetInputStreamAt(0))
                {
                    using (var stream = raStream.GetOutputStreamAt(0))
                    {
                        await RandomAccessStream.CopyAsync(thumbnailStream, stream);
                    }
                }
                StorageFolder appDataFolder = ApplicationData.Current.LocalFolder;
                string supposedPictureUriLocal = appDataFolder.Path + "\\artistPic\\" + artist.Name + "_" + "dPi" + ".jpg";
                DispatchHelper.InvokeAsync(() => artist.Picture = supposedPictureUriLocal);
                return true;
            }
            catch (Exception)
            {
                Debug.WriteLine("Error getting or saving art from LastFm.");
                return false;
            }
        }

        public static async Task GetArtistPicture(MusicLibraryVM.ArtistItem artist)
        {
            StorageFolder appDataFolder = ApplicationData.Current.LocalFolder;
            string supposedPictureUriLocal = appDataFolder.Path + "\\artistPic\\" + artist.Name + "_" + "dPi" + ".jpg";
            if (await NativeOperationsHelper.FileExist(supposedPictureUriLocal))
            {
                DispatchHelper.InvokeAsync(() =>
                {
                    artist.Picture = supposedPictureUriLocal;
                });
            }
            else
            {
                var gotArt = await DownloadArtistPictureFromDeezer(artist);
                if (!gotArt)
                {
                    await DownloadArtistPictureFromLastFm(artist);
                }
            }
        }
        public static async Task GetAlbumPicture(MusicLibraryVM.AlbumItem album)
        {
            StorageFolder appDataFolder = ApplicationData.Current.LocalFolder;
            string supposedPictureUriLocal = appDataFolder.Path + "\\albumPic\\" + album.Name + "_" + "dPi" + ".jpg";
            if (await NativeOperationsHelper.FileExist(supposedPictureUriLocal))
            {
                DispatchHelper.InvokeAsync(() =>
                {
                    album.Picture = supposedPictureUriLocal;
                });
            }
            else
            {
                var gotArt = await DownloadAlbumPictureFromLastFm(album);
                if (!gotArt)
                {
                   gotArt = await DownloadAlbumPictureFromDeezer(album);
                }

                // If we still could not find album art, set it to the default cover.
                // This way, we won't keep pinging the APIs for album art that may not exist.
                if (!gotArt)
                {
                    StorageFolder install = Windows.ApplicationModel.Package.Current.InstalledLocation;
                    StorageFile file = await install.GetFileAsync("Assets\\NoCover.jpg");
                    var bytes = await ConvertImage.ConvertImagetoByte(file);
                    await SaveAlbumImageAsync(album, bytes);
                }
            }
        }

        public static async Task GetArtistTopAlbums(MusicLibraryVM.ArtistItem artist)
        {
            try
            {
                Debug.WriteLine("Getting TopAlbums from LastFM API");
                var lastFmClient = new LastFmClient();
                var albums = await lastFmClient.GetArtistTopAlbums(artist.Name);
                Debug.WriteLine("Receive TopAlbums from LastFM API");
                if (albums != null)
                {
                    artist.OnlinePopularAlbumItems = albums;
                    artist.IsOnlinePopularAlbumItemsLoaded = true;
                }
            }
            catch
            {
                Debug.WriteLine("Error getting top albums from artist.");
            }
        }

        public static async Task GetArtistSimilarsArtist(MusicLibraryVM.ArtistItem artist)
        {
            try
            {
                var lastFmClient = new LastFmClient();
                var similarArtists = await lastFmClient.GetSimilarArtists(artist.Name);
                if (similarArtists != null)
                {
                    artist.OnlineRelatedArtists = similarArtists;
                    artist.IsOnlineRelatedArtistsLoaded = true;
                }
            }
            catch
            {
                Debug.WriteLine("Error getting similar artists from this artist.");
            }
        }

        public static async Task GetArtistBiography(MusicLibraryVM.ArtistItem artist)
        {
            string biography = string.Empty;
            try
            {
                var lastFmClient = new LastFmClient();
                var artistInformation = await lastFmClient.GetArtistInfo(artist.Name);
                biography = artistInformation.Biography;
            }
            catch
            {
                Debug.WriteLine("Failed to get artist biography from LastFM. Returning nothing.");
            }
            artist.Biography = System.Net.WebUtility.HtmlDecode(biography);
        }

        public static async Task<bool> SaveAlbumImageAsync(MusicLibraryVM.AlbumItem album, byte[] img)
        {
            try
            {
                var streamWeb = new InMemoryRandomAccessStream();
                var writer = new DataWriter(streamWeb.GetOutputStreamAt(0));
                writer.WriteBytes(img);
                await writer.StoreAsync();
                var albumPic = await ApplicationData.Current.LocalFolder.CreateFolderAsync("albumPic",
                    CreationCollisionOption.OpenIfExists);

                var fileName = MakeValidFileName(album.Name) + "_" + "dPi";
                var file = await albumPic.CreateFileAsync(fileName + ".jpg", CreationCollisionOption.OpenIfExists);
                var raStream = await file.OpenAsync(FileAccessMode.ReadWrite);

                using (var thumbnailStream = streamWeb.GetInputStreamAt(0))
                {
                    using (var stream = raStream.GetOutputStreamAt(0))
                    {
                        await RandomAccessStream.CopyAsync(thumbnailStream, stream);
                    }
                }
                await GetAlbumPicture(album);
                return true;
            }
            catch (Exception)
            {
                Debug.WriteLine("Error saving album art");
                return false;
            }
        }

        private static string MakeValidFileName(string name)
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
        }
    }
}
