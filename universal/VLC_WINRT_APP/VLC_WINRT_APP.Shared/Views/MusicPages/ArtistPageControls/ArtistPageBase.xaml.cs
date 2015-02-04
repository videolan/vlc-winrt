using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Model.Video;

namespace VLC_WINRT_APP.Views.MusicPages.ArtistPageControls
{
    public sealed partial class ArtistPageBase : UserControl
    {
        public ArtistPageBase()
        {
            this.InitializeComponent();
        }
        private void ItemsWrapGrid_Loaded(object sender, RoutedEventArgs e)
        {
            TemplateSizer.ComputeAlbums(sender as ItemsWrapGrid, TemplateSize.Normal, this.ActualWidth - 24);
        }
    }
}
