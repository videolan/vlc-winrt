using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VLC.UI.Views.UserControls.Flyouts
{
    public sealed partial class VideoInformationFlyout : Flyout
    {
        public VideoInformationFlyout() : base()
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

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var RootGrid = sender as Border;
            RootGrid.MaxWidth = 400;
            RootGrid.MaxHeight = 400;
        }
    }
}
