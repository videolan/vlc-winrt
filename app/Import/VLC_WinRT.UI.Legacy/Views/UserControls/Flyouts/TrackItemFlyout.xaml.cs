using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC;
using VLC.Model;
using VLC.Model.Music;
using VLC.ViewModels;

namespace VLC_WinRT.Views.UserControls.Flyouts
{
    public partial class TrackItemFlyout
    {
        public TrackItemFlyout()
        {
            this.InitializeComponent();
#if WINDOWS_PHONE_APP
            this.Placement = FlyoutPlacementMode.Full;
#endif
            this.Opened += TrackItemFlyout_Opened;
        }

        private void TrackItemFlyout_Opened(object sender, object e)
        {
            Locator.MusicLibraryVM.OnNavigatedTo();
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            Locator.MusicLibraryVM.CurrentTrackCollection = e.AddedItems[0] as PlaylistItem;
            Locator.MusicLibraryVM.AddToPlaylistCommand.Execute((this.Content as FrameworkElement).DataContext as TrackItem);
            (sender as ListView).SelectedIndex = -1;
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        void Hide()
        {
            var trackFlyout = App.Current.Resources["TrackItemFlyout"] as Flyout;
            trackFlyout.Hide();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var root = sender as FrameworkElement;
#if WINDOWS_PHONE_APP
            root.MaxHeight = 500;
            root.Margin = new Thickness(24,0,24,0);
#else
            root.MaxHeight = 600;
            root.MaxWidth = 400;
#endif
        }
    }
}
