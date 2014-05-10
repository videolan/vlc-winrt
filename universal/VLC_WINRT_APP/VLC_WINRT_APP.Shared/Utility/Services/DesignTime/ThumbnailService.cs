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
using Windows.UI.Xaml.Media.Imaging;
using VLC_WINRT.Utility.Services.Interface;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace VLC_WINRT.Utility.Services.DesignTime
{
    public class ThumbnailService : IThumbnailService
    {
        public async Task<StorageItemThumbnail> GetThumbnail(StorageFile file)
        {
            var uri = new Uri("ms-appx:///Assets/Logo.png");
            StorageFile image = await StorageFile.GetFileFromApplicationUriAsync(uri);

            StorageItemThumbnail thumb = await image.GetThumbnailAsync(ThumbnailMode.VideosView);
            return thumb;
        }

        public async Task<WriteableBitmap> GetScreenshot(StorageFile file)
        {
            WriteableBitmap bmp = new WriteableBitmap(400,300);
            await Task.Delay(5);
            return bmp;
        }
    }
}
