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
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VLC_WINRT_APP.Helpers.MusicLibrary.Deezer;
using VLC_WINRT_APP;
using Album = VLC_WINRT_APP.Helpers.MusicLibrary.MusicEntities.Album;
using Artist = VLC_WINRT_APP.Helpers.MusicLibrary.MusicEntities.Artist;
using Windows.Web.Http;

namespace VLC_WINRT_APP.Helpers.MusicLibrary
{
    public class DeezerClient : IMusicInformationManager
    {
        public async Task<Artist> GetArtistInfo(string artistName)
        {
            try
            {
                var deezerClient = new HttpClient();
                string json = await deezerClient.GetStringAsync(new Uri(string.Format("http://api.deezer.com/search/artist?q={0}&appid={1}", System.Net.WebUtility.HtmlEncode(artistName), App.DeezerAppID)));
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
                ExceptionHelper.CreateMemorizedException("DeezerClient.GetArtistInfo", e);
            }
            return null;
        }

        public async Task<List<Artist>> GetSimilarArtists(string artistId)
        {
            try
            {
                var deezerClient = new HttpClient();
                string json = await deezerClient.GetStringAsync(new Uri(string.Format("http://api.deezer.com/artist/{0}/related?appid={1}", artistId, App.DeezerAppID)));
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
                ExceptionHelper.CreateMemorizedException("DeezerClient.GetSimilarArtists", e);
            }
            return null;
        }

        public async Task<Album> GetAlbumInfo(string albumTitle, string artistName)
        {
            try
            {
                var deezerClient = new HttpClient();
                string json = await deezerClient.GetStringAsync(new Uri(string.Format("http://api.deezer.com/search/album?q={0} {1}&appid={2}", albumTitle, artistName, App.DeezerAppID)));
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
                ExceptionHelper.CreateMemorizedException("DeezerClient.GetAlbumInfo", e);
            }
            return null;
        }

        public async Task<List<Album>> GetArtistTopAlbums(string artistId)
        {
            try
            {
                var deezerClient = new HttpClient();
                string json = await deezerClient.GetStringAsync(new Uri(string.Format("http://api.deezer.com/artist/{0}/albums?appid={1}", artistId, App.DeezerAppID)));
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
                ExceptionHelper.CreateMemorizedException("DeezerClient.GetArtistTopAlbums", e);
            } 
            return null;
        }

        public Task<List<Artist>> GetTopArtistsGenre(string genre)
        {
            throw new NotImplementedException();
        }
    }
}
