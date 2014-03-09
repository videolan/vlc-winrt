/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

namespace VLC_WINRT.Model
{
    public class Subtitle
    {
        public int Id;
        public string Name;

        public override string ToString()
        {
            return Id + ": " + Name;
        }
    }
}
