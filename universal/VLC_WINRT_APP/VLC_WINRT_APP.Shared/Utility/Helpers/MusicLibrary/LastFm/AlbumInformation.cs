/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Newtonsoft.Json;

namespace VLC_WINRT_APP.Utility.Helpers.MusicLibrary.LastFm
{
    /// <summary>
    /// Used for mapping Last.FM Json object.
    /// </summary>
    public class AlbumInformation
    {
        [JsonProperty("album")]
        public Album Album { get; set; }
    }

    public class Album
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("mbid")]
        public string Mbid { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("releasedate")]
        public string Releasedate { get; set; }

        [JsonProperty("image")]
        public Image[] Image { get; set; }

        [JsonProperty("listeners")]
        public string Listeners { get; set; }

        [JsonProperty("playcount")]
        public string Playcount { get; set; }
    }

    public partial class Artist
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("mbid")]
        public string Mbid { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class Image
    {

        [JsonProperty("#text")]
        public string Text { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }
    }
}
