/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
#if WINDOWS_APP
using System.ServiceModel.Security;
#endif
using System.Threading.Tasks;
using Windows.Storage;

namespace VLC_WINRT_APP.Helpers
{
    internal class NativeOperationsHelper
    {
        public static async Task<bool> FileExist(string fileName)
        {
            StorageFile file = null;
            try
            {
                file = await StorageFile.GetFileFromPathAsync(fileName);
            }
            catch
            {
                return false;
            }
            if (file == null)
                return false;
            return true;
        }
    }
}
