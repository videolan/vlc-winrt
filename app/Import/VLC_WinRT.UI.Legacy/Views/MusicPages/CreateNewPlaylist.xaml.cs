using Windows.UI.Xaml;
using VLC.ViewModels;

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
