using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Helpers.MusicLibrary;

namespace VLC_WINRT_APP.Views.MusicPages
{
    public sealed partial class CreateNewPlaylist : SettingsFlyout
    {
        public CreateNewPlaylist()
        {
            this.InitializeComponent();
        }

        private async void AddToCollection_Click(object sender, RoutedEventArgs e)
        {
            await MusicLibraryManagement.AddNewPlaylist(playlistName.Text);
            this.Hide();
        }
    }
}
