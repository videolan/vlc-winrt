using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MusicVM;
using VLC_WINRT_APP.ViewModels.VideoVM;
using VLC_WINRT_APP.Views.MusicPages;

namespace VLC_WINRT_APP.Views.UserControls
{
    public sealed partial class LeftColumn : UserControl
    {
        public LeftColumn()
        {
            this.InitializeComponent();
            Window.Current.SizeChanged += CurrentOnSizeChanged;
        }

        private void CurrentOnSizeChanged(object sender, WindowSizeChangedEventArgs windowSizeChangedEventArgs)
        {
            if (Window.Current.Bounds.Width < 1080)
            {
                ColumnGrid.Width = 100;
                ToMediumVisualState();
            }
            else
            {
                ColumnGrid.Width = 340;
                ToNormalVisualState();
            }
        }

        void ToMediumVisualState()
        {
            TitleTextBlock.Visibility = Visibility.Collapsed;
            LargeSearchBox.Visibility = Visibility.Collapsed;
            LittleSearchBox.Visibility = Visibility.Visible;

            HeaderGrid.Margin = new Thickness(0);
            HeaderGrid.HorizontalAlignment = HorizontalAlignment.Center;

            PanelsListView.ItemTemplate = App.Current.Resources["SidebarIconItemTemplate"] as DataTemplate;

            MiniPlayersRowDefinition.Height = new GridLength(0);
            SeparatorRowDefinition.Height = new GridLength(0);
        }

        void ToNormalVisualState()
        {
            TitleTextBlock.Visibility = Visibility.Visible;
            LargeSearchBox.Visibility = Visibility.Visible;
            LittleSearchBox.Visibility = Visibility.Collapsed;

            HeaderGrid.Margin = new Thickness(42, 0, 20, 0);
            HeaderGrid.HorizontalAlignment = HorizontalAlignment.Left;
            PanelsListView.ItemTemplate = App.Current.Resources["SidebarItemTemplate"] as DataTemplate;

            MiniPlayersRowDefinition.Height = new GridLength(315);
            SeparatorRowDefinition.Height = new GridLength(24);
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
