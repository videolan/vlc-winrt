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
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using VLC_WinRT.MusicMetaFetcher.Fetchers;
using VLC_WinRT.MusicMetaFetcher.Models.MusicEntities;

namespace VLC_WinRT.MusicMetaFetcher
{
    public class MusicMDFetcher
    {
        public static string DeezerAppId;
        public static string ApiKeyLastFm;

        public MusicMDFetcher(string deezerAppId, string lastFmApiKey)
        {
            DeezerAppId = deezerAppId;
            ApiKeyLastFm = lastFmApiKey;
        }

        public async Task<List<Show>>  GetArtistEvents(string artistName)
        {
            var shows = await DownloadArtistEventFromLastFm(artistName);
            return shows;
        }

        private async Task<byte[]> DownloadArtistPictureFromDeezer(string artistName)
        {
            var deezerClient = new DeezerClient();
            var deezerArtist = await deezerClient.GetArtistInfo(artistName);
            if (deezerArtist == null) return null;
            if (deezerArtist.Images == null) return null;
            try
            {
                var clientPic = new HttpClient();
                HttpResponseMessage responsePic = await clientPic.GetAsync(deezerArtist.Images.LastOrDefault().Url);
                string uri = responsePic.RequestMessage.RequestUri.AbsoluteUri;
                // A cheap hack to avoid using Deezers default image for bands.
                if (uri.Equals("http://cdn-images.deezer.com/images/artist//400x400-000000-80-0-0.jpg"))
                {
                    return null;
                }
                byte[] img = await responsePic.Content.ReadAsByteArrayAsync();
                return img;
            }
            catch (Exception)
            {
                Debug.WriteLine("Error getting or saving art from deezer.");
                return null;
            }
        }

        private async Task<byte[]> DownloadArtistPictureFromLastFm(string artistName)
        {
            var lastFmClient = new LastFmClient();
            var lastFmArtist = await lastFmClient.GetArtistInfo(artistName);
            if (lastFmArtist == null) return null;
            try
            {
                var clientPic = new HttpClient();
                var imageElement = lastFmArtist.Images.LastOrDefault(node => !string.IsNullOrEmpty(node.Url));
                if (imageElement == null) return null;
                HttpResponseMessage responsePic = await clientPic.GetAsync(imageElement.Url);
                byte[] img = await responsePic.Content.ReadAsByteArrayAsync();
                return img;
            }
            catch (Exception)
            {
                Debug.WriteLine("Error getting or saving art from LastFm.");
                return null;
            }
        }

        private async Task<byte[]> DownloadAlbumPictureFromDeezer(string albumName, string albumArtist)
        {
            var deezerClient = new DeezerClient();
            var deezerAlbum = await deezerClient.GetAlbumInfo(albumName, albumArtist);
            if (deezerAlbum == null) return null;
            if (deezerAlbum.Images == null) return null;
            try
            {
                var clientPic = new HttpClient();
                string url = deezerAlbum.Images.Count == 1 ? deezerAlbum.Images[0].Url : deezerAlbum.Images[deezerAlbum.Images.Count - 2].Url;
                HttpResponseMessage responsePic = await clientPic.GetAsync(url);
                var uri = responsePic.RequestMessage.RequestUri.AbsoluteUri;
                // A cheap hack to avoid using Deezers default image for bands.
                if (uri.Equals("http://cdn-images.deezer.com/images/album//400x400-000000-80-0-0.jpg"))
                {
                    return null;
                }
                byte[] img = await responsePic.Content.ReadAsByteArrayAsync();
                return img;
            }
            catch (Exception)
            {
                Debug.WriteLine("Error getting or saving art from deezer.");
                return null;
            }
        }

        private async Task<byte[]> DownloadAlbumPictureFromLastFm(string albumName, string albumArtist)
        {
            var lastFmClient = new LastFmClient();
            var lastFmAlbum = await lastFmClient.GetAlbumInfo(albumName, albumArtist);
            if (lastFmAlbum == null) return null;
            if (lastFmAlbum.Images == null || lastFmAlbum.Images.Count == 0) return null;
            try
            {
                if (string.IsNullOrEmpty(lastFmAlbum.Images.LastOrDefault().Url)) return null;
                var clientPic = new HttpClient();
                var url = lastFmAlbum.Images.Count == 1 ? lastFmAlbum.Images[0].Url : lastFmAlbum.Images[lastFmAlbum.Images.Count - 2].Url;
                HttpResponseMessage responsePic = await clientPic.GetAsync(url);
                byte[] img = await responsePic.Content.ReadAsByteArrayAsync();
                return img;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Error getting or saving art from lastFm. {0}", ex));
            }
            return null;
        }

