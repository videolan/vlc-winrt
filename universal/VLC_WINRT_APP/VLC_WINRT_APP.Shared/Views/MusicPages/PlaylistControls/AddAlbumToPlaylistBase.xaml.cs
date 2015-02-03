using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Helpers.MusicLibrary;

namespace VLC_WINRT_APP.Views.MusicPages.PlaylistControls
{
    public sealed partial class AddAlbumToPlaylistBase : UserControl
    {
        public AddAlbumToPlaylistBase()
        {
            this.InitializeComponent();
        }
        private async void NewPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            await MusicLibraryManagement.AddNewPlaylist(playlistName.Text);
        }

        private void AddToPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            MusicLibraryManagement.AddAlbumToPlaylist(null);
        }
    }
}
