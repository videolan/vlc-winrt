/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Web.Http;
using System.Threading.Tasks;
using VLC_WINRT_APP.Helpers.MusicLibrary.LastFm;
using VLC_WINRT_APP;
using Album = VLC_WINRT_APP.Helpers.MusicLibrary.MusicEntities.Album;
using Artist = VLC_WINRT_APP.Helpers.MusicLibrary.MusicEntities.Artist;


namespace VLC_WINRT_APP.Helpers.MusicLibrary
{
    public class LastFmClient : IMusicInformationManager
    {
        public async Task<Artist> GetArtistEventInfo(string artistName)
        {
            try
            {
                var lastFmClient = new HttpClient();
                string url =
                    string.Format(
                        "http://ws.audioscrobbler.com/2.0/?method=artist.getevents&artist={1}&api_key={0}&format=json&pretty=true",
                        App.ApiKeyLastFm, artistName);
                var reponse = await lastFmClient.GetStringAsync(new Uri(url));
                var artistEventInfo = JsonConvert.DeserializeObject<ArtistEventInformation>(reponse);
                if (artistEventInfo == null) return null;
                if (artistEventInfo.Events == null) return null;
                if (artistEventInfo.Events.Event == null) return null;
                var artist = new Artist();
                artist.MapFrom(artistEventInfo);
                return artist;
            }
            catch (Exception e)
            {
                LogHelper.Log("Error when trying to get Shows for artist : " + artistName + " exception log " + e.ToString());
            }
            return null;
        }

        public async Task<Artist> GetArtistInfo(string artistName)
        {
            try
            {
                var lastFmClient = new HttpClient();

                // Get users language/region
                // If their region is not support by LastFM, it won't return an artist biography.
                var region = new Windows.Globalization.GeographicRegion();
                string regionCode = "en";
                // LastFM does not take in normal windows region codes as valid language values.
                // We must set them ourselves.
                switch (Windows.System.UserProfile.GlobalizationPreferences.Languages.First())
                {
                    case "en-US":
                        regionCode = "en";
                        break;
                    case "ja":
                        regionCode = "jp";
                        break;
                }
                string url = string.Format("http://ws.audioscrobbler.com/2.0/?method=artist.getinfo&artist={1}&api_key={0}&lang={2}&format=json", App.ApiKeyLastFm, artistName, regionCode);
                var reponse = await lastFmClient.GetStringAsync(new Uri(url));
                {
                    var artistInfo = JsonConvert.DeserializeObject<ArtistInformation>(reponse);
                    if (artistInfo == null) return null;
                    if (artistInfo.Artist == null) return null;
                    var artist = new Artist();
                    artist.MapFrom(artistInfo);
                    return artist;
                }
            }
            catch (Exception exception)
            {
                LogHelper.Log("Failed to get artist biography from LastFM. Returning nothing." + exception.ToString());
            }
            return null;
        }

        public async Task<List<Artist>> GetSimilarArtists(string artistName)
        {
            try
            {
                var lastFmClient = new HttpClient();
                var response =
                    await
                        lastFmClient.GetStringAsync(new Uri(string.Format("http://ws.audioscrobbler.com/2.0/?method=artist.getsimilar&format=json&limit=8&api_key={0}&artist={1}", App.ApiKeyLastFm, artistName)));
                var artists = JsonConvert.DeserializeObject<SimilarArtistInformation>(response);
                if (artists == null || artists.Similarartists == null || !artists.Similarartists.Artist.Any()) return null;
                var similarArtists = artists.Similarartists.Artist;
                var artistList = new List<Artist>();
                foreach (var similarArtist in similarArtists)
                {
                    var artist = new Artist();
                    artist.MapFrom(similarArtist);
                    artistList.Add(artist);
                }
                return artistList;
            }
            catch
            {
                LogHelper.Log("Error getting similar artists from this artist.");
            }
            return null;
        }

        public async Task<List<Artist>> GetTopArtistsGenre(string genre)
        {
            var lastFmClient = new HttpClient();
            var response =
                await
                    lastFmClient.GetStringAsync(
                        new Uri(
                            string.Format(
                                "http://ws.audioscrobbler.com/2.0/?method=tag.gettopartists&tag={1}&api_key={0}&format=json",
                                App.ApiKeyLastFm, genre)));
            var topartists = JsonConvert.DeserializeObject<TopArtistInformation>(response);
            var artistList = new List<Artist>();
            foreach (var topArtistArtist in topartists.topartists.artist)
            {
                var artist = new Artist();
                artist.Name = topArtistArtist.name;
                artistList.Add(artist);
            }
            return artistList;
        }

        public async Task<Album> GetAlbumInfo(string albumTitle, string artistName)
        {
            try
            {
                var lastFmClient = new HttpClient();
                // Get users language/region
                // If their region is not support by LastFM, it won't return an artist biography.
                var region = new Windows.Globalization.GeographicRegion();
                string url = string.Format(
                    "http://ws.audioscrobbler.com/2.0/?method=album.getinfo&artist={1}&album={3}&api_key={0}&format=json&lang={2}",
                    App.ApiKeyLastFm, artistName, region.Code.ToLower(), albumTitle);
                var reponse = await lastFmClient.GetStringAsync(new Uri(url));
                {
                    var albumInfo = JsonConvert.DeserializeObject<AlbumInformation>(reponse);
                    if (albumInfo == null) return null;
                    if (albumInfo.Album == null) return null;
                    var album = new Album();
                    album.MapFrom(albumInfo.Album);
                    return album;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log(string.Format("Failed to get artist biography from LastFM. Returning nothing. {0}", ex));
            }
            return null;
        }

        /// <summary>
        /// Retreve a collection of top albums by an artist via LastFmClient.
        /// </summary>
        /// <param name="name">The artists name.</param>
        /// <returns>A list of Albums.</returns>
        public async Task<List<Album>> GetArtistTopAlbums(string name)
        {
            try
            {
                LogHelper.Log("Getting TopAlbums from LastFM API");
                var lastFmClient = new HttpClient();
                var response =
                    await
                        lastFmClient.GetStringAsync(new Uri(string.Format("http://ws.audioscrobbler.com/2.0/?method=artist.gettopalbums&limit=8&format=json&api_key={0}&artist={1}", App.ApiKeyLastFm, name)));
                var albums = JsonConvert.DeserializeObject<TopAlbumInformation>(response);
                LogHelper.Log("Receive TopAlbums from LastFM API");
                if (albums == null) return null;
                var albumList = albums.TopAlbums.Album;
                if (albumList == null) return null;
                var formattedAlbumList = new List<Album>();
                foreach (var topAlbum in albumList)
                {
                    var album = new Album();
                    album.MapFrom(topAlbum);
                    formattedAlbumList.Add(album);
                }
                return formattedAlbumList;
            }
            catch
            {
                LogHelper.Log("Error getting top albums from artist.");
            }
            return null;
        }
    }
}
