using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Helpers.MusicLibrary;

namespace VLC_WINRT_APP.Views.MusicPages
{
    public sealed partial class AddAlbumToPlaylist : ContentDialog
    {
        public AddAlbumToPlaylist()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private async void NewPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            await MusicLibraryManagement.AddNewPlaylist(playlistName.Text);
        }
    }
}
