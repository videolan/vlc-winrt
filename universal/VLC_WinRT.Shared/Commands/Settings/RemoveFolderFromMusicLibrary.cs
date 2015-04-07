using VLC_WinRT.ViewModels;
using VLC_WINRT.Common;
using Windows.Storage;
using System;

namespace VLC_WinRT.Commands.Settings
{
    public class RemoveFolderFromMusicLibrary : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
#if WINDOWS_APP
            var lib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);
            await lib.RequestRemoveFolderAsync(parameter as StorageFolder);
            await Locator.SettingsVM.GetLibrariesFolders();
#endif
        }
    }
}
