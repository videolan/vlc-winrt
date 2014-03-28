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
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using VLC_WINRT.Utility.DataRepository;
using VLC_WINRT.ViewModels.MainPage;

namespace VLC_WINRT.Utility.Helpers.MusicLibrary
{
    public static class GetInformationsFromMusicFile
    {
        public async static Task<MusicLibraryViewModel.TrackItem> GetTrackItemFromFile(StorageFile track)
        {
            var trackInfos = await track.Properties.GetMusicPropertiesAsync();
            var trackItem = new MusicLibraryViewModel.TrackItem
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
