using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VLC_WinRT.Model.Video;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VLC_WinRT.UI.Legacy.Views.MusicPages.ArtistPageControls
{
    public sealed partial class ArtistAlbumsSemanticView : UserControl
    {
        public ArtistAlbumsSemanticView()
        {
            this.InitializeComponent();
            this.Loaded += ArtistAlbumsSemanticView_Loaded;
        }

        private void ArtistAlbumsSemanticView_Loaded(object sender, RoutedEventArgs e)
        {
            AlbumsListView.SizeChanged += AlbumsListViewOnSizeChanged;
            ResponsiveTracksListView();
            this.Unloaded += ArtistAlbumsSemanticView_Unloaded;
        }

        private void ArtistAlbumsSemanticView_Unloaded(object sender, RoutedEventArgs e)
        {
            AlbumsListView.SizeChanged -= AlbumsListViewOnSizeChanged;
        }

        private void ZoomedOutItemsWrapGrid_Loaded(object sender, RoutedEventArgs e)
        {
            AlbumsSemanticZoomZoomedOut.SizeChanged += ZoomedOutItemsWrapGrid_SizeChanged;
            ResponsiveAlbumsWrapGrid();
        }

        private void AlbumsListViewOnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            ResponsiveTracksListView();
        }

        private void ZoomedOutItemsWrapGrid_SizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            ResponsiveAlbumsWrapGrid();
        }

        void ResponsiveTracksListView()
        {
            var wrapGrid = AlbumsListView.ItemsPanelRoot as ItemsWrapGrid;
            if (wrapGrid == null) return;
            if (AlbumsSemanticZoom.IsZoomedInViewActive)
                TemplateSizer.ComputeAlbumTracks(ref wrapGrid, AlbumsListView.ActualWidth - wrapGrid.Margin.Left - wrapGrid.Margin.Right);
        }

        void ResponsiveAlbumsWrapGrid()
        {
            var wrapGridZoomedOut = AlbumsSemanticZoomZoomedOut.ItemsPanelRoot as ItemsWrapGrid;
            TemplateSizer.ComputeAlbums(wrapGridZoomedOut, AlbumsSemanticZoomZoomedOut.ActualWidth - wrapGridZoomedOut.Margin.Left - wrapGridZoomedOut.Margin.Right);
        }

        private void SemanticZoom_ViewChangeCompleted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            AlbumsSemanticZoomZoomedOut.ItemsSource = GroupAlbums.View.CollectionGroups;
        }
    }
}
