using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
namespace VLC_WinRT.UI.Legacy.Views.VideoPages.VideoNowPlayingControls
{
    public sealed partial class SubtitlesSettings : UserControl
    {
        public SubtitlesSettings()
        {
            this.InitializeComponent();
        }

        private void OnClose(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }
    }
}
