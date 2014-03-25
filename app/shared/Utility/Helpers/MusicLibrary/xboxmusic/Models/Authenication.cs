/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using Newtonsoft.Json;

namespace VLC_WINRT.Utility.Helpers.MusicLibrary.xboxmusic.Models
{
    /// <summary>
    /// Authenication for Xbox Music.
    /// </summary>
    public class Authenication
    {
        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public string ExpiresIn { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        public long StartTime { get; set; }
    }
}
