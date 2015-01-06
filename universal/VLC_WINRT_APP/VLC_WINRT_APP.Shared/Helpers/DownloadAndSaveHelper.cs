/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace VLC_WINRT_APP.Helpers
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

        public static async Task<StorageFile> WriteableBitmapToStorageFile(WriteableBitmap WB, FileFormat fileFormat, string fileName)
        {
            Guid BitmapEncoderGuid = BitmapEncoder.JpegEncoderId;
            fileName += ".";
            switch (fileFormat)
            {
                case FileFormat.Jpeg:
                    fileName += "jpg";
                    BitmapEncoderGuid = BitmapEncoder.JpegEncoderId;
                    break;
                case FileFormat.Png:
                    fileName += "png";
                    BitmapEncoderGuid = BitmapEncoder.PngEncoderId;
                    break;
                case FileFormat.Bmp:
                    fileName += "bmp";
                    BitmapEncoderGuid = BitmapEncoder.BmpEncoderId;
                    break;
                case FileFormat.Tiff:
                    fileName += "tiff";
                    BitmapEncoderGuid = BitmapEncoder.TiffEncoderId;
                    break;
                case FileFormat.Gif:
                    fileName += "gif";
                    BitmapEncoderGuid = BitmapEncoder.GifEncoderId;
                    break;
            }
            StorageFolder videoPic = await ApplicationData.Current.LocalFolder.CreateFolderAsync("videoPic", CreationCollisionOption.OpenIfExists);
            var file = await videoPic.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoderGuid, stream);
                Stream pixelStream = WB.PixelBuffer.AsStream();
                byte[] pixels = new byte[pixelStream.Length];
                await pixelStream.ReadAsync(pixels, 0, pixels.Length);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                          (uint)WB.PixelWidth,
                          (uint)WB.PixelHeight,
                          96.0,
                          96.0,
                          pixels);
                await encoder.FlushAsync();
            }
            return file;
        }
        public enum FileFormat
        {
            Jpeg,
            Png,
            Bmp,
            Tiff,
            Gif
        }
    }
}
