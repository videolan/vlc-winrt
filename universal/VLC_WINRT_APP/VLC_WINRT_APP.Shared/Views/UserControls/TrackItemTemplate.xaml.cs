using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace VLC_WINRT_APP.Views.UserControls
{
    public sealed partial class TrackItemTemplate : UserControl
    {
        public TrackItemTemplate()
        {
            this.InitializeComponent();
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
#if WINDOWS_APP
            Flyout.ShowAttachedFlyout((Grid)sender);
#endif
        }
    }
}
