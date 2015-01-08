using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
namespace VLC_WINRT_APP.Views.UserControls.Flyouts
{
    public sealed partial class VideoInformationFlyout : UserControl
    {
        public VideoInformationFlyout()
        {
            this.InitializeComponent();
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
    }
}
