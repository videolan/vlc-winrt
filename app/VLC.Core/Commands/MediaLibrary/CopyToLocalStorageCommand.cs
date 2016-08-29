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
                await CopyMediaFileToLocalStorage(f);
            }
            else if (parameter is VLCStorageFolder)
            {
                // Copy all media files in the selected folder.
                var folder = parameter as VLCStorageFolder;
                StorageFolder d = await StorageFolder.GetFolderFromPathAsync(folder.StorageItem.Path);
                var list = await MediaLibraryHelper.GetSupportedFiles(d);
                foreach (StorageFile f in list)
                    await CopyMediaFileToLocalStorage(f);
            }
        }

        private async Task CopyMediaFileToLocalStorage(StorageFile source)
        {
            StorageFile copy = await source.CopyAsync(await FileUtils.GetLocalStorageMediaFolder(),
                source.Name, NameCollisionOption.GenerateUniqueName);
            bool success = await Locator.MediaLibrary.DiscoverMediaItemOrWaitAsync(copy, false);
            if (success == false)
                await copy.DeleteAsync();
        }
    }
}
