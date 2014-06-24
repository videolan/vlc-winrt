/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Diagnostics;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using VLC_WINRT.Common;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Commands.Video
{
    public class OpenSubtitleCommand : AlwaysExecutableCommand
    {
        public async override void Execute(object parameter)
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.VideosLibrary
            };
            picker.FileTypeFilter.Add(".srt");
            picker.FileTypeFilter.Add(".ass");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                string mru = StorageApplicationPermissions.FutureAccessList.Add(file);

                string mrl = "file://" + mru;
                Locator.VideoVm.OpenSubtitle(mrl);
            }
            else
            {
                Debug.WriteLine("Cancelled Opening subtitle");
            }
        }
    }
}
