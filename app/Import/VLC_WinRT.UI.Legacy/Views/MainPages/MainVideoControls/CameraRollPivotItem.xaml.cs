using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC.Model.Video;
using VLC.ViewModels;

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
            this.Unloaded += CameraRollPivotItem_Unloaded;
        }

        private async void CameraRollPivotItem_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private void VideosWrapGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TemplateSizer.ComputeCompactVideo(sender as ItemsWrapGrid, CameraRollListView.ActualWidth);
        }
    }
}
