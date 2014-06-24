/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

namespace XboxMusicLibrary.Models
{
    public class Contributor
    {
        // The contributing artist.
        public Artist Artist { get; set; }
        // The type of contribution, for example "Main" or "Featured".
        public string Role { get; set; }
    }
}
