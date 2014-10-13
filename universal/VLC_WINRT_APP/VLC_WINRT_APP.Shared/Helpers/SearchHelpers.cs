using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.Model.Video;
using VLC_WINRT_APP.ViewModels;
#if WINDOWS_APP
using Windows.ApplicationModel.Search;
#endif
namespace VLC_WINRT_APP.Helpers
{
    public static class SearchHelpers
    {
#if WINDOWS_APP
        public static void Search(string tag, SearchBoxSuggestionsRequestedEventArgs args)
        {
            if (string.IsNullOrEmpty(tag))
                return;

            tag = tag.ToLower();
            SearchSuggestionsRequestDeferral deferral = args.Request.GetDeferral();
            
            // We don't need null checks here, because even if nothing was found, the Enumerable items will be loaded with zero items in them.
            // So the foreach loops will skip past them.
            IEnumerable<TrackItem> trackItems = Locator.MusicLibraryVM.Tracks.Where(x => x.Name.ToLower().Contains(tag));
            foreach (TrackItem item in trackItems)
            {
                args.Request.SearchSuggestionCollection.AppendResultSuggestion(item.Name, "track", "track://" + item.Id, RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Icons/music.png")), "music");
            }

            IEnumerable<VideoItem> videoVms = Locator.VideoLibraryVM.Videos.Where(x => x.Title.ToLower().Contains(tag));
            foreach (VideoItem vm in videoVms)
            {
                args.Request.SearchSuggestionCollection.AppendResultSuggestion(vm.Title, "video", "video://" + vm.Title,
                    RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Icons/Video.png")), "video");
            }

            IEnumerable<ArtistItem> artistItems =
                Locator.MusicLibraryVM.Artists.Where(x => x.Name.ToLower().Contains(tag));

            foreach (var artistItem in artistItems)
            {
                args.Request.SearchSuggestionCollection.AppendResultSuggestion(artistItem.Name, "artist", "artist://" + artistItem.Id, RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Icons/music.png")), "music");
            }

            IEnumerable<AlbumItem> albumItems =
                Locator.MusicLibraryVM.Artists.SelectMany(node => node.Albums)
                    .Where(x => x.Name.ToLower().Contains(tag));

            foreach (AlbumItem albumItem in albumItems)
            {
                args.Request.SearchSuggestionCollection.AppendResultSuggestion(albumItem.Name, "album", "album://" + albumItem.Id, RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Icons/music.png")), "music");
            }

            deferral.Complete();
        }
#endif
    }
}
