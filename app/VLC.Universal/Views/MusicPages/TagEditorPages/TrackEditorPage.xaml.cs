using System.Threading.Tasks;
using VLC.ViewModels;
using Windows.UI.Xaml.Controls;

namespace VLC.UI.Views.MusicPages.TagEditorPages
{
    public sealed partial class TrackEditorPage : Page
    {
        public TrackEditorPage()
        {
            this.InitializeComponent();
        }

        public void SaveChanges()
        {
            if (TrackNameTextBox.Text != Locator.MusicLibraryVM.CurrentTrack.Name)
            {
                if (!string.IsNullOrEmpty(TrackNameTextBox.Text))
                    Locator.MusicLibraryVM.CurrentTrack.Name = TrackNameTextBox.Text;

                Locator.MediaLibrary.Update(Locator.MusicLibraryVM.CurrentTrack);
            }

            Locator.NavigationService.GoBack_Specific();
        }

        private void SaveChanges_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SaveChanges();
        }
    }
}
