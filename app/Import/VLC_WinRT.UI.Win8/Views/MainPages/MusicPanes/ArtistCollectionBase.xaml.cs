using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC.Model;
using VLC.ViewModels;

namespace VLC_WinRT.Views.MainPages.MusicPanes
{
    public sealed partial class ArtistCollectionBase : UserControl
    {
        public ArtistCollectionBase()
        {
            this.InitializeComponent();
            this.Loaded += ArtistCollectionBase_Loaded;
            this.Unloaded += ArtistCollectionBase_Unloaded;
        }

        private void ArtistCollectionBase_Unloaded(object sender, RoutedEventArgs e)
        {
            if (Locator.NavigationService.CurrentPage != VLCPage.ArtistShowsPage)
            {
                Locator.MusicLibraryVM.OnNavigatedFrom();
            }
        }

        void ArtistCollectionBase_Loaded(object sender, RoutedEventArgs e)
        {
            Locator.MusicLibraryVM.OnNavigatedTo();
        }

        private void SemanticZoom_OnViewChangeCompleted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            ArtistsZoomedOutView.ItemsSource = GroupArtists.View.CollectionGroups;
        }
    }
}