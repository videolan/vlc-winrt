using VLC_WinRT.ViewModels;
using VLC_WINRT.Common;
using Windows.Storage;
using System;
using VLC_WinRT.Utils;

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
