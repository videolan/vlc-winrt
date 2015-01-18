using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VLC_WINRT_APP.Helpers.VideoPlayer;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.Model.Video;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Views.MusicPages;
#if WINDOWS_APP
using Windows.ApplicationModel.Search;
using Windows.UI.Xaml.Controls;
using Windows.Storage.Streams;
#endif
#if WINDOWS_PHONE_APP
using VLC_WINRT_APP.Model.Search;
using Windows.Storage;
#endif

namespace VLC_WINRT_APP.Helpers
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
                    AlbumItem albumItem = Locator.MusicLibraryVM.Albums.FirstOrDefault(x => x.Id == idquery);
                    if (albumItem != null)
                        albumItem.PlayAlbum.Execute(albumItem);
                    break;
                case VLCItemType.Artist:
                    ArtistItem artistItem =
                        Locator.MusicLibraryVM.Artists.FirstOrDefault(node => node.Id == idquery);
                    if (artistItem != null) Locator.MusicLibraryVM.CurrentArtist = artistItem;
                    App.ApplicationFrame.Navigate(typeof(ArtistPage));
                    break;
                case VLCItemType.Video:
                    VideoItem vm = Locator.VideoLibraryVM.Videos.FirstOrDefault(x => x.Title == query);
                    await vm.Play();
                    break;
                case VLCItemType.VideoShow:
                    VideoItem show = Locator.VideoLibraryVM.Shows.SelectMany(x => x.Episodes).FirstOrDefault(x => x.Title == query);
                    await show.Play();
                    break;
                case VLCItemType.VideoCamera:
                    VideoItem camera = Locator.VideoLibraryVM.CameraRoll.FirstOrDefault(x => x.Title == query);
                    await camera.Play();
                    break;
            }
        }
#if WINDOWS_PHONE_APP

        public static void Search()
        {
            if (string.IsNullOrEmpty(Locator.MainVM.SearchTag)) return;
            Locator.MainVM.SearchResults.Clear();
            // We don't need null checks here, because even if nothing was found, the Enumerable items will be loaded with zero items in them.
            // So the foreach loops will skip past them.
            if (Locator.SettingsVM.SearchTracks)
            {
                IEnumerable<TrackItem> trackItems =
                    Locator.MusicLibraryVM.Tracks.Where(x => x.Name.Contains(Locator.MainVM.SearchTag, StringComparison.CurrentCultureIgnoreCase));
                foreach (TrackItem item in trackItems)
                {
                    Locator.MainVM.SearchResults.Add(new SearchResult(item.Name, item.Thumbnail,
                        VLCItemType.Track, item.Id));
                }
            }

            if (Locator.SettingsVM.SearchVideos)
            {
                IEnumerable<VideoItem> videoVms =
                    Locator.VideoLibraryVM.Videos.Where(x => x.Title.Contains(Locator.MainVM.SearchTag, StringComparison.CurrentCultureIgnoreCase));
                foreach (VideoItem vm in videoVms)
                {
                    Locator.MainVM.SearchResults.Add(new SearchResult(vm.Title,
                        ApplicationData.Current.LocalFolder.Path + "\\videoPic\\" + vm.Title + ".jpg",
                        VLCItemType.Video));
                }

                IEnumerable<VideoItem> showsVms = Locator.VideoLibraryVM.Shows.SelectMany(show => show.Episodes).Where(x => x.Title.Contains(Locator.MainVM.SearchTag, StringComparison.CurrentCultureIgnoreCase));
                foreach (var showsVm in showsVms)
                {
                    Locator.MainVM.SearchResults.Add(new SearchResult(showsVm.Title, ApplicationData.Current.LocalFolder.Path + "\\videoPic\\" + showsVm.Title + ".jpg", VLCItemType.VideoShow));
                }

                IEnumerable<VideoItem> cameraVms =
                    Locator.VideoLibraryVM.CameraRoll.Where(x => x.Title.Contains(Locator.MainVM.SearchTag, StringComparison.CurrentCultureIgnoreCase));
                foreach (var cameraVm in cameraVms)
                {
                    Locator.MainVM.SearchResults.Add(new SearchResult(cameraVm.Title, ApplicationData.Current.LocalFolder.Path + "\\videoPic\\" + cameraVm.Title + ".jpg", VLCItemType.VideoCamera));
                }
            }

            if (Locator.SettingsVM.SearchArtists)
            {
                IEnumerable<ArtistItem> artistItems =
                    Locator.MusicLibraryVM.Artists.Where(x => x.Name.Contains(Locator.MainVM.SearchTag, StringComparison.CurrentCultureIgnoreCase));

                foreach (var artistItem in artistItems)
                {
                    Locator.MainVM.SearchResults.Add(new SearchResult(artistItem.Name,
                        ApplicationData.Current.LocalFolder.Path + "\\artistPic\\" + artistItem.Id + ".jpg",
                        VLCItemType.Artist, artistItem.Id));
                }
            }

            if (Locator.SettingsVM.SearchAlbums)
            {
                IEnumerable<AlbumItem> albumItems =
                    Locator.MusicLibraryVM.Artists.SelectMany(node => node.Albums)
                        .Where(x => x.Name.Contains(Locator.MainVM.SearchTag, StringComparison.CurrentCultureIgnoreCase));

                foreach (AlbumItem albumItem in albumItems)
                {
                    Locator.MainVM.SearchResults.Add(new SearchResult(albumItem.Name,
                        ApplicationData.Current.LocalFolder.Path + "\\albumPic\\" + albumItem.Id + ".jpg",
                        VLCItemType.Album,
                        albumItem.Id));
                }
            }
        }

        //}
