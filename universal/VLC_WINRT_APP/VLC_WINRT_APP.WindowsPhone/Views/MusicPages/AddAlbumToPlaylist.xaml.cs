using Windows.UI.Xaml.Controls;
using VLC_WinRT.Helpers.MusicLibrary;

namespace VLC_WinRT.Views.MusicPages
{
    public sealed partial class AddAlbumToPlaylist : ContentDialog
    {
        public AddAlbumToPlaylist()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            MusicLibraryManagement.AddAlbumToPlaylist(args);
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
