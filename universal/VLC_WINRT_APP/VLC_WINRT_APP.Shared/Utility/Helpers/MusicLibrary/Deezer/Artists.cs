/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Newtonsoft.Json;

namespace VLC_WINRT.Utility.Helpers.MusicLibrary.Deezer
{
    public class Artists
    {
        [JsonProperty("data")]
        public Artist[] Data { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }
    }

    public class Artist
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("picture")]
        public string Picture { get; set; }

        [JsonProperty("nb_album")]
        public int NbAlbum { get; set; }

        [JsonProperty("nb_fan")]
        public int NbFan { get; set; }

        [JsonProperty("radio")]
        public bool Radio { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
