using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC.Model.Music;
using VLC.ViewModels;
using VLC.ViewModels.MusicVM;
using Windows.UI.Xaml.Controls.Primitives;
using VLC.Model;

namespace VLC.Views.UserControls.Flyouts
{
    public partial class TrackItemFlyout
    {
        public TrackItemFlyout()
        {
            this.InitializeComponent();
            this.Opened += TrackItemFlyout_Opened;
        }

        private void TrackItemFlyout_Opened(object sender, object e)
        {
            Locator.MusicLibraryVM.OnNavigatedToPlaylists();
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
            root.MaxHeight = 600;
            root.MaxWidth = 400;
        }
    }
}
