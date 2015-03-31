using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WinRT.Model.Video;

namespace VLC_WinRT.Views.MainPages.MainVideoControls
{
    public sealed partial class AllVideosPivotItem : Page
    {
        public AllVideosPivotItem()
        {
            this.InitializeComponent();
        }

        private void VideosWrapGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TemplateSizer.ComputeCompactVideo(sender as ItemsWrapGrid, this.ActualWidth);
        }
    }
}
