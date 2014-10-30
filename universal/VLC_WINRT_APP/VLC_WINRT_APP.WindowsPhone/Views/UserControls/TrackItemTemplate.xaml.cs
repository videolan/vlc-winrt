using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MusicVM;

namespace VLC_WINRT_APP.Views.UserControls
{
    public sealed partial class TrackItemTemplate : UserControl
    {
        public TrackItemTemplate()
        {
            this.InitializeComponent();
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Locator.MusicLibraryVM.CurrentTrackCollection = e.AddedItems[0] as TrackCollection;
            Locator.MusicLibraryVM.AddToPlaylistCommand.Execute((this.DataContext as TrackItem).Id);
        }
    }
}
