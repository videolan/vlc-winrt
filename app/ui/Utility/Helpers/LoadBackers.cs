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
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace VLC_WINRT.Utility.Helpers
{
    public static class LoadBackers
    {
        public static async void Get()
        {
            string path = @"backers.json";
            StorageFolder folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile file = await folder.GetFileAsync(path);
            string json = await FileIO.ReadTextAsync(file);
            //var items = JsonConvert.DeserializeObject(json);
            Debug.WriteLine(items.ToString());
        }
    }
}
