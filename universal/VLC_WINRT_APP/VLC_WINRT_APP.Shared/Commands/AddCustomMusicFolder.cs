/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using VLC_WINRT.Common;
#if WINDOWS_PHONE_APP
using VLC_WINPRT;
#endif
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.Settings;

namespace VLC_WINRT_APP.Commands
{
    public class AddCustomAudioFolder : AlwaysExecutableCommand
    {
        public async override void Execute(object parameter)
        {
            var picker = new FolderPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.MusicLibrary,
            };
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".wma");
            // TODO: Add more file types 

            StorageFolder folder = await picker.PickSingleFolderAsync();

            if (folder != null)
            {
                string mru = StorageApplicationPermissions.FutureAccessList.Add(folder);
                Locator.SettingsVM.AddMusicFolder(new CustomFolder()
                {
                    DisplayName = folder.DisplayName,
                    Mru = mru,
                });
                await Locator.MusicLibraryVM.StartIndexing();
            }
        }
    }
}
