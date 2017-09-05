using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using VLC.ViewModels;

namespace VLC_WinRT.UI.Legacy.Views.MusicPages.TagEditorPages
{
    public sealed partial class TrackEditorPage : Page
    {
        public TrackEditorPage()
        {
            this.InitializeComponent();
        }

        public async Task SaveChanges()
        {
            if (TrackNameTextBox.Text != Locator.MusicLibraryVM.CurrentTrack.Name)
            {
                if (!string.IsNullOrEmpty(TrackNameTextBox.Text))
                    Locator.MusicLibraryVM.CurrentTrack.Name = TrackNameTextBox.Text;

                Locator.MediaLibrary.Update(Locator.MusicLibraryVM.CurrentTrack);
            }

            Locator.NavigationService.GoBack_Specific();
        }

        private async void SaveChanges_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await SaveChanges();
        }
    }
}
