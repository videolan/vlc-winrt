/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;

namespace VLC_WINRT.Model
{
    public class MediaHistory
    {
        public DateTime LastPlayed { get; set; }
        public string Filename { get; set; }
        public string Token { get; set; }
        public double TotalWatchedMilliseconds { get; set; }
        public bool IsAudio { get; set; }
    }
}
