/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright c 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Newtonsoft.Json;

namespace VLC_WINRT.Model
{
    internal class ArtistInformation
    {
        [JsonProperty("artist")]
        public Artist Artist { get; set; }
    }

    internal partial class Artist
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

    internal class Artist2
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("image")]
        public Image2[] Image { get; set; }
    }

    internal class Bio
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

    internal class Image2
    {

        [JsonProperty("#text")]
        public string Text { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }
    }

    internal class Link
    {

        [JsonProperty("#text")]
        public string Text { get; set; }

        [JsonProperty("rel")]
        public string Rel { get; set; }

        [JsonProperty("href")]
        public string Href { get; set; }
    }

    internal class Links
    {

        [JsonProperty("link")]
        public Link Link { get; set; }
    }

    internal class Similar
    {

        [JsonProperty("artist")]
        public Artist2[] Artist { get; set; }
    }

    internal class Stats
    {

        [JsonProperty("listeners")]
        public string Listeners { get; set; }

        [JsonProperty("playcount")]
        public string Playcount { get; set; }
    }

    internal class Tag
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    internal class Tags
    {

        [JsonProperty("tag")]
        public Tag[] Tag { get; set; }
    }

}
