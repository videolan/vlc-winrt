using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Helpers.MusicLibrary;

namespace VLC_WINRT_APP.Views.MusicPages
{
    public sealed partial class AddAlbumToPlaylist : SettingsFlyout
    {
        public AddAlbumToPlaylist()
        {
            this.InitializeComponent();
        }
        private void AddToPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            MusicLibraryManagement.AddAlbumToPlaylist(null);
        }
    }
}