        private async Task<List<Show>> DownloadArtistEventFromLastFm(string artistName)
        {
            var lastFmClient = new LastFmClient();
            var lastfmArtistEvents = await lastFmClient.GetArtistEventInfo(artistName);
            if (lastfmArtistEvents == null) return null;
            try
            {
                var shows = new List<Show>();
                foreach (var show in lastfmArtistEvents.Shows)
                {
                    DateTime date;
                    Show Show = null;
                    bool tryParse = DateTime.TryParse(show.StartDate, out date);
                    if (tryParse)
                    {
                        if (show.Venue.Location.GeoPoint != null && show.Venue.Location.GeoPoint.Latitude != null &&
                            show.Venue.Location.GeoPoint.Longitute != null)
                        {
                            Show = new Show(show.Title, date, show.Venue.Location.City, show.Venue.Location.Country, show.Venue.Location.GeoPoint.Latitude, show.Venue.Location.GeoPoint.Longitute);
                        }
                        else
                        {
                            Show = new Show(show.Title, date, show.Venue.Location.City, show.Venue.Location.Country);
                        }
                    }
                    else continue;
                    foreach (var artistShow in show.Artists.Artists)
                    {
                        // dirty hack
                        if (artistShow is JValue)
                            Show.Artists.Add(artistShow.Value);
                    }
                    shows.Add(Show);
                }
                return shows;
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Error when trying to map from Events collection to Artist object for artist : " + artistName + " exceptio log " + exception.ToString());
            }
            return null;
        }

        public async Task<byte[]> GetArtistPicture(string artistName)
        {
            var gotArt = await DownloadArtistPictureFromDeezer(artistName) ?? await DownloadArtistPictureFromLastFm(artistName);
            return gotArt;
        }

        public async Task<byte[]> GetAlbumPictureFromInternet(string albumName, string albumArtist)
        {
            byte[] gotArt = null;
            if (!string.IsNullOrEmpty(albumArtist) && !string.IsNullOrEmpty(albumName))
            {
                gotArt = await DownloadAlbumPictureFromLastFm(albumName, albumArtist);
                if (gotArt == null)
                {
                    gotArt = await DownloadAlbumPictureFromDeezer(albumName, albumArtist);
                }
            }
            return gotArt;
        }

        public async Task<List<Album>>  GetArtistTopAlbums(string artistName)
        {
            try
            {
                if (string.IsNullOrEmpty(artistName)) return null;
                Debug.WriteLine("Getting TopAlbums from LastFM API");
                var lastFmClient = new LastFmClient();
                var albums = await lastFmClient.GetArtistTopAlbums(artistName);
                Debug.WriteLine("Receive TopAlbums from LastFM API");
                return albums;
            }
            catch
            {
                Debug.WriteLine("Error getting top albums from artist.");
            }
            return null;
        }

        public async Task<List<Artist>>  GetArtistSimilarsArtist(string artistName)
        {
            try
            {
                if (string.IsNullOrEmpty(artistName)) return null;
                var lastFmClient = new LastFmClient();
                var similarArtists = await lastFmClient.GetSimilarArtists(artistName);
                return similarArtists;
            }
            catch
            {
                Debug.WriteLine("Error getting similar artists from this artist.");
            }
            return null;
        }

        public async Task<List<Artist>>  GetTopArtistGenre(string genre)
        {
            try
            {
                if (string.IsNullOrEmpty(genre)) return null;
                var lastFmClient = new LastFmClient();
                var artists = await lastFmClient.GetTopArtistsGenre(genre);
                return artists;
            }
            catch
            {
                
            }
            return null;
        }

        public async Task<string> GetArtistBiography(string artistName)
        {
            if (string.IsNullOrEmpty(artistName)) return null;
            var biography = string.Empty;
            try
            {
                var lastFmClient = new LastFmClient();
                var artistInformation = await lastFmClient.GetArtistInfo(artistName);
                biography = artistInformation != null ? artistInformation.Biography : String.Empty;
            }
            catch
            {
                Debug.WriteLine("Failed to get artist biography from LastFM. Returning nothing.");
            }
            return biography;
        }


    }
}
