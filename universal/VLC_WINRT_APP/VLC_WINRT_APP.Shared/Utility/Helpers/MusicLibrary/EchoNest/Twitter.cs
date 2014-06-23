/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Newtonsoft.Json;

namespace VLC_WINRT_APP.Utility.Helpers.MusicLibrary.EchoNest
{
    public class Twitter
    {
        [JsonProperty("response")]
        public Response Response { get; set; }
    }

    public partial class Artist
    {

        [JsonProperty("twitter")]
        public string Twitter { get; set; }
    }

    public partial class Response
    {
        [JsonProperty("artist")]
        public Artist Artist { get; set; }
    }
}
