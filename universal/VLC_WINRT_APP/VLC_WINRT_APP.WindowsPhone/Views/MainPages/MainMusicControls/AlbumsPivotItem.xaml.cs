using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Model.Video;

namespace VLC_WINRT_APP.Views.MainPages.MainMusicControls
{
    public sealed partial class AlbumsPivotItem : Page
    {
        public AlbumsPivotItem()
        {
            this.InitializeComponent();
        }
        
        private void AlbumsWrapGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TemplateSizer.ComputeAlbums(sender as ItemsWrapGrid);
        }
    }
}
