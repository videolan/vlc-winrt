using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC.Helpers.MusicLibrary;
using VLC.ViewModels;

namespace VLC.UI.Views.MusicPages.PlaylistControls
{
    public sealed partial class AddAlbumToPlaylistBase
    {
        public AddAlbumToPlaylistBase()
        {
            this.InitializeComponent();
            this.Loaded += AddAlbumToPlaylistBase_Loaded;
        }

        private void AddAlbumToPlaylistBase_Loaded(object sender, RoutedEventArgs e)
        {
            Locator.MusicLibraryVM.InitializePlaylists();
        }

        private async void NewPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            await Locator.MediaLibrary.AddNewPlaylist(playlistName.Text);
        }


        private void AddToPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            if (Locator.MediaLibrary.AddAlbumToPlaylist(null))
                Locator.NavigationService.GoBack_Specific();
        }
    }
}
