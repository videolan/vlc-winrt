using VLC_WinRT.Model.Video;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VLC_WinRT.Views.MusicPages.ArtistPageControls
{
    public sealed partial class ArtistAlbumsList : UserControl
    {
        public ArtistAlbumsList()
        {
            this.InitializeComponent();
        }
        private void ItemsWrapGrid_Loaded(object sender, RoutedEventArgs e)
        {
            TemplateSizer.ComputeAlbums(sender as ItemsWrapGrid, this.ActualWidth - 24, TemplateSize.Normal);
        }
    }
}
