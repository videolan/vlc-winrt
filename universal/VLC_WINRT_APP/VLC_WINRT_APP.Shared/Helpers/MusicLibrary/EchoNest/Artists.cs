/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Newtonsoft.Json;

namespace VLC_WINRT_APP.Helpers.MusicLibrary.EchoNest
{
    public class Artists
    {
        [JsonProperty("response")]
        public Response Response { get; set; }
    }

    public partial class Artist
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public partial class Response
    {

        [JsonProperty("status")]
        public Status Status { get; set; }

        [JsonProperty("artists")]
        public Artist[] Artists { get; set; }
    }

    public class Status
    {

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
