using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace VLC_WinRT.Views.UserControls.Flyouts
{
    public sealed partial class VideoInformationFlyout
    {
        public VideoInformationFlyout()
        {
            this.InitializeComponent();
#if WINDOWS_PHONE_APP
            this.Placement = FlyoutPlacementMode.Full;
#endif
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        void Hide()
        {
            var trackFlyout = App.Current.Resources["VideoInformationFlyout"] as Flyout;
            trackFlyout.Hide();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var RootGrid = sender as Border;
            RootGrid.MaxWidth = 400;
            RootGrid.MaxHeight = 400;
#if WINDOWS_PHONE_APP
            RootGrid.Margin = new Thickness(40, 0, 40, 0);
#endif
        }
    }
}
