using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WinRT.Helpers.MusicLibrary;

namespace VLC_WinRT.Views.MusicPages
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
