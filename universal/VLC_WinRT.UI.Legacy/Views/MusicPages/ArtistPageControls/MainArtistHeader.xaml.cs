using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WinRTXamlToolkit.Controls.Extensions;
namespace VLC_WinRT.Views.MusicPages.ArtistPageControls
{
    public sealed partial class MainArtistHeader : UserControl
    {
        ContentPresenter parent;
        public MainArtistHeader()
        {
            this.InitializeComponent();
        }

        void SwitchBetweenViewsButton_Click(object sender, RoutedEventArgs e)
        {
            if(IsArtistAlbumsList(parent))
                parent.Content = new ArtistTracksGroupedList();
            else parent.Content = new ArtistAlbumsList();
        }

        private void SwitchBetweenViewsButton_Loaded(object sender, RoutedEventArgs e)
        {
            parent = GetContentPresenter();
            if (IsArtistAlbumsList(parent))
            {
                SwitchBetweenViewsButton.Label = "Tracks";
                SwitchBetweenViewsGlyph.Glyph = App.Current.Resources["DropdownSymbol"].ToString();
            }
            else
            {
                SwitchBetweenViewsGlyph.Glyph = App.Current.Resources["AlbumDiscSymbol"].ToString();
                SwitchBetweenViewsButton.Label = "Albums";
            }
        }

        bool IsArtistAlbumsList(ContentPresenter cP)
        {
            return cP.Content is ArtistAlbumsList;
        }

        ContentPresenter GetContentPresenter()
        {
            var artistPage = this.GetFirstAncestorOfType<ArtistPageBase>();
            return artistPage.FindName("ArtistPageBaseContent") as ContentPresenter;
        }
    }
}
