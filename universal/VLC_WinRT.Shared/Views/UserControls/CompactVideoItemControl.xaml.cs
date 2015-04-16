using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace VLC_WinRT.Views.UserControls
{
    public sealed partial class CompactVideoItemControl : UserControl
    {
        public CompactVideoItemControl()
        {
            this.InitializeComponent();
        }

        private void RootAlbumItem_Holding(object sender, HoldingRoutedEventArgs e)
        {
            Flyout.ShowAttachedFlyout((Border)sender);
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            Flyout.ShowAttachedFlyout((Border)sender);
        }
    }
}
