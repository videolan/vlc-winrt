/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using VLC_WINRT.Utility.Services.Interface;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace VLC_WINRT.Utility.Services.RunTime
{
    public class ThumbnailService : IThumbnailService
    {
        public async Task<StorageItemThumbnail> GetThumbnail(StorageFile file)
        {
            StorageItemThumbnail thumb = null;

            try
            {
                thumb = await file.GetThumbnailAsync(ThumbnailMode.VideosView);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error getting thumbnail: ");
                Debug.WriteLine(ex);
            }

            return thumb;
        }
    }
}
