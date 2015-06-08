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
            this.SizeChanged += ArtistsListView_SizeChanged;
            this.Unloaded += ArtistsListView_Unloaded;
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

        private void ArtistsListView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.SizeChanged -= ArtistsListView_SizeChanged;
        }

        private void ArtistsListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Responsive();
        }

        void Responsive()
        {
            if (Window.Current.Bounds.Width >= 800)
                ArtistListView.HeaderTemplate = this.Resources["ListViewHeaderTemplate"] as DataTemplate;
            else
                ArtistListView.HeaderTemplate = null;
        }

        private void SemanticZoom_OnViewChangeCompleted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            ArtistsZoomedOutView.ItemsSource = GroupArtists.View.CollectionGroups;
        }
        
        private void ArtistListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!e.AddedItems.Any()) return;
            var artist = e.AddedItems[0] as ArtistItem;
            if (Window.Current.Bounds.Width >= 800)
                Locator.MusicLibraryVM.CurrentArtist = artist;
            else Locator.MusicLibraryVM.ArtistClickedCommand.Execute(artist);
        }
    }
}
