/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Newtonsoft.Json;

namespace VLC_WINRT.Utility.Helpers.MusicLibrary.EchoNest
{
    public class Videos
    {
        [JsonProperty("response")]
        public Response Response { get; set; }
    }

    public partial class Response
    {
        [JsonProperty("start")]
        public int Start { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("video")]
        public Video[] Video { get; set; }
    }

    public class Video
    {

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("site")]
        public string Site { get; set; }

        [JsonProperty("date_found")]
        public string DateFound { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
