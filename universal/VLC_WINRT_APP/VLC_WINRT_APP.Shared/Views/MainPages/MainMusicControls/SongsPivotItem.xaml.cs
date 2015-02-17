using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Helpers;

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
            Responsive(sender as ItemsWrapGrid);
        }

        private void ItemsWrapGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Responsive(sender as ItemsWrapGrid);
        }

        void Responsive(ItemsWrapGrid grid)
        {
            if (grid == null) return;
            double width;
            width = DisplayHelper.IsPortrait() ? Window.Current.Bounds.Width : 400;
            grid.ItemWidth = (width - 48) / 4;
            grid.ItemHeight = grid.ItemWidth;
        }
    }
}
