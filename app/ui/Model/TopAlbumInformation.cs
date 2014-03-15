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
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VLC_WINRT.Model
{
    public class TopAlbumInformation
    {

        [JsonProperty("topalbums")]
        public TopAlbums TopAlbums { get; set; }
    }

    public class TopAlbum
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("playcount")]
        public string Playcount { get; set; }

        [JsonProperty("mbid")]
        public string Mbid { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("artist")]
        public TopArtist Artist { get; set; }

        [JsonProperty("image")]
        public TopImage[] Image { get; set; }

        [JsonProperty("@attr")]
        public Attr Attr { get; set; }
    }

    public class TopImage
    {

        [JsonProperty("#text")]
        public string Text { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }
    }

    public class TopAlbums
    {

        [JsonProperty("album")]
        public TopAlbum[] Album { get; set; }

        [JsonProperty("@attr")]
        public Attr2 Attr { get; set; }
    }

    public class TopArtist
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("mbid")]
        public string Mbid { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class Attr
    {

        [JsonProperty("rank")]
        public string Rank { get; set; }
    }

    public class Attr2
    {

        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("page")]
        public string Page { get; set; }

        [JsonProperty("perPage")]
        public string PerPage { get; set; }

        [JsonProperty("totalPages")]
        public string TotalPages { get; set; }

        [JsonProperty("total")]
        public string Total { get; set; }
    }

}
