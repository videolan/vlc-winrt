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
    public class Tracks
    {
        [JsonProperty("data")]
        public Track[] Data { get; set; }
    }

    public class Genres
    {

        [JsonProperty("data")]
        public Genre[] Data { get; set; }
    }

    public class Genre
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class AlbumArtist
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class Track
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("readable")]
        public bool Readable { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonProperty("rank")]
        public string Rank { get; set; }

        [JsonProperty("preview")]
        public string Preview { get; set; }

        [JsonProperty("artist")]
        public AlbumArtist Artist { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
