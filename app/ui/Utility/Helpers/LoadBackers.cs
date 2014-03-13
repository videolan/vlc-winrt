/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using VLC_WINRT.Model;
using VLC_WINRT.ViewModels;

namespace VLC_WINRT.Utility.Helpers
{
    public class LoadBackers
    {
        private string[] lines;
        public async Task Get(int position)
        {
            if (lines == null)
            {
                string path = @"backers.csv";
                StorageFolder folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                StorageFile file = await folder.GetFileAsync(path);
                string csv = await FileIO.ReadTextAsync(file);
                csv = csv.Remove(0, 12);
                lines = csv.Split('\n');
            }

            for (int index = position; index < position + 250 && index < lines.Count(); index++)
            {
                var line = lines[index];
                if(line == "")
                {
                    continue;
                }
                string[] item = line.Split(';');

                BackItem backers = new BackItem();
                backers.Name = item[0];
                backers.Country = item[1];
                Locator.MainPageVM.Backers.Add(backers);
            }
        }
    }
}
