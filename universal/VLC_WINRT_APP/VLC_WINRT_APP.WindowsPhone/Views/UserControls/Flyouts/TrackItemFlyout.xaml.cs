using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MusicVM;

namespace VLC_WINRT_APP.Views.UserControls.Flyouts
{
    public sealed partial class TrackItemFlyout : UserControl
    {
        public TrackItemFlyout()
        {
            this.InitializeComponent();
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            Locator.MusicLibraryVM.CurrentTrackCollection = e.AddedItems[0] as TrackCollection;
            Locator.MusicLibraryVM.AddToPlaylistCommand.Execute(this.DataContext as TrackItem);
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
    }
}
