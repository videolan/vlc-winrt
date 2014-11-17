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
using Windows.Storage.AccessCache;
using Windows.UI.Xaml.Media.Imaging;
using VLC_WINRT_APP.Services.Interface;
using Windows.Storage;
using Windows.Storage.FileProperties;
using libVLCX;

namespace VLC_WINRT_APP.Services.RunTime
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

        public async Task<WriteableBitmap> GetScreenshot(StorageFile file)
        {
            using (Thumbnailer thumbnailer = new Thumbnailer())
            {
                string token = StorageApplicationPermissions.FutureAccessList.Add(file);
                var screenshot = await thumbnailer.TakeScreenshot("winrt://" + token, 342, 234);
                return screenshot;
            }
        }
    }
}
