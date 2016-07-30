using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WinRT.Helpers.MusicLibrary;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Views.MusicPages
{
    public sealed partial class CreateNewPlaylist
    {
        public CreateNewPlaylist()
        {
            this.InitializeComponent();
        }

        private async void AddToCollection_Click(object sender, RoutedEventArgs e)
        {
            await Locator.MediaLibrary.AddNewPlaylist(playlistName.Text);
            Locator.NavigationService.GoBack_HideFlyout();
        }
    }
}
