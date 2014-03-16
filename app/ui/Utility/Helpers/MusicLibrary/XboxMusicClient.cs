/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright c 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VLC_WINRT.ViewModels;
using XboxMusicLibrary.Models;
using XboxMusicLibrary.Settings;
using Album = VLC_WINRT.Utility.Helpers.MusicLibrary.MusicEntities.Album;
using Artist = VLC_WINRT.Utility.Helpers.MusicLibrary.MusicEntities.Artist;

namespace VLC_WINRT.Utility.Helpers.MusicLibrary
{
    public class XboxMusicClient : IMusicInformationManager
    {
        public async Task<Artist> GetArtistInfo(string artistName)
        {
            var music = await GetMusicEntity(artistName, new[] { Filters.Artists });
            var xboxArtistItem = music.Artists.Items.FirstOrDefault(x => x.Name == artistName) ??
                                 music.Artists.Items.FirstOrDefault();
            Debug.WriteLine("XBOX Music artist found : " + xboxArtistItem.Name);
            var artist = new Artist();
            artist.MapFrom(xboxArtistItem);
            return artist;
        }

        /// <summary>
        /// Get similar artists from Xbox Music (NOTE: Does not work, need required permission from Microsoft).
        /// </summary>
        /// <param name="artistId">The xbox artist Id.</param>
        /// <returns>A list of related artists.</returns>
        public async Task<List<Artist>> GetSimilarArtists(string artistId)
        {
            var music = await GetMusicEntityViaArtistId(new[] { artistId }, new[] { Extras.RelatedArtists });
            var xboxArtistItem = music.Artists.Items.FirstOrDefault();
            if (xboxArtistItem == null) return null;
            Debug.WriteLine("XBOX Music artist found : " + xboxArtistItem.Name);
            var artistList = new List<Artist>();
            foreach (var xboxArtist in xboxArtistItem.RelatedArtists.Items)
            {
                var artist = new Artist();
                artist.MapFrom(xboxArtist);
                artistList.Add(artist);
            }
            return artistList;
        }

        public Task<Album> GetAlbumInfo(string albumId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Album>> GetArtistTopAlbums(string artistId)
        {
            var music = await GetMusicEntityViaArtistId(new []{artistId}, new[] { Extras.Albums });
            var xboxArtistItem = music.Artists.Items.FirstOrDefault();
            if (xboxArtistItem == null) return null;
            Debug.WriteLine("XBOX Music artist found : " + xboxArtistItem.Name);

            var albumList = new List<Album>();
            // TODO: Use token to continue request if needed.
            foreach (var xboxAlbum in xboxArtistItem.Albums.Items)
            {
                var album = new Album();
                album.MapFrom(xboxAlbum);
                albumList.Add(album);
            }
            return albumList;
        }

        public async Task<Music> GetMusicEntityViaArtistId(string[] artistIds, Extras[] extras)
        {
            var xboxArtistItem = new XboxMusicLibrary.Models.Artist();
            try
            {
                // TODO: Client secret should be hidden. Hence why it's "secret"
                // TODO: Remove XboxMusicAuthenication from the music view model.
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
                Debug.WriteLine("Connecting to XBOX Music API for ");
                Debug.WriteLine("XBOX Music token " + Locator.MusicLibraryVM.XboxMusicAuthenication);
                var region = new Windows.Globalization.GeographicRegion();
                Music xboxMusic = await Locator.MusicLibraryVM.XboxMusicHelper.LookupMediaCatalog(Locator.MusicLibraryVM.XboxMusicAuthenication.AccessToken, artistIds, extras, new Culture(region.Code.ToLower(), null));

                Debug.WriteLine("XBOX Music artist found : " + xboxArtistItem.Name);

                return xboxMusic;
            }
            catch (Exception e)
            {
                Debug.WriteLine("XBOX Error\n" + e);
            }
            return null;
        }

        private async Task<Music> GetMusicEntity(string artistName, Filters[] filters)
        {
            var xboxArtistItem = new XboxMusicLibrary.Models.Artist();
            try
            {
                // TODO: Client secret should be hidden. Hence why it's "secret"
                // TODO: Remove XboxMusicAuthenication from the music view model.
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
                Debug.WriteLine(string.Format("Connecting to XBOX Music API for {0}", artistName));
                Debug.WriteLine("XBOX Music token " + Locator.MusicLibraryVM.XboxMusicAuthenication);
                var region = new Windows.Globalization.GeographicRegion();
                Music xboxMusic = await Locator.MusicLibraryVM.XboxMusicHelper.SearchMediaCatalog(Locator.MusicLibraryVM.XboxMusicAuthenication.AccessToken, artistName, null, 3, filters, new Culture(region.Code.ToLower(), null));

                Debug.WriteLine(string.Format("XBOX Music artist found : {0}", xboxArtistItem.Name));

                return xboxMusic;
            }
            catch (Exception e)
            {
                Debug.WriteLine("XBOX Error\n" + e);
            }
            return null;
        }

        private static long GetUnixTime(DateTime time)
        {
            time = time.ToUniversalTime();
            TimeSpan timeSpam = time - (new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local));
            return (long)timeSpam.TotalSeconds;
        }

    }
}
