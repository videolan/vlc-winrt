using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WinRT.Model.Video;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Views.UserControls.Placeholder
{
    public sealed partial class NoVideosPlaceholder : UserControl
    {
        public NoVideosPlaceholder()
        {
            this.InitializeComponent();
        }

        private void ShowsClick(object sender, RoutedEventArgs e)
        {
            Locator.VideoLibraryVM.VideoView = VideoView.Shows;
        }

        private void CameraRollClick(object sender, RoutedEventArgs e)
        {
            Locator.VideoLibraryVM.VideoView = VideoView.CameraRoll;
        }
    }
}
