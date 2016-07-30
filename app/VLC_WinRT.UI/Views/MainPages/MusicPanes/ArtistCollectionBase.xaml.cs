using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using VLC.Model.Video;
using VLC.ViewModels;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;

namespace VLC.Views.MainPages.MusicPanes
{
    public sealed partial class ArtistCollectionBase : Page
    {
        public ArtistCollectionBase()
        {
            this.InitializeComponent();
            this.Loaded += ArtistCollectionBase_Loaded;
            this.Unloaded += ArtistCollectionBase_Unloaded;
            this.SizeChanged += ArtistCollectionBase_SizeChanged;
        }

        private void ArtistCollectionBase_Unloaded(object sender, RoutedEventArgs e)
        {
            Locator.MusicLibraryVM.OnNavigatedFromArtists();
        }

        void ArtistCollectionBase_Loaded(object sender, RoutedEventArgs e)
        {
            Locator.MusicLibraryVM.OnNavigatedToArtists();
        }

        private void ArtistCollectionBase_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResponsiveArtistsList();
        }

        void ResponsiveArtistsList()
        {
            var minItemWidth = 300;
            var itemsWrapGrid = ArtistListView?.ItemsPanelRoot as ItemsWrapGrid;
            if (itemsWrapGrid == null) return;
            if (ArtistsGridColumnDefinition.ActualWidth * 2 > minItemWidth)
            {
                itemsWrapGrid.ItemWidth = minItemWidth;
            }
            else if (ArtistsGridColumnDefinition.ActualWidth * 1.75 > minItemWidth)
            {
                itemsWrapGrid.ItemWidth = ArtistListView.ActualWidth / 2;
            }
            else
            {
                itemsWrapGrid.ItemWidth = ArtistListView.ActualWidth;
            }
        }

        private void SemanticZoom_OnViewChangeCompleted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            ArtistsZoomedOutView.ItemsSource = GroupArtists.View.CollectionGroups;
        }

        private void ItemsWrapGrid_Loaded(object sender, RoutedEventArgs e)
        {
            ResponsiveArtistsList();
        }
    }
}