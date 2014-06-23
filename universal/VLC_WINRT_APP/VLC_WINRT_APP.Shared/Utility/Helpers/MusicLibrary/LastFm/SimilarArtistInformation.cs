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
    public class SimilarArtistInformation
    {
        [JsonProperty("similarartists")]
        public Similarartists Similarartists { get; set; }
    }

    public class Similarartists
    {

        [JsonProperty("artist")]
        public SimilarArtist[] Artist { get; set; }

        [JsonProperty("@attr")]
        public Attr Attr { get; set; }
    }

    public class SimilarArtist
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("mbid")]
        public string Mbid { get; set; }

        [JsonProperty("match")]
        public string Match { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("image")]
        public SimilarArtistImage[] Image { get; set; }

        [JsonProperty("streamable")]
        public string Streamable { get; set; }
    }

    public class SimilarArtistImage
    {

        [JsonProperty("#text")]
        public string Text { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }
    }
}
