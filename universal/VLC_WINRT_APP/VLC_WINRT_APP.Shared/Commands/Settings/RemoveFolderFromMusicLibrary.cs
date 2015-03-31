using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage;
using VLC_WINRT.Common;
using VLC_WinRT.ViewModels;

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
