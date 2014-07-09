using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.Search;
using Windows.Networking.Vpn;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MusicVM;
using VLC_WINRT_APP.ViewModels.VideoVM;

namespace VLC_WINRT_APP.Helpers
{
    public static class SearchHelpers
    {
        public static void Search(string tag, SearchBoxSuggestionsRequestedEventArgs args)
        {
            if (string.IsNullOrEmpty(tag))
                return;
            tag = tag.ToLower();
            SearchSuggestionsRequestDeferral deferral = args.Request.GetDeferral();
            
            IEnumerable<MusicLibraryVM.TrackItem> trackItems = Locator.MusicLibraryVM.Tracks.Where(x => x.Name.ToLower().Contains(tag));
            if (trackItems != null && trackItems.Any())
            {
                foreach (MusicLibraryVM.TrackItem item in trackItems)
                {
                    args.Request.SearchSuggestionCollection.AppendResultSuggestion(item.Name, "track", "track://" + item.Id, RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Icons/music.png")), "music");
                }
            }

            IEnumerable<VideoVM> videoVms = Locator.VideoLibraryVM.Videos.Where(x => x.Title.ToLower().Contains(tag));
            if (videoVms != null && videoVms.Any())
            {
                foreach (VideoVM vm in videoVms)
                {
                    args.Request.SearchSuggestionCollection.AppendResultSuggestion(vm.Title, "video", "video://" + vm.Title,
                        RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Icons/Video.png")), "video");
                }
            }

            IEnumerable<MusicLibraryVM.ArtistItem> artistItems =
                Locator.MusicLibraryVM.Artists.Where(x => x.Name.ToLower().Contains(tag));
            if (artistItems != null && artistItems.Any())
            {
                foreach (MusicLibraryVM.ArtistItem artistItem in artistItems)
                {
                    IEnumerable<MusicLibraryVM.AlbumItem> albumItems =
                        artistItem.Albums.Where(x => x.Name.ToLower().Contains(tag));
                    if (albumItems != null && albumItems.Any())
                    {
                        foreach (MusicLibraryVM.AlbumItem albumItem in albumItems)
                        {
                            args.Request.SearchSuggestionCollection.AppendResultSuggestion(albumItem.Name, "album", "album://" + albumItem.Id, RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Icons/music.png")), "music");
                        }
                    }
                    args.Request.SearchSuggestionCollection.AppendResultSuggestion(artistItem.Name, "artist", "artist://" + artistItem.Id, RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Icons/music.png")), "music");
                }
            }
            deferral.Complete();
        }
    }
}
