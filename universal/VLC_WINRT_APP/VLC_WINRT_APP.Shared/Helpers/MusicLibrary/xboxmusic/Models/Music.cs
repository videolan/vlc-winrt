/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Newtonsoft.Json;
using System;
using System.Diagnostics;
using VLC_WINRT_APP.Helpers;

namespace XboxMusicLibrary.Models
{
    public class Music
    {
        // A paginated list of Artists that matched the request criteria.
        public Artists Artists { get; set; }
        // A paginated list of Albums that matched the request criteria.
        public Albums Albums { get; set; }
        // A paginated list of Tracks that matched the request criteria.
        public Tracks Tracks { get; set; }
        // Optional error.
        public Error Error { get; set; }

        public static Music PopulateObject(string jsonContent)
        {
            Music root = null;

            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.NullValueHandling = NullValueHandling.Ignore;

                root = JsonConvert.DeserializeObject<Music>(jsonContent, settings);
            }
            catch (Exception ex)
            {
                LogHelper.Log(string.Format("Exception triggered in Music::PopulateObject method: {0}", ex.Message));
                throw;
            }

            return root;
        }
    }
}
