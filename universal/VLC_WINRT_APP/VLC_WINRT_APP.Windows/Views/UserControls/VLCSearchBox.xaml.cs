using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Helpers;
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
            int separatorIndex = args.Tag.IndexOf("://");
            int separatorEndIndex = separatorIndex + 3;
            string type = args.Tag.Remove(separatorIndex);
            string query = args.Tag.Remove(0, separatorEndIndex);
            switch (type)
            {
                case "track":
                    MusicLibraryVM.TrackItem trackItem = await MusicLibraryVM._trackDataRepository.LoadTrack(int.Parse(query));
                    if (trackItem != null)
                        Task.Run(() => trackItem.Play());
                    break;
                case "album":
                    MusicLibraryVM.AlbumItem albumItem =
                        await MusicLibraryVM._albumDataRepository.LoadAlbum(int.Parse(query));
                    if (albumItem != null)
                        Task.Run(() => albumItem.Play());
                    break;
                case "artist":
                    MusicLibraryVM.ArtistItem artistItem =
                        await MusicLibraryVM._artistDataRepository.LoadArtist(int.Parse(query));
#if WINDOWS_APP
                    App.ApplicationFrame.Navigate(typeof(ArtistPage));
#endif
                    Locator.MusicLibraryVM.CurrentArtist = artistItem;
                    break;
                case "video":
                    VideoVM vm = Locator.VideoLibraryVM.Videos.FirstOrDefault(x => x.Title == query);
                    vm.Play();
                    break;
            }
        }
    }
}
