using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Model.Video;

namespace VLC_WINRT_APP.Views.MainPages.MainVideoControls
{
    public sealed partial class AllVideosPivotItem : UserControl
    {
        public AllVideosPivotItem()
        {
            this.InitializeComponent();
        }
        private void ItemsWrapGrid_Loaded(object sender, RoutedEventArgs e)
        {
            TemplateSizer.ComputeCompactVideo(sender as ItemsWrapGrid);
        }
    }
}
