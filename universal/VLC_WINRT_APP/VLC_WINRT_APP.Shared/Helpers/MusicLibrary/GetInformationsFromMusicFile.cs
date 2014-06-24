/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Threading.Tasks;
using VLC_WINRT_APP.ViewModels.MusicVM;
using Windows.Storage;

namespace VLC_WINRT_APP.Helpers.MusicLibrary
{
    public static class GetInformationsFromMusicFile
    {
        public async static Task<MusicLibraryVM.TrackItem> GetTrackItemFromFile(StorageFile track)
        {
            var trackInfos = await track.Properties.GetMusicPropertiesAsync();
            var trackItem = new MusicLibraryVM.TrackItem
            {
                ArtistName = string.IsNullOrEmpty(trackInfos.Artist) ? "Unknown artist" : trackInfos.Artist,
                AlbumName = trackInfos.Album,
                Name = trackInfos.Title,
                Path = track.Path,
                Duration = trackInfos.Duration,
                Index = 0
            };
            return trackItem;
        }
    }
}
