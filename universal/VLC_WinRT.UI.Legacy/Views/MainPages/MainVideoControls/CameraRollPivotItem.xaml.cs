using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WinRT.Model.Video;

namespace VLC_WinRT.Views.MainPages.MainVideoControls
{
    public sealed partial class CameraRollPivotItem : Page
    {
        public CameraRollPivotItem()
        {
            this.InitializeComponent();
            this.Loaded += CameraRollPivotItem_Loaded;
        }

        private void CameraRollPivotItem_Loaded(object sender, RoutedEventArgs e)
        {
            Locator.VideoLibraryVM.OnNavigatedToCameraRollVideos();
        }

        private void VideosWrapGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TemplateSizer.ComputeCompactVideo(sender as ItemsWrapGrid, CameraRollListView.ActualWidth);
        }
    }
}
