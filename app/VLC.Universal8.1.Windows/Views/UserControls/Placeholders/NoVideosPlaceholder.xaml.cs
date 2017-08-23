using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC.Model.Video;
using VLC.ViewModels;

namespace VLC.Universal8._1.Views.UserControls.Placeholder
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
