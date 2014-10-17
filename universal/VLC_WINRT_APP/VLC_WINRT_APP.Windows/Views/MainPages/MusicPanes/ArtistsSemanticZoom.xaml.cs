using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Views.MainPages.MusicPanes
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class ArtistsSemanticZoom : Page
    {
        public ArtistsSemanticZoom()
        {
            this.InitializeComponent();
        }
        private void Header_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SemanticZoomAlbumsByArtist.IsZoomedInViewActive = false;
        }

        private void SemanticZoom_OnViewChangeCompletedArtistByName(object sender, SemanticZoomViewChangedEventArgs e)
        {
            //Locator.MusicLibraryVM.ExecuteSemanticZoom(SemanticZoomAlbumsByArtist, AlbumsGroupedByName);
        }
    }
}
