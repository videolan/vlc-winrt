using VLC.ViewModels;
using Windows.Storage;
using System;
using VLC.Utils;

namespace VLC.Commands.Settings
{
    public class RemoveFolderFromMusicLibrary : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
#if WINDOWS_APP
            var lib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);
            await lib.RequestRemoveFolderAsync(parameter as StorageFolder);
            await  Locator.SettingsVM.GetMusicLibraryFolders();
#endif
        }
    }
}