#endif
#if WINDOWS_APP
        public static void Search(string tag, SearchBoxSuggestionsRequestedEventArgs args)
        {
            if (string.IsNullOrEmpty(tag))
                return;

            SearchSuggestionsRequestDeferral deferral = args.Request.GetDeferral();

            // We don't need null checks here, because even if nothing was found, the Enumerable items will be loaded with zero items in them.
            // So the foreach loops will skip past them.
            IEnumerable<TrackItem> trackItems = Locator.MusicLibraryVM.Tracks.Where(x => x.Name.Contains(tag, StringComparison.CurrentCultureIgnoreCase));
            foreach (TrackItem item in trackItems)
            {
                args.Request.SearchSuggestionCollection.AppendResultSuggestion(item.Name, "3", "track://" + item.Id, RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Icons/music.png")), "music");
            }

            IEnumerable<VideoItem> videoVms = Locator.VideoLibraryVM.Videos.Where(x => x.Title.Contains(tag, StringComparison.CurrentCultureIgnoreCase));
            foreach (VideoItem vm in videoVms)
            {
                args.Request.SearchSuggestionCollection.AppendResultSuggestion(vm.Title, "0", "video://" + vm.Title,
                    RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Icons/Video.png")), "video");
            }

            IEnumerable<ArtistItem> artistItems =
                Locator.MusicLibraryVM.Artists.Where(x => x.Name.Contains(tag, StringComparison.CurrentCultureIgnoreCase));

            foreach (var artistItem in artistItems)
            {
                args.Request.SearchSuggestionCollection.AppendResultSuggestion(artistItem.Name, "2", "artist://" + artistItem.Id, RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Icons/music.png")), "music");
            }

            IEnumerable<AlbumItem> albumItems =
                Locator.MusicLibraryVM.Artists.SelectMany(node => node.Albums)
                    .Where(x => x.Name.Contains(tag, StringComparison.CurrentCultureIgnoreCase));

            foreach (AlbumItem albumItem in albumItems)
            {
                args.Request.SearchSuggestionCollection.AppendResultSuggestion(albumItem.Name, "1", "album://" + albumItem.Id, RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Icons/music.png")), "music");
            }

            deferral.Complete();
        }
#endif
    }
}
