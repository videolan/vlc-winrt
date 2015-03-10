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
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.ViewModels.MusicVM;
using Windows.Storage;
using Windows.Storage.FileProperties;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Helpers.MusicLibrary
{
    public static class GetInformationsFromMusicFile
    {
        public async static Task<TrackItem> GetTrackItemFromFile(StorageFile track)
        {
            //TODO: Warning, is it safe to consider this a good idea?
            var trackItem = await Locator.MusicLibraryVM._trackDataRepository.LoadTrackByPath(track.Path);
            if (trackItem != null)
            {
                return trackItem;
            }

            MusicProperties trackInfos = null;
            try
            {
                trackInfos = await track.Properties.GetMusicPropertiesAsync();
            }
            catch
            {

            }
            trackItem = new TrackItem
            {
                ArtistName = (trackInfos == null || string.IsNullOrEmpty(trackInfos.Artist)) ? "Unknown artist" : trackInfos.Artist,
                AlbumName = (trackInfos == null) ? "Uknown album" : trackInfos.Album,
                Name = (trackInfos == null || string.IsNullOrEmpty(trackInfos.Title)) ? track.DisplayName : trackInfos.Title,
                Path = track.Path,
                Duration = (trackInfos == null) ? TimeSpan.Zero : trackInfos.Duration,
                Index = 0
            };
            return trackItem;
        }
    }
}
