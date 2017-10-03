using VLC.ViewModels;
using Windows.Storage;
using System;
using VLC.Utils;

namespace VLC.Commands.Settings
{
    public class AddFolderToLibrary : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
#if WINDOWS_APP
            KnownLibraryId id = (KnownLibraryId)parameter;
            var lib = await StorageLibrary.GetLibraryAsync(id);
            await lib.RequestAddFolderAsync();
            await Locator.SettingsVM.GetVideoLibraryFolders();
            await Locator.SettingsVM.GetMusicLibraryFolders();
#endif
        }
    }
}