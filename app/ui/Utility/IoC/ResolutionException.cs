/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;

namespace VLC_WINRT.Utility.IoC
{
    public class ResolutionException : Exception
    {
        public ResolutionException(string s) : base(s)
        {
        }
    }
}
