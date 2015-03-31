using Windows.UI.Xaml.Controls;
using VLC_WinRT.Helpers.MusicLibrary;
using VLC_WinRT.ViewModels.MusicVM;

namespace VLC_WinRT.Views.MusicPages
{
    public sealed partial class CreateNewPlaylist : ContentDialog
    {
        public CreateNewPlaylist()
        {
            this.InitializeComponent();
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            await MusicLibraryManagement.AddNewPlaylist(playlistName.Text);
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
