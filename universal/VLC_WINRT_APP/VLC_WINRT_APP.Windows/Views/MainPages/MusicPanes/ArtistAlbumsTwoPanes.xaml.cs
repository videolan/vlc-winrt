using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Views.MainPages.MusicPanes
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class ArtistAlbumsTwoPanes : Page
    {
        public ArtistAlbumsTwoPanes()
        {
            this.InitializeComponent();
        }
        private void SemanticZoom_OnViewChangeCompletedCurrentArtistAlbum(object sender, SemanticZoomViewChangedEventArgs e)
        {
            if (e.IsSourceZoomedInView == false)
            {
                e.DestinationItem.Item = e.SourceItem.Item;
            }
        }
        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (CapabilitiesHelper.IsTouchCapable)
            {
                Locator.MusicLibraryVM.IsMainPageMusicArtistAlbumsSemanticZoomViewedIn = false;
            }
            if (Locator.MusicPlayerVM.IsPlaying
                && Locator.MusicPlayerVM.PlayingType == PlayingType.Music
                && Locator.MusicPlayerVM.CurrentPlayingArtist != null)
            {
                ArtistListView.SelectedItem =
                    Locator.MusicLibraryVM.Artists.FirstOrDefault(
                        x => x.Name == Locator.MusicPlayerVM.CurrentPlayingArtist.Name);
                ArtistListView.ScrollIntoView(ArtistListView.SelectedItem);
            }
            if (ArtistListView.SelectedIndex == -1
                && Window.Current.Bounds.Width > 800)
            {
                if(ArtistListView.Items != null && ArtistListView.Items.Count > 0)
                    ArtistListView.SelectedIndex = 0;
            }
            App.RootPage.ColumnGrid.MinimizeSidebar();
        }
    }
}
