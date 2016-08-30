using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace VLC.Views.UserControls.Flyouts
{
    public sealed partial class VideoInformationFlyout
    {
        public VideoInformationFlyout()
        {
            this.InitializeComponent();
        }

        public VideoInformationFlyout(object videoItem)
        {
            this.InitializeComponent();
            this.FlyoutGrid.DataContext = videoItem;
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
        }
    }
}
