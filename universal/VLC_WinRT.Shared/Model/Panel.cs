/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/
 
using VLC_WinRT.Utils;

namespace VLC_WinRT.Model
{
    public class Panel
    {
        public Panel(string t, int i, string defaultIcon, string filledIcon)
        {
            Title = t;
            Index = i;
            DefaultIcon = defaultIcon;
            FilledIcon = filledIcon;
        }

        public string Title { get; private set; }
        public int Index { get; private set; }
        public string DefaultIcon { get; private set; }

        public string FilledIcon { get; private set; }
    }
}
