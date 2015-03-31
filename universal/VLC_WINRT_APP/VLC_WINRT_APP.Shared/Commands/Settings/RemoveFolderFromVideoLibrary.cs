using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage;
using VLC_WINRT.Common;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.Settings
{
    public class RemoveFolderFromVideoLibrary : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
#if WINDOWS_APP
            var lib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Videos);
            lib.RequestRemoveFolderAsync(parameter as StorageFolder);
            Locator.SettingsVM.GetLibrariesFolders();
#endif
        }
    }
}
