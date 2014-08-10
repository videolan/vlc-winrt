using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage;
using VLC_WINRT.Common;

namespace VLC_WINRT_APP.Commands.Settings
{
    public class RemoveFolderFromMusicLibrary : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            var lib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);
            lib.RequestRemoveFolderAsync(parameter as StorageFolder);
        }
    }
}
