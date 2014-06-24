/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace VLC_WINRT_APP.Helpers
{
    public static class DoesFileExistHelper
    {
        public static async Task<bool> DoesFileExistAsync(string fileName)
        {
            try
            {
                // TODO: Change to TryGetItemAsync when we switch toWindows 8.1
                await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
