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
using Windows.Storage.Streams;

namespace VLC_WinRT.Utils
{
    public class ConvertImage
    {
        public static async Task<byte[]> ConvertImagetoByte(StorageFile image)
        {
            IRandomAccessStream fileStream = await image.OpenAsync(FileAccessMode.Read);
            var reader = new DataReader(fileStream.GetInputStreamAt(0));
            await reader.LoadAsync((uint)fileStream.Size);

            var pixels = new byte[fileStream.Size];

            reader.ReadBytes(pixels);

            return pixels;

        }
    }
}
