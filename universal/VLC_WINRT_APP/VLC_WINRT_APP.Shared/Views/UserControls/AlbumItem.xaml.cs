using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VLC_WINRT_APP.Views.UserControls
{
    public sealed partial class AlbumItem : UserControl
    {
        public AlbumItem()
        {
            this.InitializeComponent();
        }

        private void RootAlbumItem_Loaded(object sender, RoutedEventArgs e)
        {
#if WINDOWS_APP
            var grid = sender as Grid;
            if (grid != null)
                grid.Tapped += (o, args) => Flyout.ShowAttachedFlyout((Grid) sender);
#endif
        }
    }
}
