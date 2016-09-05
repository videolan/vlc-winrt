using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC.Helpers.MusicLibrary;
using VLC.ViewModels;

namespace VLC.UI.Views.MusicPages
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
