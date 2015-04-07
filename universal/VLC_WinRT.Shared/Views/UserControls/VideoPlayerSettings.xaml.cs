using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VLC_WinRT.Views.UserControls
{
    public sealed partial class VideoPlayerSettings : UserControl
    {
        public VideoPlayerSettings()
        {
            this.InitializeComponent();
        }

        private void RootGrid_OnLoaded(object sender, RoutedEventArgs e)
        {
            RootScrollViewer.Margin = new Thickness(12);
        }
    }
}
