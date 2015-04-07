using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VLC_WinRT.Helpers.VideoPlayer;
using VLC_WinRT.Model;
using VLC_WinRT.Model.Music;
using VLC_WinRT.Model.Video;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Model.Search;
using Windows.Storage;
using System.Collections.ObjectModel;

namespace VLC_WinRT.Helpers
{
    public static class SearchHelpers
    {
        public static async Task OpenSearchItem(VLCItemType type, string query, int idquery)
        {
            switch (type)
            {
                case VLCItemType.Track:
                    TrackItem trackItem = Locator.MusicLibraryVM.Tracks.FirstOrDefault(node => node.Id == idquery);
                    if (trackItem != null)
                    {
                        Locator.MusicLibraryVM.TrackClickedCommand.Execute(trackItem);
                    }
                    break;
                case VLCItemType.Album:
                    Locator.MusicLibraryVM.AlbumClickedCommand.Execute(idquery);
                    break;
                case VLCItemType.Artist:
                    Locator.MusicLibraryVM.ArtistClickedCommand.Execute(idquery);
                    break;
                case VLCItemType.Video:
                    VideoItem vm = Locator.VideoLibraryVM.Videos.FirstOrDefault(x => x.Name == query);
                    await vm.Play();
                    break;
                case VLCItemType.VideoShow:
                    VideoItem show = Locator.VideoLibraryVM.Shows.SelectMany(x => x.Episodes).FirstOrDefault(x => x.Name == query);
                    await show.Play();
                    break;
                case VLCItemType.VideoCamera:
                    VideoItem camera = Locator.VideoLibraryVM.CameraRoll.FirstOrDefault(x => x.Name == query);
                    await camera.Play();
                    break;
            }
        }

        public static void Search()
        {
            if (string.IsNullOrEmpty(Locator.MainVM.SearchTag)) return;
            var results = new ObservableCollection<SearchResult>();
            Locator.MainVM.SearchResults.Clear();
            // We don't need null checks here, because even if nothing was found, the Enumerable items will be loaded with zero items in them.
            // So the foreach loops will skip past them.
            if (Locator.SettingsVM.SearchTracks)
            {
                var tracks = SearchTracks(Locator.MainVM.SearchTag);
                foreach (var track in tracks)
                {
                    results.Add(track);
                }
            }

            if (Locator.SettingsVM.SearchVideos)
            {
                var videos = SearchVideosGeneric(Locator.MainVM.SearchTag, results);
                foreach (var video in videos)
                {
                    results.Add(video);
                }
            }

            if (Locator.SettingsVM.SearchArtists)
            {
                var artists = SearchArtists(Locator.MainVM.SearchTag);
                foreach (var artist in artists)
                {
                    results.Add(artist);
                }
            }

            if (Locator.SettingsVM.SearchAlbums)
            {
                var albums = SearchAlbums(Locator.MainVM.SearchTag);
                foreach (var album in albums)
                {
                    results.Add(album);
                }
            }
            Locator.MainVM.SearchResults = results;
        }


        public static ObservableCollection<SearchResult> SearchArtists(string tag)
        {
            var results = new ObservableCollection<SearchResult>();
            IEnumerable<ArtistItem> artistItems = SearchArtistItems(tag);
            foreach (var artistItem in artistItems)
            {
                results.Add(new SearchResult(artistItem.Name,
                    ApplicationData.Current.LocalFolder.Path + "\\artistPic\\" + artistItem.Id + ".jpg",
                    VLCItemType.Artist, artistItem.Id));
            }
            return results;
        }

        public static ObservableCollection<SearchResult> SearchTracks(string tag)
        {
            var results = new ObservableCollection<SearchResult>();
            IEnumerable<TrackItem> trackItems = SearchTrackItems(tag);
            foreach (TrackItem item in trackItems)
            {
                results.Add(new SearchResult(item.Name, item.Thumbnail,
                    VLCItemType.Track, item.Id));
            }
            return results;
        }

