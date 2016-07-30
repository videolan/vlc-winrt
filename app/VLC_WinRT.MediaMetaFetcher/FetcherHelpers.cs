using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VLC.Utils;
using Windows.Storage;
using Windows.Storage.Streams;

namespace VLC.MediaMetaFetcher
{
    public static class FetcherHelpers
    {
        public static async Task<bool> SaveBytes(int id, String folderName, byte[] img, string extension, bool isTemp)
        {
            String fileName = String.Format("{0}.{1}", id, extension);
            try
            {
                using (var streamWeb = new InMemoryRandomAccessStream())
                {
                    using (var writer = new DataWriter(streamWeb.GetOutputStreamAt(0)))
                    {
                        writer.WriteBytes(img);
                        await writer.StoreAsync();

                        StorageFolder folder;
                        if (isTemp)
                            folder = ApplicationData.Current.TemporaryFolder;
                        else
                            folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists);

                        var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
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
                Debug.WriteLine("Error saving bytes: " + e);
                return false;
            }
        }

        public static async Task<string> ExtractFromArchive(int id, string archivePath)
        {
            try
            {
                var subFile = ZipFile.OpenRead(archivePath)?.Entries?.FirstOrDefault(x => x.FullName.EndsWith("srt") || x.FullName.EndsWith("ass"));
                var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("movieSub", CreationCollisionOption.OpenIfExists);
                
                if (subFile != null)
                {
                    var ext = Path.GetExtension(subFile.FullName);
                    subFile.ExtractToFile($"{folder.Path}\\{id}{ext}", true);
                    return ext;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to extract srt from zip archive");
            }
            return string.Empty;
        }
    }
}
