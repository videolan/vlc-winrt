using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Views.MainPages.MusicPanes;

namespace VLC_WINRT_APP.Views.MainPages.MainMusicControls
{
    public sealed partial class AlbumsPivotItem : Page
    {
        public AlbumsPivotItem()
        {
            this.InitializeComponent();
            this.Loaded += AlbumsPivotItem_Loaded;
        }

        private void AlbumsPivotItem_Loaded(object sender, RoutedEventArgs e)
        {
            this.Content = new AlbumCollectionBase();
        }
    }
}
