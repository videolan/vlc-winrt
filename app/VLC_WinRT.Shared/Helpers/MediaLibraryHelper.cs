using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VLC_WinRT.Model;
using Windows.Storage;
using Windows.Storage.Search;

namespace VLC_WinRT.Helpers
{
    public static class MediaLibraryHelper
    {
        public static async Task<IReadOnlyList<StorageFile>> GetSupportedFiles(StorageFolder folder)
        {
            IReadOnlyList<StorageFile> files = null;
            try
            {
                var queryOptions = new QueryOptions { FolderDepth = FolderDepth.Deep };
                foreach (var type in VLCFileExtensions.Supported)
                    queryOptions.FileTypeFilter.Add(type);
                var fileQueryResult = folder.CreateFileQueryWithOptions(queryOptions);
                files = await fileQueryResult.GetFilesAsync();
            }
            catch(Exception e)
            {
            }
            return files;
        }


    }
}
