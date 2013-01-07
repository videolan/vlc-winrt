using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;

namespace VLC_WINRT.Utility.Services
{
    public class MediaFolderScanner
    {
        //TODO: provide a better way to describe to the app what file types are supported
        private readonly List<string> ValidFiles = new List<string> {".m4v", ".mp4", ".mp3", ".avi"};

        public async Task<List<StorageFile>> GetMediaFromFolder(StorageFolder folder, int numberOfFiles,
                                                                       CommonFileQuery query)
        {
            IReadOnlyList<StorageFile> files = await folder.GetFilesAsync(CommonFileQuery.OrderByDate);
            List<StorageFile> validFiles = files.Where(file => ValidFiles.Contains(file.FileType)).Take(numberOfFiles).ToList();

            return validFiles;
        }
    }
}