        public static ObservableCollection<SearchResult> SearchAlbums(string tag)
        {
            var results = new ObservableCollection<SearchResult>();
            IEnumerable<AlbumItem> albumItems = SearchAlbumItems(tag);
            foreach (AlbumItem albumItem in albumItems)
            {
                results.Add(new SearchResult(albumItem.Name,
                    ApplicationData.Current.LocalFolder.Path + "\\albumPic\\" + albumItem.Id + ".jpg",
                    VLCItemType.Album,
                    albumItem.Id));
            }
            return results;
        }

        public static ObservableCollection<SearchResult> SearchVideosGeneric(string tag, ObservableCollection<SearchResult> results)
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

            var cameraVms = SearchCameraItems(tag);
            foreach (var cameraVm in cameraVms)
            {
                results.Add(new SearchResult(cameraVm.Name, ApplicationData.Current.LocalFolder.Path + "\\videoPic\\" + cameraVm.Name + ".jpg", VLCItemType.VideoCamera));
            }
            return results;
        }

        public static void SearchVideos(string tag, ObservableCollection<VideoItem> results)
        {
            var videos = SearchVideoItems(tag);
            foreach (var video in videos)
            {
                if (!results.Contains(video))
                    results.Add(video);
            }
            var shows = SearchShowItems(tag);
            foreach (var show in shows)
            {
                if (!results.Contains(show))
                    results.Add(show);
            }

            var cameras = SearchCameraItems(tag);
            foreach (var camera in cameras)
            {
                if (!results.Contains(camera))
                    results.Add(camera);
            }

            foreach (var result in results.ToList())
            {
                if (!videos.Contains(result) && !shows.Contains(result) && !cameras.Contains(result))
                    results.Remove(result);
            }
        }

        public static void SearchMusic(string tag, ObservableCollection<SearchResult> results)
        {
            var albums = SearchAlbums(tag);
            foreach (var album in albums)
            {
                if (!results.Contains(album))
                    results.Add(album);
            }

            var artists = SearchArtists(tag);
            foreach (var artist in artists)
            {
                if (!results.Contains(artist))
                    results.Add(artist);
            }

            var tracks = SearchTracks(tag);
            foreach (var track in tracks)
            {
                if (!results.Contains(track))
                    results.Add(track);
            }

            foreach (var result in results.ToList())
            {
                if (!albums.Contains(result) && !artists.Contains(result) && !tracks.Contains(result))
                    results.Remove(result);
            }
        }


        public static IEnumerable<ArtistItem> SearchArtistItems(string tag)
        {
            return Locator.MusicLibraryVM.Artists.Where(x => x.Name.Contains(tag, StringComparison.CurrentCultureIgnoreCase));
        }

        public static IEnumerable<AlbumItem> SearchAlbumItems(string tag)
        {
            return Locator.MusicLibraryVM.Artists.SelectMany(node => node.Albums)
                    .Where(x => x.Name.Contains(tag, StringComparison.CurrentCultureIgnoreCase));
        }

        public static IEnumerable<TrackItem> SearchTrackItems(string tag)
        {
            return Locator.MusicLibraryVM.Tracks.Where(x => x.Name.Contains(tag, StringComparison.CurrentCultureIgnoreCase));
        }

        public static IEnumerable<VideoItem> SearchVideoItems(string tag)
        {
            return Locator.VideoLibraryVM.Videos.Where(x => x.Name.Contains(tag, StringComparison.CurrentCultureIgnoreCase));
        }

        public static IEnumerable<VideoItem> SearchCameraItems(string tag)
        {
            return Locator.VideoLibraryVM.CameraRoll.Where(x => x.Name.Contains(tag, StringComparison.CurrentCultureIgnoreCase));
        }

        public static IEnumerable<VideoItem> SearchShowItems(string tag)
        {
            return Locator.VideoLibraryVM.Shows.SelectMany(show => show.Episodes).Where(x => x.Name.Contains(tag, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
