using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WinRT.Model.Music;
using VLC_WinRT.ViewModels;
using Windows.UI.Xaml.Data;
using VLC_WinRT.Utils;
using VLC_WinRT.Helpers.MusicLibrary;

namespace VLC_WinRT.Views.MainPages.MusicPanes.ArtistCollectionPanes
{
    public sealed partial class ArtistsListView : UserControl
    {
        public ArtistsListView()
        {
            this.InitializeComponent();
            this.Loaded += ArtistCollectionBase_Loaded;
        }

        async void ArtistCollectionBase_Loaded(object sender, RoutedEventArgs e)
        {
            await Locator.MusicLibraryVM.MusicCollectionLoaded.Task;
            if(Locator.MusicLibraryVM.Artists.Count > Numbers.SemanticZoomItemCountThreshold)
            {
                var b = new Binding();
                b.Mode = BindingMode.OneWay;
                b.Source = this.Resources["GroupArtists"] as CollectionViewSource;
                ArtistListView.SetBinding(ListView.ItemsSourceProperty, b);
                SemanticZoom.IsZoomOutButtonEnabled = true;
            }
        }

        private void SemanticZoom_OnViewChangeCompleted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            ArtistsZoomedOutView.ItemsSource = GroupArtists.View.CollectionGroups;
        }
    }
}
