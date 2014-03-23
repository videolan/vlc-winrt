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
        public async static Task<MusicLibraryViewModel.TrackItem> GetTrackItemFromFile(StorageFile track, string artist, string name, int i, int artistId, int albumId)
        {
            var trackInfos = await track.Properties.GetMusicPropertiesAsync();
            var trackItem = new MusicLibraryViewModel.TrackItem
            {
                ArtistName = string.IsNullOrEmpty(artist) ? "Unknown artist" : artist,
                AlbumName = name,
                Name = trackInfos.Title,
                Path = track.Path,
                Duration = trackInfos.Duration,
                Index = i,
                ArtistId = artistId,
                AlbumId = albumId
            };
            return trackItem;
        }
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

        public async static Task<MusicLibraryViewModel.AlbumItem> GetAlbumItemFromFolder(StorageFolder item, StorageFolderQueryResult albumQueryResult, int artistId)
        {
            var albumDataRepository = new AlbumDataRepository();
            var musicAttr = await item.Properties.GetMusicPropertiesAsync();

            var albumItem = await albumDataRepository.LoadAlbumViaName(artistId, musicAttr.Album);
            if (albumItem == null)
            {
                var thumbnail = await item.GetThumbnailAsync(ThumbnailMode.MusicView, 250);

                albumItem = new MusicLibraryViewModel.AlbumItem(thumbnail, musicAttr.Album, albumQueryResult.Folder.DisplayName)
                {
                    ArtistId = artistId
                };
                await albumDataRepository.Add(albumItem);
            }
            var files = await item.GetFilesAsync(CommonFileQuery.OrderByMusicProperties);
            await albumItem.LoadTracks(files);
            return albumItem;
        }
    }
}
