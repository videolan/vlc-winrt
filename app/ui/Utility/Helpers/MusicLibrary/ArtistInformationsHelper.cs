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
using Newtonsoft.Json;
using VLC_WINRT.Common;
using VLC_WINRT.Model;
using VLC_WINRT.ViewModels;
using Windows.Storage;
using Windows.Storage.Streams;
using VLC_WINRT.ViewModels.MainPage;
using XboxMusicLibrary.Models;
using XboxMusicLibrary.Settings;
using Artist = XboxMusicLibrary.Models.Artist;

namespace VLC_WINRT.Utility.Helpers.MusicLibrary
{
    public static class ArtistInformationsHelper
    {
        public static async Task<Artist> GetArtistFromXboxMusic(string artist)
        {
            var xboxArtistItem = new Artist();
            try
            {
                // TODO: Client secret should be hidden. Hence why it's "secret"
                // TODO: Create interface for various music services used in VLC.
                if (Locator.MusicLibraryVM.XboxMusicAuthenication == null)
                {
                    Locator.MusicLibraryVM.XboxMusicAuthenication = await Locator.MusicLibraryVM.XboxMusicHelper.GetAccessToken("5bf9b614-1651-4b49-98ee-1831ae58fb99", "copuMsVkCAFLQlP38bV3y+Azysz/crELZ5NdQU7+ddg=", string.Empty);
                    Locator.MusicLibraryVM.XboxMusicAuthenication.StartTime = GetUnixTime(DateTime.Now);
                }
                var expiresIn = Convert.ToInt64(Locator.MusicLibraryVM.XboxMusicAuthenication.ExpiresIn);
                if (GetUnixTime(DateTime.Now) - Locator.MusicLibraryVM.XboxMusicAuthenication.StartTime >= expiresIn)
                {
                    Locator.MusicLibraryVM.XboxMusicAuthenication = await Locator.MusicLibraryVM.XboxMusicHelper.GetAccessToken("5bf9b614-1651-4b49-98ee-1831ae58fb99", "copuMsVkCAFLQlP38bV3y+Azysz/crELZ5NdQU7+ddg=", string.Empty);
                    Locator.MusicLibraryVM.XboxMusicAuthenication.StartTime = GetUnixTime(DateTime.Now);
                }


                Debug.WriteLine("Connecting to XBOX Music API for " + artist);
                Debug.WriteLine("XBOX Music token " + Locator.MusicLibraryVM.XboxMusicAuthenication);
                Music xboxMusic = await Locator.MusicLibraryVM.XboxMusicHelper.SearchMediaCatalog(Locator.MusicLibraryVM.XboxMusicAuthenication.AccessToken, artist, null, 3, new Filters[] { Filters.Artists });

                xboxArtistItem = xboxMusic.Artists.Items.FirstOrDefault(x => x.Name == artist) ??
                                 xboxMusic.Artists.Items.FirstOrDefault();

                Debug.WriteLine("XBOX Music artist found : " + xboxArtistItem.Name);

                Locator.MusicLibraryVM.ImgCollection.Add(xboxArtistItem.ImageUrl);
            }
            catch (Exception e)
            {
                Debug.WriteLine("XBOX Error\n" + e.ToString());
            }
            return xboxArtistItem ?? null;
        }

        public static long GetUnixTime(DateTime time)
        {
            time = time.ToUniversalTime();
            TimeSpan timeSpam = time - (new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local));
            return (long)timeSpam.TotalSeconds;
        }

        static async void DownloadPicFromDeezerToLocalFolder(MusicLibraryViewModel.ArtistItemViewModel artist)
        {
            HttpClient clientPic = new HttpClient();
            string json = await clientPic.GetStringAsync("http://api.deezer.com/search/artist?q=" + artist.Name);
            if(json == "{\"data\":[],\"total\":0}")
            {
                return;
            }

            DeezerArtistItem.RootObject deezerData = JsonConvert.DeserializeObject<DeezerArtistItem.RootObject>(json);
            Debug.WriteLine("Deezer picture for " + artist.Name + " : " + deezerData.data[0].picture);
            string imageUrl = string.Format("{0}?size=big", deezerData.data[0].picture);
            HttpResponseMessage responsePic = await clientPic.GetAsync(imageUrl);
            Locator.MusicLibraryVM.ImgCollection.Add(imageUrl);
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
        }

        public static async Task GetArtistPicture(MusicLibraryViewModel.ArtistItemViewModel artist)
        {
            StorageFolder appDataFolder = ApplicationData.Current.LocalFolder;
            string supposedPictureUriLocal = appDataFolder.Path + "\\artistPic\\" + artist.Name + "_" + "dPi" + ".jpg";
            if (NativeOperationsHelper.FileExist(supposedPictureUriLocal))
            {
                artist.Picture = supposedPictureUriLocal;
            }
            else
            {
                await DownloadPicFromDeezerToLocalFolder(artist);
            }
        }

        public static async Task GetArtistTopAlbums(MusicLibraryViewModel.ArtistItemViewModel artist)
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

        public static async Task GetArtistSimilarsArtist(MusicLibraryViewModel.ArtistItemViewModel artist)
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

        public static async Task GetArtistBiography(MusicLibraryViewModel.ArtistItemViewModel artist)
        {
            string biography = string.Empty;
            try
            {
                var lastFmClient = new LastFmClient();
                var artistInformation = await lastFmClient.GetArtistInfo(artist.Name);
                biography = artistInformation.Biograpgy;
            }
            catch
            {
                Debug.WriteLine("Failed to get artist biography from LastFM. Returning nothing.");
            }
            artist.Biography = System.Net.WebUtility.HtmlDecode(biography);
        }
    }
}
// This code will be used later when XBOX Music will support all these queries (still no date of release though)

//{
//    foreach (var album in xBoxArtistItem.Albums.Items)
//    {
//        artist.OnlinePopularAlbumItems.Add(new VLC_WINRT.ViewModels.MainPage.MusicLibraryViewModel.OnlineAlbumItem()
//        {
//            Artist = xBoxArtistItem.Name,
//            Name = album.Name,
//            Picture = album.ImageUrlWithOptions(new ImageSettings(200, 200, ImageMode.Scale, "")),
//        });
//    }
//    foreach (var artists in xBoxArtistItem.RelatedArtists.Items)
//    {
//        var onlinePopularAlbums = artists.Albums.Items.Select(albums => new VLC_WINRT.ViewModels.MainPage.MusicLibraryViewModel.OnlineAlbumItem
//        {
//            Artist = artists.Name,
//            Name = albums.Name,
//            Picture = albums.ImageUrlWithOptions(new ImageSettings(280, 156, ImageMode.Scale, "")),
//        }).ToList();

//        var artistPic = artists.ImageUrl;
//        artist.OnlineRelatedArtists.Add(new VLC_WINRT.ViewModels.MainPage.MusicLibraryViewModel.ArtistItemViewModel
//        {
//            Name = artists.Name,
//            OnlinePopularAlbumItems = onlinePopularAlbums,
//            Picture = artistPic,
//        });
//    }

//}
