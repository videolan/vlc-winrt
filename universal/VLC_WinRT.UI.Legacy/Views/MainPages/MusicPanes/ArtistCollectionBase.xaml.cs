using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Utils;
using Windows.UI.Xaml.Data;
using ScrollWatchedSelector;

namespace VLC_WinRT.Views.MainPages.MusicPanes
{
    public sealed partial class ArtistCollectionBase : UserControl
    {
        public ArtistCollectionBase()
        {
            this.InitializeComponent();
            this.Loaded += ArtistCollectionBase_Loaded;
        }

        async void ArtistCollectionBase_Loaded(object sender, RoutedEventArgs e)
        {
            await Locator.MusicLibraryVM.MusicCollectionLoaded.Task;
            if (Locator.MusicLibraryVM.Artists.Count > Numbers.SemanticZoomItemCountThreshold)
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