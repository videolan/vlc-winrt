using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC.Helpers.MusicLibrary;
using VLC.ViewModels;
using System.Threading.Tasks;

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
            await addToCollectionAndClose();
        }

        private async void playlistName_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter
                && e.KeyStatus.RepeatCount == 1)
            {
                await addToCollectionAndClose();
                e.Handled = true;
            }
        }

        private async Task addToCollectionAndClose()
        {
            await Locator.MediaLibrary.AddNewPlaylist(playlistName.Text);
            Locator.NavigationService.GoBack_HideFlyout();
        }
    }
}
