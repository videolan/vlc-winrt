using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VLC.Helpers;
using VLC.Model;
using VLC.Utils;
using VLC.ViewModels;
using Windows.Storage;

namespace VLC.Commands.MediaLibrary
{
    public class CopyToLocalStorageCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            if (parameter is VLCStorageFile)
            {
                // Copy the selected file.
                var file = parameter as VLCStorageFile;
                StorageFile f = await StorageFile.GetFileFromPathAsync(file.StorageItem.Path);
                CopyMediaFileToLocalStorage(f);
            }
            else if (parameter is VLCStorageFolder)
            {
                // Copy all media files in the selected folder.
                var folder = parameter as VLCStorageFolder;
                StorageFolder d = await StorageFolder.GetFolderFromPathAsync(folder.StorageItem.Path);
                await MediaLibraryHelper.ForeachSupportedFile(d, (IReadOnlyList<StorageFile> files) =>
                {
                    foreach (var f in files)
                        CopyMediaFileToLocalStorage(f);
                    return Task.FromResult(true);
                });
            }
        }

        private void CopyMediaFileToLocalStorage(StorageFile source)
        {
            Locator.FileCopyService.Enqueue(source);
        }
    }
}
