using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;

namespace VLC.Utils
{
    public static class FileUtils
    {
        /**
         * Size of the chunks that will be hashed in bytes (64 KB)
         */
        private static readonly uint HASH_CHUNK_SIZE = 64 * 1024;

        public static async Task<string> ComputeHash(StorageFile file)
        {
            using (var stream = await file.OpenReadAsync())
            {
                var input = stream.AsStream();
                long lhash, streamsize;
                streamsize = input.Length;
                lhash = streamsize;

                long i = 0;
                byte[] buffer = new byte[sizeof(long)];
                while (i < 65536 / sizeof(long) && (input.Read(buffer, 0, sizeof(long)) > 0))
                {
                    i++;
                    lhash += BitConverter.ToInt64(buffer, 0);
                }

                input.Position = Math.Max(0, streamsize - 65536);
                i = 0;
                while (i < 65536 / sizeof(long) && (input.Read(buffer, 0, sizeof(long)) > 0))
                {
                    i++;
                    lhash += BitConverter.ToInt64(buffer, 0);
                }
                input.Flush();
                byte[] result = BitConverter.GetBytes(lhash);
                Array.Reverse(result);
                return ToHexadecimal(result);
            }
        }

        private static string ToHexadecimal(byte[] bytes)
        {
            StringBuilder hexBuilder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                hexBuilder.Append(bytes[i].ToString("x2"));
            }
            return hexBuilder.ToString();
        }

        public static async Task<StorageFolder> GetLocalStorageMediaFolder()
        {
            return await ApplicationData.Current.LocalFolder.CreateFolderAsync(
                "Media", CreationCollisionOption.OpenIfExists);
        }
    }
}
