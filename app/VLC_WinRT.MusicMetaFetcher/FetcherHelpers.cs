using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace VLC_WinRT.MediaMetaFetcher
{
    public static class FetcherHelpers
    {
        public static async Task<bool> SaveImage(int id, String folderName, byte[] img)
        {
            String fileName = String.Format("{0}.jpg", id);
            try
            {
                using (var streamWeb = new InMemoryRandomAccessStream())
                {
                    using (var writer = new DataWriter(streamWeb.GetOutputStreamAt(0)))
                    {
                        writer.WriteBytes(img);
                        await writer.StoreAsync();
                        var albumPic = await ApplicationData.Current.LocalFolder.CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists);

                        var file = await albumPic.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
                        Debug.WriteLine("Writing file " + folderName + " " + id);
                        using (var raStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                        {
                            using (var thumbnailStream = streamWeb.GetInputStreamAt(0))
                            {
                                using (var stream = raStream.GetOutputStreamAt(0))
                                {
                                    await RandomAccessStream.CopyAsync(thumbnailStream, stream);
                                    await stream.FlushAsync();
                                }
                            }
                            await raStream.FlushAsync();
                        }
                        await writer.FlushAsync();
                    }
                    await streamWeb.FlushAsync();
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error saving album art: " + e);
                return false;
            }
        }
    }
}
