/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

namespace XboxMusicLibrary.Settings
{
    public class Culture
    {
        public string Language { get; set; }
        public string Country { get; set; }

        public Culture(string language, string country)
        {
            this.Language = language;
            this.Country = country;
        }
    }
}
