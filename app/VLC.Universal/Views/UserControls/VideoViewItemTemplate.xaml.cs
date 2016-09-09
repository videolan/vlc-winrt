using VLC.Model.Video;
using VLC.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace VLC.UI.Views.UserControls
{
    public sealed partial class VideoViewItemTemplate : UserControl
    {
        private Brush savedBrush = null;

        public VideoViewItemTemplate()
        {
            this.InitializeComponent();
            this.DataContextChanged += VideoViewItemTemplate_DataContextChanged;
            Locator.VideoLibraryVM.VideoViewSet += VideoViewSet;
            savedBrush = Title.Foreground;
        }

        ~VideoViewItemTemplate()
        {
            Locator.VideoLibraryVM.VideoViewSet -= VideoViewSet;
        }

        private void VideoViewItemTemplate_DataContextChanged(Windows.UI.Xaml.FrameworkElement sender, Windows.UI.Xaml.DataContextChangedEventArgs args)
        {
            if (this.DataContext != null)
                VideoViewSet(Locator.VideoLibraryVM.VideoView);
        }

        void VideoViewSet(VideoView v)
        {
            if (v == (VideoView)this.DataContext)
                Title.Foreground = (Brush)App.Current.Resources["MainColor"];
            else
                if (savedBrush != null)
                    Title.Foreground = savedBrush;
        }
    }
}
