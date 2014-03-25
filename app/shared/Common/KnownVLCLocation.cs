/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.Storage;

namespace VLC_WINRT.Common
{
    /// <summary>
    ///     Media sources for VLC.  Any file in these locations matching File Type Associations (ie MKV) included
    ///     with the app will be returns from any storage folder queries.
    ///     Additionally the app must declare the proper capabilities (ie Video Library) to access these locations.
    /// </summary>
    public static class KnownVLCLocation
    {
        public static StorageFolder VideosLibrary
        {
            get { return KnownFolders.VideosLibrary; }
        }

        public static StorageFolder MusicLibrary
        {
            get { return KnownFolders.MusicLibrary; }
        }

        //Combination of USB ports and SD cards.
        public static StorageFolder RemovableDevices
        {
            get { return KnownFolders.RemovableDevices; }
        }

        // Discoverable DLNA servers (I think)
        public static StorageFolder MediaServers
        {
            get { return KnownFolders.MediaServerDevices; }
        }

        //Access libraries of other computers that are in the same 'Home Group' as this machine.
        public static StorageFolder HomeGroup
        {
            get { return KnownFolders.HomeGroup; }
        }
    }
}
