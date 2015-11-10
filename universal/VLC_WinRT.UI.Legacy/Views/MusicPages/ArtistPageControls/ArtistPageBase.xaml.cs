using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WinRT.Model.Video;

namespace VLC_WinRT.Views.MusicPages.ArtistPageControls
{
    public sealed partial class ArtistPageBase : Page
    {
        public ArtistPageBase()
        {
            this.InitializeComponent();
            ArtistPageBaseContent.Loaded += ArtistPageBaseContent_Loaded;
        }

        private void ArtistPageBaseContent_Loaded(object sender, RoutedEventArgs e)
        {
            ArtistPageBaseContent.Content = new ArtistAlbumsList();
        }
    }
}
