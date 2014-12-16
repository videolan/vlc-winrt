using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Model.Video;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Views.UserControls.Placeholder
{
    public sealed partial class NoVideosPlaceholder : UserControl
    {
        public NoVideosPlaceholder()
        {
            this.InitializeComponent();
        }

        private void ShowsClick(object sender, RoutedEventArgs e)
        {
            Locator.SettingsVM.VideoView = VideoView.Shows;
        }

        private void CameraRollClick(object sender, RoutedEventArgs e)
        {
            Locator.SettingsVM.VideoView = VideoView.CameraRoll;
        }
    }
}
