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
using System.Net.Http;
using System.Threading.Tasks;
using VLC_WINRT_APP.Helpers.MusicLibrary.LastFm;
using VLC_WINRT_APP;
using Album = VLC_WINRT_APP.Helpers.MusicLibrary.MusicEntities.Album;
using Artist = VLC_WINRT_APP.Helpers.MusicLibrary.MusicEntities.Artist;


namespace VLC_WINRT_APP.Helpers.MusicLibrary
{
    public class LastFmClient : IMusicInformationManager
    {
        public async Task<Artist> GetArtistInfo(string artistName)
        {
            try
            {
                var lastFmClient = new HttpClient();
                // Get users language/region
                // If their region is not support by LastFM, it won't return an artist biography.
                var region = new Windows.Globalization.GeographicRegion();
                string url =
                    string.Format(
 "http://ws.audioscrobbler.com/2.0/?method=artist.getinfo&artist={1}&api_key={0}&format=json&lang={2}",
                        App.ApiKeyLastFm, artistName, region.Code.ToLower());
                var reponse = await lastFmClient.GetStringAsync(url);
                {
                    var artistInfo = JsonConvert.DeserializeObject<ArtistInformation>(reponse);
                    if (artistInfo == null) return null;
                    if (artistInfo.Artist == null) return null;
                    var artist = new Artist();
                    artist.MapFrom(artistInfo);
                    return artist;
                }
            }
            catch
            {
                Debug.WriteLine("Failed to get artist biography from LastFM. Returning nothing.");
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
                        lastFmClient.GetStringAsync(
                            string.Format("http://ws.audioscrobbler.com/2.0/?method=artist.getsimilar&format=json&limit=8&api_key={0}&artist={1}", App.ApiKeyLastFm, artistName));
                var artists = JsonConvert.DeserializeObject<SimilarArtistInformation>(response);
                if (artists == null || !artists.Similarartists.Artist.Any()) return null;
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
                Debug.WriteLine("Error getting similar artists from this artist.");
            }
            return null;
        }

        public Task<Album> GetAlbumInfo(string albumTitle)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retreve a collection of top albums by an artist via LastFmClient.
        /// </summary>
        /// <param name="name">The artists name.</param>
        /// <returns>A list of Albums.</returns>
        public async Task<List<Album>>  GetArtistTopAlbums(string name)
        {
            try
            {

                Debug.WriteLine("Getting TopAlbums from LastFM API");
                var lastFmClient = new HttpClient();
                var response =
                    await
                        lastFmClient.GetStringAsync(
                            string.Format("http://ws.audioscrobbler.com/2.0/?method=artist.gettopalbums&limit=8&format=json&api_key={0}&artist={1}", App.ApiKeyLastFm, name));
                var albums = JsonConvert.DeserializeObject<TopAlbumInformation>(response);
                Debug.WriteLine("Receive TopAlbums from LastFM API");
                if (albums == null) return null;
                var albumList = albums.TopAlbums.Album;
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
                Debug.WriteLine("Error getting top albums from artist.");
            }
            return null;
        }
    }
}
