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
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VLC_WINRT.Utility.Helpers.MusicLibrary.Deezer;
using VLC_WINRT.Utility.Helpers.MusicLibrary.MusicEntities;
using Album = VLC_WINRT.Utility.Helpers.MusicLibrary.MusicEntities.Album;
using Artist = VLC_WINRT.Utility.Helpers.MusicLibrary.MusicEntities.Artist;

namespace VLC_WINRT.Utility.Helpers.MusicLibrary
{
    public class DeezerClient : IMusicInformationManager
    {
        public async Task<Artist> GetArtistInfo(string artistName)
        {
            var deezerClient = new HttpClient();
            string json = await deezerClient.GetStringAsync(string.Format("http://api.deezer.com/search/artist?q={0}", artistName));
            // TODO: See if this is even needed. It should just map an empty object.
            if (json == "{\"data\":[],\"total\":0}")
            {
                return null;
            }
            var deezerArtists = JsonConvert.DeserializeObject<Artists>(json);
            var deezerArtist = deezerArtists.Data.FirstOrDefault();
            var artist = new Artist();
            artist.MapFrom(deezerArtist);
            return artist;

        }

        public async Task<List<Artist>> GetSimilarArtists(string artistId)
        {
            var deezerClient = new HttpClient();
            string json = await deezerClient.GetStringAsync(string.Format("http://api.deezer.com/artist/{0}/related", artistId));
            var deezerArtists = JsonConvert.DeserializeObject<Artists>(json);
            var artistList = new List<Artist>();
            foreach (var deezerArtist in deezerArtists.Data)
            {
                var artist = new Artist();
                artist.MapFrom(deezerArtist);
                artistList.Add(artist);
            }
            return artistList;
        }

        public Task<Album> GetAlbumInfo(string albumTitle)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Album>> GetArtistTopAlbums(string artistId)
        {
            var deezerClient = new HttpClient();
            string json = await deezerClient.GetStringAsync(string.Format("http://api.deezer.com/artist/{0}/albums", artistId));
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
    }
}
