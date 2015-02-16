using System.Linq;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using VLC_WINRT_APP.ViewModels;

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
        }

        private void RootAlbumItem_Holding(object sender, HoldingRoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            Flyout.ShowAttachedFlyout((Grid)sender);
#endif
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
#if WINDOWS_APP
            Flyout.ShowAttachedFlyout((Grid)sender);
#endif
        }
    }
}
