using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VLC.Model;
using VLC.Model.Music;
using VLC.Model.Video;
using VLC.ViewModels;
using VLC.Model.Search;
using Windows.Storage;
using System.Collections.ObjectModel;
using VLC.Utils;

namespace VLC.Helpers
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
                if (results != null && !results.Contains(album)) 
                    results.Add(album);
            }
            foreach (var result in results?.ToList())
            {
                if (!albums.Contains(result))
                    results.Remove(result);
            }
            return results;
        }

        public static async Task<ObservableCollection<SearchResult>> SearchVideosGeneric(string tag, ObservableCollection<SearchResult> results)
        {
            var videoVms = SearchVideoItems(tag);
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
            var videos = SearchVideoItems(tag);
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
            return Locator.MediaLibrary.LoadArtists(x => x.Name.Contains(tag, StringComparison.CurrentCultureIgnoreCase));
        }

        public static Task<List<AlbumItem>> SearchAlbumItems(string tag)
        {
            return Locator.MediaLibrary.Contains(nameof(AlbumItem.Name), tag);
        }

        public static List<VideoItem> SearchVideoItems(string tag)
        {
            return Locator.MediaLibrary.ContainsVideo(nameof(VideoItem.Name), tag);
        }

        public static IEnumerable<VideoItem> SearchShowItems(string tag)
        {
            return Locator.MediaLibrary.Shows.SelectMany(show => show.Episodes).Where(x => x.Name.Contains(tag, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
