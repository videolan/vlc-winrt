using System;
using Windows.Storage;
using VLC_WINRT.Common;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Commands.Settings
{
    public class AddFolderToLibrary : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            KnownLibraryId id = (KnownLibraryId)parameter;
            var lib = await StorageLibrary.GetLibraryAsync(id);
            lib.RequestAddFolderAsync();
            Locator.SettingsVM.GetLibrariesFolders();
        }
    }
}