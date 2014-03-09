using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using VLC_WINRT.ViewModels.MainPage;

namespace VLC_WINRT.Utility.Helpers.MusicLibrary
{
    public static class GetInformationsFromMusicFile
    {
        public async static Task<MusicLibraryViewModel.TrackItem> GetTrackItemFromFile(StorageFile track, string Artist, string Name, int i)
        {
            var trackInfos = await track.Properties.GetMusicPropertiesAsync();
            MusicLibraryViewModel.TrackItem trackItem = new MusicLibraryViewModel.TrackItem();
            trackItem.ArtistName = Artist;
            trackItem.AlbumName = Name;
            trackItem.Name = trackInfos.Title;
            trackItem.Path = track.Path;
            trackItem.Duration = trackInfos.Duration;
            trackItem.Index = i;
            return trackItem;
        }
        public async static Task<MusicLibraryViewModel.TrackItem> GetTrackItemFromFile(StorageFile track)
        {
            var trackInfos = await track.Properties.GetMusicPropertiesAsync();
            MusicLibraryViewModel.TrackItem trackItem = new MusicLibraryViewModel.TrackItem();
            trackItem.ArtistName = trackInfos.Artist;
            trackItem.AlbumName = trackInfos.Album;
            trackItem.Name = trackInfos.Title;
            trackItem.Path = track.Path;
            trackItem.Duration = trackInfos.Duration;
            trackItem.Index = 0;
            return trackItem;
        }

        public async static Task<MusicLibraryViewModel.AlbumItem> GetAlbumItemFromFolder(StorageFolder item, StorageFolderQueryResult albumQueryResult)
        {
            var musicAttr = await item.Properties.GetMusicPropertiesAsync();
            var files = await item.GetFilesAsync(CommonFileQuery.OrderByMusicProperties);
            var thumbnail = await item.GetThumbnailAsync(ThumbnailMode.MusicView, 250);
            var albumItem = new MusicLibraryViewModel.AlbumItem(thumbnail, files, musicAttr.Album, albumQueryResult.Folder.DisplayName);
            return albumItem;
        }
    }
}
