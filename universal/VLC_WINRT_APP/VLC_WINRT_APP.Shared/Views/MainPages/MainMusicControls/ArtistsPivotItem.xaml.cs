using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Model.Video;

namespace VLC_WINRT_APP.Views.MainPages.MainMusicControls
{
    public sealed partial class ArtistsPivotItem : Page
    {
        public ArtistsPivotItem()
        {
            this.InitializeComponent();
        }
        private void Collection_Loaded(object sender, RoutedEventArgs e)
        {
#if WINDOWS_APP
            ArtistCollectionBase.Margin = new Thickness(24, 0, 0, 0);
#endif
        }
    }
}
