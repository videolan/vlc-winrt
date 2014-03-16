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
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;

namespace VLC_WINRT.Utility.Helpers
{
    public class DownloadAndSaveHelper
    {
        public async static Task SaveAsync(
           Uri fileUri,
           StorageFolder folder,
           string fileName)
        {
            // Hitting System.UnauthorizedAccessException when the file already exists.
            // If they already have it, keep what is there.
            var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
            var downloader = new BackgroundDownloader();
            var download = downloader.CreateDownload(
                fileUri,
                file);

            await download.StartAsync();
        }
    }
}
