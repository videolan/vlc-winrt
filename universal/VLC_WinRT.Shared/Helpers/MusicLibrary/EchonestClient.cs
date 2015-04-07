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
using System.Threading.Tasks;
using Newtonsoft.Json;
using VLC_WinRT.Helpers.MusicLibrary.EchoNest;

namespace VLC_WinRT.Helpers.MusicLibrary
{
    public class EchonestClient
    {
        private const string ApiKey = "GTDAC7FAIKZFJQK6F";

        /// <summary>
        /// Get an artists Twitter Handle.
        /// </summary>
        /// <param name="artist">An artists EchoNest Entity.</param>
        /// <returns>A twitter handle.</returns>
        public async Task<string> GetArtistsTwitterHandle(EchoNest.Artist artist)
        {
            try
            {
                var echonestClient = new HttpClient();
                var response =
                    await echonestClient.GetStringAsync(
                        new Uri(
                            string.Format("http://developer.echonest.com/api/v4/artist/twitter?api_key={0}&id={1}", ApiKey, artist.Id)));
                var artistTwitterResult = JsonConvert.DeserializeObject<Twitter>(response);
                if (artistTwitterResult == null) return null;
                if (artistTwitterResult.Response.Artist == null) return null;
                var topResult = artistTwitterResult.Response.Artist;
                return topResult == null ? null : topResult.Twitter;
            }
            catch (Exception)
            {
                LogHelper.Log("Error parsing echonest data.");
            }
            return null;
        }

        /// <summary>
        /// Get an artists music videos.
        /// </summary>
        /// <param name="artist">The artists EchoNest Entity.</param>
        /// <returns>A list of music videos.</returns>
        public async Task<List<Video>> GetArtistsVideos(EchoNest.Artist artist)
        {
            try
            {
                var echonestClient = new HttpClient();
                var response =
                    await echonestClient.GetStringAsync(
                        new Uri(
                            string.Format("http://developer.echonest.com/api/v4/artist/video?api_key={0}&id={1}&format=json&start=0", ApiKey, artist.Id)));
                var artistVideos = JsonConvert.DeserializeObject<Videos>(response);
                if (artistVideos == null) return null;
                return artistVideos.Response == null ? null : artistVideos.Response.Video.ToList();
            }
            catch (Exception)
            {
                LogHelper.Log("Error parsing echonest data.");
            }
            return null;
        }

        /// <summary>
        /// Get an artists EchoNest ID. 
        /// (TODO: once we have a proper database, get this ID and store it)
        /// </summary>
        /// <param name="artistName">The artists name.</param>
        /// <returns>A string containing the EchoNest ID.</returns>
        public async Task<EchoNest.Artist> GetArtistId(string artistName)
        {
            try
            {
                var echonestClient = new HttpClient();
                var response =
                    await echonestClient.GetStringAsync(
                        new Uri(
                            string.Format("http://developer.echonest.com/api/v4/artist/search?api_key={0}&name={1}", ApiKey, artistName)));
                var echonestArtistResults = JsonConvert.DeserializeObject<Artists>(response);
                if (echonestArtistResults == null) return null;
                if (echonestArtistResults.Response.Artists == null) return null;
                var topResult = echonestArtistResults.Response.Artists.FirstOrDefault();
                return topResult;
            }
            catch (Exception)
            {
                LogHelper.Log("Error parsing echonest data.");
            }
            return null;
        }
    }
}
