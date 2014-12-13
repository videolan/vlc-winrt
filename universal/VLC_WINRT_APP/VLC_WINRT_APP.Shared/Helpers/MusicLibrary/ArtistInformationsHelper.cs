/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.UI.Core;
using Newtonsoft.Json.Linq;
using VLC_WINRT.Common;
using Windows.Storage;
using Windows.Storage.Streams;
using VLC_WINRT_APP.Common;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.ViewModels.MusicVM;
using WinRTXamlToolkit.IO.Extensions;

namespace VLC_WINRT_APP.Helpers.MusicLibrary
{
    public static class ArtistInformationsHelper
    {
        private static async Task<bool> DownloadArtistPictureFromDeezer(ArtistItem artist)
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
                string fileName = artist.Id.ToString();
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
                string supposedPictureUriLocal = appDataFolder.Path + "\\artistPic\\" + artist.Id + ".jpg";
                await DispatchHelper.InvokeAsync(() => artist.Picture = supposedPictureUriLocal);
                return true;
            }
            catch (Exception)
            {
                Debug.WriteLine("Error getting or saving art from deezer.");
                return false;
            }
        }
        private static async Task<bool> DownloadAlbumPictureFromDeezer(AlbumItem album)
        {
            var deezerClient = new DeezerClient();
            var deezerAlbum = await deezerClient.GetAlbumInfo(album.Name, album.Artist);
            if (deezerAlbum == null) return false;
            if (deezerAlbum.Images == null) return false;
            try
            {
                var clientPic = new HttpClient();
                string url = deezerAlbum.Images.Count == 1 ? deezerAlbum.Images[0].Url : deezerAlbum.Images[deezerAlbum.Images.Count - 2].Url;
                HttpResponseMessage responsePic = await clientPic.GetAsync(url);
                var uri = responsePic.RequestMessage.RequestUri.AbsoluteUri;
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

        private static async Task<bool> DownloadAlbumPictureFromLastFm(AlbumItem album)
        {
            var lastFmClient = new LastFmClient();
            var lastFmAlbum = await lastFmClient.GetAlbumInfo(album.Name, album.Artist);
            if (lastFmAlbum == null) return false;
            if (lastFmAlbum.Images == null || lastFmAlbum.Images.Count == 0) return false;
            try
            {
                if (string.IsNullOrEmpty(lastFmAlbum.Images.LastOrDefault().Url)) return false;
                var clientPic = new HttpClient();
                var url = lastFmAlbum.Images.Count == 1 ? lastFmAlbum.Images[0].Url : lastFmAlbum.Images[lastFmAlbum.Images.Count - 2].Url;
                HttpResponseMessage responsePic = await clientPic.GetAsync(url);
                byte[] img = await responsePic.Content.ReadAsByteArrayAsync();
                var result = await SaveAlbumImageAsync(album, img);
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error getting or saving art from lastFm. {0}", ex);
            }
            return false;
        }

        public static async Task GetArtistEvents(ArtistItem artist)
        {
            await DownloadArtistEventFromLastFm(artist);
        }

        private static async Task<bool> DownloadArtistEventFromLastFm(ArtistItem artist)
        {
            var lastFmClient = new LastFmClient();
            var lastfmArtistEvents = await lastFmClient.GetArtistEventInfo(artist.Name);
            if (lastfmArtistEvents == null) return false;
            try
            {
                var showItems = new List<ShowItem>();
                foreach (var show in lastfmArtistEvents.Shows)
                {
                    DateTime date;
                    ShowItem showItem = null;
                    bool tryParse = DateTime.TryParse(show.StartDate, out date);
                    if (tryParse)
                        showItem = new ShowItem(show.Title, date, show.Venue.Location.City, show.Venue.Location.Country);
                    else continue;
                    foreach (var artistShow in show.Artists.Artists)
                    {
                        // dirty hack
                        if (artistShow is JValue)
                            showItem.Artists.Add(artistShow.Value);
                    }
                    showItems.Add(showItem);
                }
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    artist.UpcomingShows = showItems;
                    artist.IsUpcomingShowsLoading = false;
                });
                return true;
            }
            catch (Exception exception)
            {
                LogHelper.Log("Error when trying to map from Events collection to Artist object for artist : " + artist.Name + " exceptio log " + exception.ToString());
            }
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                artist.IsUpcomingShowsLoading = false;
            });
            return false;
        }

        private static async Task<bool> DownloadArtistPictureFromLastFm(ArtistItem artist)
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
                string fileName = artist.Id.ToString();

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
                string supposedPictureUriLocal = appDataFolder.Path + "\\artistPic\\" + artist.Id + ".jpg";
                await DispatchHelper.InvokeAsync(() => artist.Picture = supposedPictureUriLocal);
                return true;
            }
            catch (Exception)
            {
                Debug.WriteLine("Error getting or saving art from LastFm.");
                return false;
            }
        }

        public static async Task GetArtistPicture(ArtistItem artist)
        {
            StorageFolder appDataFolder = ApplicationData.Current.LocalFolder;
            bool picFolderExist = await appDataFolder.ContainsFolderAsync("artistPic");
            StorageFolder picFolder = null;
            bool picExist = false;
            if (picFolderExist)
            {
                picFolder = await StorageFolder.GetFolderFromPathAsync(appDataFolder.Path + "\\artistPic\\");
                picExist = await picFolder.ContainsFileAsync(artist.Id + ".jpg");
            }

            if (picFolderExist && picExist)
            {
                await DispatchHelper.InvokeAsync(() =>
                {
                    artist.Picture = picFolder.Path + "\\" + artist.Id + ".jpg";
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

        public static async Task GetAlbumPicture(AlbumItem album)
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()) return;
            await GetAlbumPictureFromInternet(album);
        }

        public static async Task GetAlbumPicture(TrackItem track)
        {
            await DispatchHelper.InvokeAsync(() =>
            {
                track.Thumbnail = "ms-appdata:///local/albumPic/" + track.AlbumId + ".jpg";
            });
        }

        public static async Task GetAlbumPictureFromInternet(AlbumItem album)
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
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                album.IsPictureLoaded = true;
                album.Picture = "ms-appdata:///local/albumPic/" + album.Id + ".jpg";
            });
            await MusicLibraryVM._albumDataRepository.Update(album);
        }

        public static async Task GetArtistTopAlbums(ArtistItem artist)
        {
            try
            {
                Debug.WriteLine("Getting TopAlbums from LastFM API");
                var lastFmClient = new LastFmClient();
                var albums = await lastFmClient.GetArtistTopAlbums(artist.Name);
                Debug.WriteLine("Receive TopAlbums from LastFM API");
                if (albums != null)
                {
                    await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        artist.OnlinePopularAlbumItems = albums;
                        artist.IsOnlinePopularAlbumItemsLoaded = true;
                    });
                }
            }
            catch
            {
                Debug.WriteLine("Error getting top albums from artist.");
            }
        }

        public static async Task GetArtistSimilarsArtist(ArtistItem artist)
        {
            try
            {
                var lastFmClient = new LastFmClient();
                var similarArtists = await lastFmClient.GetSimilarArtists(artist.Name);
                if (similarArtists != null)
                {
                    await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        artist.OnlineRelatedArtists = similarArtists;
                        artist.IsOnlineRelatedArtistsLoaded = true;
                    });
                }
            }
            catch
            {
                Debug.WriteLine("Error getting similar artists from this artist.");
            }
        }

        public static async Task GetArtistBiography(ArtistItem artist)
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
            if (!string.IsNullOrEmpty(biography))
                await
                    App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () => artist.Biography = System.Net.WebUtility.HtmlDecode(biography));
            else await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => artist.Biography = "ERROR");
        }

        public static async Task<bool> SaveAlbumImageAsync(AlbumItem album, byte[] img)
        {
            try
            {
                var streamWeb = new InMemoryRandomAccessStream();
                var writer = new DataWriter(streamWeb.GetOutputStreamAt(0));
                writer.WriteBytes(img);
                await writer.StoreAsync();
                var albumPic = await ApplicationData.Current.LocalFolder.CreateFolderAsync("albumPic",
                    CreationCollisionOption.OpenIfExists);

                var fileName = album.Id;
                var file = await albumPic.CreateFileAsync(fileName + ".jpg", CreationCollisionOption.OpenIfExists);
                var raStream = await file.OpenAsync(FileAccessMode.ReadWrite);

                using (var thumbnailStream = streamWeb.GetInputStreamAt(0))
                {
                    using (var stream = raStream.GetOutputStreamAt(0))
                    {
                        await RandomAccessStream.CopyAsync(thumbnailStream, stream);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                Debug.WriteLine("Error saving album art");
                return false;
            }
        }
    }
}
