using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MusicVM;

namespace VLC_WINRT_APP.Views.UserControls.Flyouts
{
    public sealed partial class TrackItemFlyout : Flyout
    {
        public TrackItemFlyout()
        {
            this.InitializeComponent();
#if WINDOWS_PHONE_APP
            this.Placement = FlyoutPlacementMode.Full;
#endif
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            Locator.MusicLibraryVM.CurrentTrackCollection = e.AddedItems[0] as TrackCollection;
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
            root.MaxHeight = 500;
            root.MaxWidth = 400;
#endif
        }
    }
}
