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
using System.Threading.Tasks;
using Windows.Web.Http;
using Newtonsoft.Json;
using VLC_WinRT.MusicMetaFetcher.Models.Deezer;
using Album = VLC_WinRT.MusicMetaFetcher.Models.MusicEntities.Album;
using Artist = VLC_WinRT.MusicMetaFetcher.Models.MusicEntities.Artist;

namespace VLC_WinRT.MusicMetaFetcher.Fetchers
{
    public class DeezerClient : IMusicMetaFetcher
    {
        public async Task<Artist> GetArtistInfo(string artistName)
        {
            try
            {
                var deezerClient = new HttpClient();
                string json = await deezerClient.GetStringAsync(new Uri(string.Format("http://api.deezer.com/search/artist?q={0}&appid={1}", System.Net.WebUtility.HtmlEncode(artistName), MusicMDFetcher.DeezerAppId)));
                // TODO: See if this is even needed. It should just map an empty object.
                if (json == "{\"data\":[],\"total\":0}")
                {
                    return null;
                }
                var deezerArtists = JsonConvert.DeserializeObject<Artists>(json);
                if (deezerArtists.Data != null && deezerArtists.Total > 0)
                {
                    var deezerArtist = deezerArtists.Data.FirstOrDefault();
                    var artist = new Artist();
                    artist.MapFrom(deezerArtist);
                    return artist;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("DeezerClient.GetArtistInfo", e);
            }
            return null;
        }

        public async Task<List<Artist>> GetSimilarArtists(string artistId)
        {
            try
            {
                var deezerClient = new HttpClient();
                string json = await deezerClient.GetStringAsync(new Uri(string.Format("http://api.deezer.com/artist/{0}/related?appid={1}", artistId, MusicMDFetcher.DeezerAppId)));
                var deezerArtists = JsonConvert.DeserializeObject<Artists>(json);
                var artistList = new List<Artist>();
                if (deezerArtists.Data != null)
                {
                    foreach (var deezerArtist in deezerArtists.Data)
                    {
                        var artist = new Artist();
                        artist.MapFrom(deezerArtist);
                        artistList.Add(artist);
                    }
                }
                return artistList;
            }
            catch (Exception e)
            {
                Debug.WriteLine("DeezerClient.GetSimilarArtists", e);
            }
            return null;
        }

        public async Task<Album> GetAlbumInfo(string albumTitle, string artistName)
        {
            try
            {
                var deezerClient = new HttpClient();
                string json = await deezerClient.GetStringAsync(new Uri(string.Format("http://api.deezer.com/search/album?q={0} {1}&appid={2}", albumTitle, artistName, MusicMDFetcher.DeezerAppId)));
                if (json == "{\"data\":[],\"total\":0}")
                {
                    return null;
                }
                var deezerAlbums = JsonConvert.DeserializeObject<Albums>(json);
                if (deezerAlbums.Data != null)
                {
                    var deezerAlbum = deezerAlbums.Data.FirstOrDefault();
                    var album = new Album();
                    album.MapFrom(deezerAlbum);
                    return album;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("DeezerClient.GetAlbumInfo", e);
            }
            return null;
        }

        public async Task<List<Album>> GetArtistTopAlbums(string artistId)
        {
            try
            {
                var deezerClient = new HttpClient();
                string json = await deezerClient.GetStringAsync(new Uri(string.Format("http://api.deezer.com/artist/{0}/albums?appid={1}", artistId, MusicMDFetcher.DeezerAppId)));
                var deezerAlbums = JsonConvert.DeserializeObject<Albums>(json);
                if (deezerAlbums == null) return null;
                if (deezerAlbums.Data == null) return null;
                var albumList = new List<Album>();
                foreach (var deezerAlbum in deezerAlbums.Data)
                {
                    var album = new Album();
                    album.MapFrom(deezerAlbum);
                    albumList.Add(album);
                }
                return albumList;
            }
            catch (Exception e)
            {
                Debug.WriteLine("DeezerClient.GetArtistTopAlbums", e);
            } 
            return null;
        }

        public Task<List<Artist>> GetTopArtistsGenre(string genre)
        {
            throw new NotImplementedException();
        }
    }
}
