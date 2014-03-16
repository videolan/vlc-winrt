/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright c 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Newtonsoft.Json;

namespace VLC_WINRT.Utility.Helpers.MusicLibrary.LastFm
{
    public class ArtistInformation
    {
        [JsonProperty("artist")]
        public Artist Artist { get; set; }
    }

    public partial class Artist
    {
        [JsonProperty("image")]
        public Image[] Image { get; set; }

        [JsonProperty("streamable")]
        public string Streamable { get; set; }

        [JsonProperty("ontour")]
        public string Ontour { get; set; }

        [JsonProperty("stats")]
        public Stats Stats { get; set; }

        [JsonProperty("similar")]
        public Similar Similar { get; set; }

        //[JsonProperty("tags")]
        //public Tags Tags { get; set; }

        [JsonProperty("bio")]
        public Bio Bio { get; set; }
    }

    public class Artist2
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("image")]
        public Image2[] Image { get; set; }
    }

    public class Bio
    {

        [JsonProperty("links")]
        public Links Links { get; set; }

        [JsonProperty("published")]
        public string Published { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }

    public class Image2
    {

        [JsonProperty("#text")]
        public string Text { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }
    }

    public class Link
    {

        [JsonProperty("#text")]
        public string Text { get; set; }

        [JsonProperty("rel")]
        public string Rel { get; set; }

        [JsonProperty("href")]
        public string Href { get; set; }
    }

    public class Links
    {

        [JsonProperty("link")]
        public Link Link { get; set; }
    }

    public class Similar
    {

        [JsonProperty("artist")]
        public Artist2[] Artist { get; set; }
    }

    public class Stats
    {

        [JsonProperty("listeners")]
        public string Listeners { get; set; }

        [JsonProperty("playcount")]
        public string Playcount { get; set; }
    }

    public class Tag
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class Tags
    {

        [JsonProperty("tag")]
        public Tag[] Tag { get; set; }
    }

}
