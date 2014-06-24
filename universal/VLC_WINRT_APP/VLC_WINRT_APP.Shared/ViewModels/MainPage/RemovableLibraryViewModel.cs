/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.Storage;
using VLC_WINRT_APP.ViewModels.VideoVM;

namespace VLC_WINRT.ViewModels.MainPage
{
    public class RemovableLibraryViewModel : VideoLibraryVM
    {
        public RemovableLibraryViewModel(StorageFolder location, string id) 
        {
            Id = id;
        }

        public string Id { get; private set; }
    }
}
