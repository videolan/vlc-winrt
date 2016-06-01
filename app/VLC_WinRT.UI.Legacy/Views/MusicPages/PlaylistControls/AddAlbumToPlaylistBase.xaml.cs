using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WinRT.Helpers.MusicLibrary;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Views.MusicPages.PlaylistControls
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
            Locator.MusicLibraryVM.OnNavigatedToPlaylists();
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
