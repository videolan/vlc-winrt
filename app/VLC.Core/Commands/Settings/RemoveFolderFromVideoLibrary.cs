using VLC.ViewModels;
using Windows.Storage;
using System;
using VLC.Utils;

namespace VLC.Commands.Settings
{
    public class RemoveFolderFromVideoLibrary : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
#if WINDOWS_APP
            var lib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Videos);
            await lib.RequestRemoveFolderAsync(parameter as StorageFolder);
            await Locator.SettingsVM.GetVideoLibraryFolders();
#endif
        }
    }
}
