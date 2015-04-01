using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WinRT.Helpers.MusicLibrary;

namespace VLC_WinRT.Views.MusicPages
{
    public sealed partial class CreateNewPlaylist : UserControl
    {
        public CreateNewPlaylist()
        {
            this.InitializeComponent();
        }

        private async void AddToCollection_Click(object sender, RoutedEventArgs e)
        {
            await MusicLibraryManagement.AddNewPlaylist(playlistName.Text);
        }
    }
}
