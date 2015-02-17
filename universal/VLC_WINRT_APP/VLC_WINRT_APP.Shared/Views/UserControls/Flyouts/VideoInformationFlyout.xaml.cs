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

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
              this.MaxHeight = 400;
              this.Margin = new Thickness(40,0,40,0);
#else
            this.MaxWidth = 400;
            this.MaxHeight = 400;
#endif
        }
    }
}
