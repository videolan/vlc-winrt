using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.Model.Video;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MusicVM;
using VLC_WINRT_APP.ViewModels.VideoVM;
using VLC_WINRT_APP.Views.MusicPages;

namespace VLC_WINRT_APP.Views.UserControls
{
    public sealed partial class VLCSearchBox : UserControl
    {
        public VLCSearchBox()
        {
            this.InitializeComponent();
        }
        private void LargeSearchBox_SuggestionsRequested(SearchBox sender, SearchBoxSuggestionsRequestedEventArgs args)
        {
            SearchHelpers.Search(args.QueryText, args);
        }

        private void LargeSearchBox_QuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
        }

        private async void LargeSearchBox_OnResultSuggestionChosen(SearchBox sender, SearchBoxResultSuggestionChosenEventArgs args)
        {
            int separatorIndex = args.Tag.IndexOf("://", System.StringComparison.Ordinal);
            int separatorEndIndex = separatorIndex + 3;
            string type = args.Tag.Remove(separatorIndex);
            string query = args.Tag.Remove(0, separatorEndIndex);
            // Instead of searching the database, search the music library VM. This way we already have the track and album information and
            // don't have to call the database for it again.
            switch (type)
            {
                case "track":
                    TrackItem trackItem = Locator.MusicLibraryVM.Tracks.FirstOrDefault(node => node.Id == int.Parse(query));
                    if (trackItem != null)
                    {
                        Locator.MusicPlayerVM.CurrentPlayingArtist = Locator.MusicLibraryVM.Artists.FirstOrDefault(node => node.Id == trackItem.ArtistId);
                        await Task.Run(() => trackItem.Play());
                    }
                    break;
                case "album":
                    AlbumItem albumItem =
                        Locator.MusicLibraryVM.Artists.SelectMany(node => node.Albums)
                            .FirstOrDefault(node => node.Id == int.Parse(query));
                    if (albumItem != null)
                    {
                        Locator.MusicLibraryVM.CurrentArtist = Locator.MusicLibraryVM.Artists.FirstOrDefault(node => node.Id == albumItem.ArtistId);
                        await Task.Run(() => albumItem.Play());
                    }
                    break;
                case "artist":
                    ArtistItem artistItem =
                        Locator.MusicLibraryVM.Artists.FirstOrDefault(node => node.Id == int.Parse(query));
                   Locator.MusicLibraryVM.CurrentArtist = artistItem;
#if WINDOWS_APP
                    App.ApplicationFrame.Navigate(typeof(ArtistPage));
#endif
                    break;
                case "video":
                    VideoVM vm = Locator.VideoLibraryVM.Videos.FirstOrDefault(x => x.Title == query);
                    await vm.Play();
                    break;
            }
        }
    }
}
