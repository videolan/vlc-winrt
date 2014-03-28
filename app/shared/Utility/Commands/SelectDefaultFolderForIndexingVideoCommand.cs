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
using VLC_WINRT.ViewModels;
#if WINDOWS_PHONE_APP
using VLC_WINPRT;
#endif

namespace VLC_WINRT.Utility.Commands
{
    public class SelectDefaultFolderForIndexingVideoCommand : AlwaysExecutableCommand
    {
        public async override void Execute(object parameter)
        {
            var picker = new FolderPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.VideosLibrary,
            };
            picker.FileTypeFilter.Add(".3g2");
            picker.FileTypeFilter.Add(".3gp");
            picker.FileTypeFilter.Add(".3gp2");
            picker.FileTypeFilter.Add(".3gpp");
            picker.FileTypeFilter.Add(".amv");
            picker.FileTypeFilter.Add(".asf");
            picker.FileTypeFilter.Add(".avi");
            picker.FileTypeFilter.Add(".divx");
            picker.FileTypeFilter.Add(".drc");
            picker.FileTypeFilter.Add(".dv");
            picker.FileTypeFilter.Add(".f4v");
            picker.FileTypeFilter.Add(".flv");
            picker.FileTypeFilter.Add(".gvi");
            picker.FileTypeFilter.Add(".gxf");
            picker.FileTypeFilter.Add(".ismv");
            picker.FileTypeFilter.Add(".iso");
            picker.FileTypeFilter.Add(".m1v");
            picker.FileTypeFilter.Add(".m2v");
            picker.FileTypeFilter.Add(".m2t");
            picker.FileTypeFilter.Add(".m2ts");
            picker.FileTypeFilter.Add(".m3u8");
            picker.FileTypeFilter.Add(".mkv");
            picker.FileTypeFilter.Add(".mov");
            picker.FileTypeFilter.Add(".mp2");
            picker.FileTypeFilter.Add(".mp2v");
            picker.FileTypeFilter.Add(".mp4");
            picker.FileTypeFilter.Add(".mp4v");
            picker.FileTypeFilter.Add(".mpe");
            picker.FileTypeFilter.Add(".mpeg");
            picker.FileTypeFilter.Add(".mpeg1");
            picker.FileTypeFilter.Add(".mpeg2");
            picker.FileTypeFilter.Add(".mpeg4");
            picker.FileTypeFilter.Add(".mpg");
            picker.FileTypeFilter.Add(".mpv2");
            picker.FileTypeFilter.Add(".mts");
            picker.FileTypeFilter.Add(".mtv");
            picker.FileTypeFilter.Add(".mxf");
            picker.FileTypeFilter.Add(".mxg");
            picker.FileTypeFilter.Add(".nsv");
            picker.FileTypeFilter.Add(".nut");
            picker.FileTypeFilter.Add(".nuv");
            picker.FileTypeFilter.Add(".ogm");
            picker.FileTypeFilter.Add(".ogv");
            picker.FileTypeFilter.Add(".ogx");
            picker.FileTypeFilter.Add(".ps");
            picker.FileTypeFilter.Add(".rec");
            picker.FileTypeFilter.Add(".rm");
            picker.FileTypeFilter.Add(".rmvb");
            picker.FileTypeFilter.Add(".tod");
            picker.FileTypeFilter.Add(".ts");
            picker.FileTypeFilter.Add(".tts");
            picker.FileTypeFilter.Add(".vob");
            picker.FileTypeFilter.Add(".vro");
            picker.FileTypeFilter.Add(".webm");
            picker.FileTypeFilter.Add(".wm");
            picker.FileTypeFilter.Add(".wmv");
            picker.FileTypeFilter.Add(".wtv");
            picker.FileTypeFilter.Add(".xesc");
            StorageFolder folder = await picker.PickSingleFolderAsync();

            if (folder != null)
            {
                string mru = StorageApplicationPermissions.FutureAccessList.Add(folder);
                App.LocalSettings["DefaultVideoFolder"] = mru;
                await Locator.MainPageVM.InitVideoVM();
            }
        }
    }
}
