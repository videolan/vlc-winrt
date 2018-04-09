using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC.Helpers;
using VLC.ViewModels;
using Windows.UI.Xaml.Input;
using Windows.System;
using VLC.Model;
using VLC.UI.Views.UserControls.Flyouts;

namespace VLC.UI.Views.MainPages.MusicPanes
{
    public sealed partial class SongCollectionBase : Page
    {
        private ListViewItem focussedListViewItem;

        public SongCollectionBase()
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
            width = DeviceHelper.IsPortrait() ? Window.Current.Bounds.Width : 400;
            grid.ItemWidth = (width - 48) / 4;
            grid.ItemHeight = grid.ItemWidth;
        }

        private void TracksZoomedInView_GotFocus(object sender, RoutedEventArgs e)
        {
            ListView list = (ListView)sender;
            if (FocusManager.GetFocusedElement() as ListViewItem != null)
            {
                focussedListViewItem = (ListViewItem)FocusManager.GetFocusedElement();
            }
        }

        private void TracksZoomedInView_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            ListView list = (ListView)sender;
            if (Locator.NavigationService.CurrentPage == VLCPage.MainPageMusic
                && e.Key == VirtualKey.GamepadView)
            {
                var menu = new TrackItemFlyout(TracksZoomedInView.ItemFromContainer(focussedListViewItem));
                menu.ShowAt(focussedListViewItem);
            }
        }
    }
}
