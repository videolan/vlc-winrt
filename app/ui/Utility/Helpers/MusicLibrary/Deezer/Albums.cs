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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VLC_WINRT.Utility.Helpers.MusicLibrary.Deezer
{
    public class Albums
    {
        [JsonProperty("data")]
        public Album[] Data { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }
    }
    public class Album
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("upc")]
        public string Upc { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("cover")]
        public string Cover { get; set; }

        [JsonProperty("genre_id")]
        public int GenreId { get; set; }

        [JsonProperty("genres")]
        public Genres Genres { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("nb_tracks")]
        public int NbTracks { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("fans")]
        public int Fans { get; set; }

        [JsonProperty("rating")]
        public int Rating { get; set; }

        [JsonProperty("release_date")]
        public string ReleaseDate { get; set; }

        [JsonProperty("available")]
        public bool Available { get; set; }

        [JsonProperty("artist")]
        public Artist Artist { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("tracks")]
        public Tracks Tracks { get; set; }
    }
}
