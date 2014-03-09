using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using VLC_WINRT.ViewModels;
using Windows.Storage;
using Windows.Storage.Streams;
using XboxMusicLibrary;
using XboxMusicLibrary.Models;
using XboxMusicLibrary.Settings;

namespace VLC_WINRT.Utility.Helpers.MusicLibrary
{
    public class ArtistInformationsHelper
    {
        public Artist XBoxArtistItem;
        string _artist;
        public async Task<bool> GetArtistFromXboxMusic(string artist)
        {
            _artist = artist;
            try
            {
                Debug.WriteLine("Connecting to XBOX Music API for " + artist);
                Music XboxMusic;
                MusicHelper XboxMusicHelper = new MusicHelper();
                var token = await XboxMusicHelper.GetAccessToken("5bf9b614-1651-4b49-98ee-1831ae58fb99", "copuMsVkCAFLQlP38bV3y+Azysz/crELZ5NdQU7+ddg=");
                Debug.WriteLine("XBOX Music token " + token);
                XboxMusic = await XboxMusicHelper.SearchMediaCatalog(token, artist, null, 3);

                XBoxArtistItem = XboxMusic.Artists.Items.FirstOrDefault(x => x.Name == artist);
                if (XBoxArtistItem == null)
                    XBoxArtistItem = XboxMusic.Artists.Items.FirstOrDefault();

                Debug.WriteLine("XBOX Music artist found : " + XBoxArtistItem.Name);

                Locator.MusicLibraryVM.ImgCollection.Add(XBoxArtistItem.ImageUrl);
            }
            catch (Exception e)
            {
                Debug.WriteLine("XBOX Error\n" + e.ToString());
            }
            return XBoxArtistItem != null;
        }
        async Task SaveArtistThumbnailInAppFolder()
        {
            HttpClient clientPic = new HttpClient();
            HttpResponseMessage responsePic = await clientPic.GetAsync(XBoxArtistItem.ImageUrlWithOptions(new ImageSettings(280, 156, ImageMode.Scale, "")));
            byte[] img = await responsePic.Content.ReadAsByteArrayAsync();
            InMemoryRandomAccessStream streamWeb = new InMemoryRandomAccessStream();

            DataWriter writer = new DataWriter(streamWeb.GetOutputStreamAt(0));
            writer.WriteBytes(img);

            await writer.StoreAsync();

            string fileName = _artist + "_" + "dPi";

            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName + ".jpg", CreationCollisionOption.ReplaceExisting);
            var raStream = await file.OpenAsync(FileAccessMode.ReadWrite);

            using (var thumbnailStream = streamWeb.GetInputStreamAt(0))
            {
                using (var stream = raStream.GetOutputStreamAt(0))
                {
                    await RandomAccessStream.CopyAsync(thumbnailStream, stream);
                }
            }
        }
        public async Task<string> GetArtistPicture()
        {
            await SaveArtistThumbnailInAppFolder();
            return "ms-appdata:///local/" + _artist + "_" + "dPi" + ".jpg";
        }

        public async Task<List<ViewModels.MainPage.MusicLibraryViewModel.OnlineAlbumItem>> GetArtistTopAlbums()
        {
            Debug.WriteLine("Getting TopAlbums from XBOX Music API");
            HttpClient lastFmClient = new HttpClient();
            var response =
                await
                    lastFmClient.GetStringAsync(
                        "http://ws.audioscrobbler.com/2.0/?method=artist.gettopalbums&limit=8&api_key=" +
                        App.ApiKeyLastFm + "&artist=" + _artist);
            var xml = XDocument.Parse(response);
            var topAlbums = from results in xml.Descendants("album")
                            select new ViewModels.MainPage.MusicLibraryViewModel.OnlineAlbumItem
                            {
                                Name = results.Element("name").Value.ToString(),
                                Picture = results.Elements("image").ElementAt(3).Value,
                            };
            Debug.WriteLine("Receive TopAlbums from XBOX Music API");
            return topAlbums.ToList();
        }

        public async Task<List<ViewModels.MainPage.MusicLibraryViewModel.ArtistItemViewModel>> GetArtistSimilarsArtist()
        {
            try
            {
                HttpClient lastFmClient = new HttpClient();
                var response =
                    await
                        lastFmClient.GetStringAsync(
                            "http://ws.audioscrobbler.com/2.0/?method=artist.getsimilar&limit=8&api_key=" +
                            App.ApiKeyLastFm + "&artist=" + _artist);
                
                var xml = XDocument.Parse(response);
                var similarArtists = from results in xml.Descendants("artist")
                    select new ViewModels.MainPage.MusicLibraryViewModel.ArtistItemViewModel
                    {
                        Name = results.Element("name").Value.ToString(),
                        Picture = results.Elements("image").ElementAt(3).Value,
                    };
                return similarArtists.ToList();
            }
            catch
            {
                
            }
            return null;
        }
        public async Task<string> GetArtistBiography()
        {
            string Biography = "";
            try
            {
                HttpClient Bio = new HttpClient();
                var reponse = await Bio.GetStringAsync("http://ws.audioscrobbler.com/2.0/?method=artist.getinfo&api_key=" + App.ApiKeyLastFm + "&artist=" + _artist);
                {
                    var xml1 = XDocument.Parse(reponse);
                    var bio = xml1.Root.Descendants("bio").Elements("summary").FirstOrDefault();
                    if (bio != null)
                    {
                        // Deleting the html tags
                        Biography = Regex.Replace(bio.Value, "<.*?>", string.Empty);
                        if (Biography != null)
                        {
                            // Removes the "Read more about ... on last.fm" message
                            Biography = Biography.Remove(Biography.Length - "Read more about  on Last.fm".Length - _artist.Length - 6);
                        }
                        else
                        {
                            Biography = "It seems we did'nt found a biography for this artist.";
                        }
                    }
                    else
                    {
                        Biography = "It seems we did'nt found a biography for this artist.";
                    }
                }
            }
            catch
            {

            }
            return Biography;
        }
    }
}
// This code will be used later when XBOX Music will support all these queries (still no date of release though)

                //if (xBoxArtistItem.Albums != null)
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