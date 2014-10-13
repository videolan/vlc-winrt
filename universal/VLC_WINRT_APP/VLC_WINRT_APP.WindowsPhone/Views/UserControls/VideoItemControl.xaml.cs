using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VLC_WINRT_APP.Views.UserControls
{
    public sealed partial class VideoItemControl : UserControl
    {
        public VideoItemControl()
        {
            this.InitializeComponent();
        }
        private void ImageVideo_Loaded(object sender, RoutedEventArgs e)
        {
            FadeInVideoImg.Begin();
        }
    }
}
