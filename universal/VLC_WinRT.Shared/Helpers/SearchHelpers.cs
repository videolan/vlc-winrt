using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VLC_WinRT.Model;
using VLC_WinRT.Model.Music;
using VLC_WinRT.Model.Video;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Model.Search;
using Windows.Storage;
using System.Collections.ObjectModel;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Helpers
{
    public static class SearchHelpers
    {        
        public static async Task<ObservableCollection<SearchResult>> SearchArtists(string tag)
        {
            var results = new ObservableCollection<SearchResult>();
            var artistItems = await SearchArtistItems(tag);
            foreach (var artistItem in artistItems)
            {
                results.Add(new SearchResult(artistItem.Name, ApplicationData.Current.LocalFolder.Path + "\\artistPic\\" + artistItem.Id + ".jpg",
                    VLCItemType.Artist, artistItem.Id));
            }
            return results;
        }

        public static ObservableCollection<SearchResult> SearchTracks(string tag)
        {
            var results = new ObservableCollection<SearchResult>();
            return results;
        }

        public static async Task<ObservableCollection<SearchResult>> SearchAlbumsGeneric(string tag)
        {
            var results = new ObservableCollection<SearchResult>();
            var albumItems = await SearchAlbumItems(tag);
            foreach (AlbumItem albumItem in albumItems)
            {
                results.Add(new SearchResult(albumItem.Name,
                    ApplicationData.Current.LocalFolder.Path + "\\albumPic\\" + albumItem.Id + ".jpg",
                    VLCItemType.Album,
                    albumItem.Id));
            }
            return results;
        }

        public static async Task<List<AlbumItem>> SearchAlbums(string tag, List<AlbumItem> results)
        {
            var albums = await SearchAlbumItems(tag);
            foreach (var album in albums)
            {
                if (!results.Contains(album)) 
                    results.Add(album);
            }
            foreach (var result in results.ToList())
            {
                if (!albums.Contains(result))
                    results.Remove(result);
            }
            return results;
        }

        public static async Task<ObservableCollection<SearchResult>> SearchVideosGeneric(string tag, ObservableCollection<SearchResult> results)
        {
            var videoVms = await SearchVideoItems(tag);
            foreach (VideoItem vm in videoVms)
            {
                results.Add(new SearchResult(vm.Name, ApplicationData.Current.LocalFolder.Path + "\\videoPic\\" + vm.Name + ".jpg", VLCItemType.Video));
            }

            var showsVms = SearchShowItems(tag);
            foreach (var showsVm in showsVms)
            {
                results.Add(new SearchResult(showsVm.Name, ApplicationData.Current.LocalFolder.Path + "\\videoPic\\" + showsVm.Name + ".jpg", VLCItemType.VideoShow));
            }
            return results;
        }

        public static async Task<List<VideoItem>> SearchVideos(string tag, List<VideoItem> results)
        {
            var videos = await SearchVideoItems(tag);
            foreach (var video in videos)
            {
                if (!results.Contains(video))
                    results.Add(video);
            }

            foreach (var result in results.ToList())
            {
                if (!videos.Contains(result))
                    results.Remove(result);
            }
            return results;
        }

        public static async Task SearchMusic(string tag, ObservableCollection<SearchResult> results)
        {
            var albums = await SearchAlbumsGeneric(tag);
            foreach (var album in albums.Where(album => !results.Contains(album)))
            {
                results.Add(album);
            }
            foreach (var result in results.ToList().Where(result => !albums.Contains(result)))
            {
                results.Remove(result);
            }
        }

        public static Task<List<ArtistItem>> SearchArtistItems(string tag)
        {
            return Locator.MusicLibraryVM.MusicLibrary.LoadArtists(x => x.Name.Contains(tag, StringComparison.CurrentCultureIgnoreCase));
        }

        public static Task<List<AlbumItem>> SearchAlbumItems(string tag)
        {
            return Locator.MusicLibraryVM.MusicLibrary.Contains(nameof(AlbumItem.Name), tag);
        }

        public static Task<List<VideoItem>> SearchVideoItems(string tag)
        {
            return Locator.VideoLibraryVM.VideoRepository.Contains(nameof(VideoItem.Name), tag);
        }

        public static IEnumerable<VideoItem> SearchShowItems(string tag)
        {
            return Locator.VideoLibraryVM.Shows.SelectMany(show => show.Episodes).Where(x => x.Name.Contains(tag, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
