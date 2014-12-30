using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VLC_WINRT_APP.Views.MainPages.MainMusicControls
{
    public sealed partial class SongsPivotItem : Page
    {
        public SongsPivotItem()
        {
            this.InitializeComponent();
        }

        private void SemanticZoom_OnViewChangeCompleted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            if (TracksZoomedOutView.ItemsSource == null)
                TracksZoomedOutView.ItemsSource = GroupTracks.View.CollectionGroups;
        }

        private void ItemsWrapGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var width = Window.Current.Bounds.Width;
            var grid = sender as ItemsWrapGrid;
            grid.ItemWidth = (width - 48)/4;
            grid.ItemHeight = grid.ItemWidth;
        }
    }
}
