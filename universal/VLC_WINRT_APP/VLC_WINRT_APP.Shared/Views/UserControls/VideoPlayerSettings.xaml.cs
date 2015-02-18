using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace VLC_WINRT_APP.Views.UserControls
{
    public sealed partial class VideoPlayerSettings : UserControl
    {
        public VideoPlayerSettings()
        {
            this.InitializeComponent();
        }

        private void RootGrid_OnLoaded(object sender, RoutedEventArgs e)
        {
#if WINDOWS_APP
            RootGrid.Background = new SolidColorBrush(Color.FromArgb(255,25,25,25));
            RootScrollViewer.Margin = new Thickness(12);
#endif
        }
    }
}
