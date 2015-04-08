using VLC_WinRT.ViewModels;
using Windows.Storage;
using System;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Commands.Settings
{
    public class AddFolderToLibrary : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
#if WINDOWS_APP
            KnownLibraryId id = (KnownLibraryId)parameter;
            var lib = await StorageLibrary.GetLibraryAsync(id);
            await lib.RequestAddFolderAsync();
            await Locator.SettingsVM.GetLibrariesFolders();
#endif
        }
    }
}