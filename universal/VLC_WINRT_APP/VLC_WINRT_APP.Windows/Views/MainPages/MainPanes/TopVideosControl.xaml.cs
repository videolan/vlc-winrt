using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Model.Video;

namespace VLC_WINRT_APP.Views.MainPages.MainPanes
{
    public sealed partial class TopVideosControl : UserControl
    {
        ItemsWrapGrid VideosItemsWrapGrid;
        public TopVideosControl()
        {
            this.InitializeComponent();
        }

        void TopVideosControl_Loaded(object sender, RoutedEventArgs e)
        {
            VideosItemsWrapGrid = (sender as ItemsWrapGrid);
            (sender as ItemsWrapGrid).Unloaded += TopVideosControl_Unloaded;
            Window.Current.SizeChanged += Current_SizeChanged;
            TemplateSizer.ComputeCompactVideo(VideosItemsWrapGrid, Window.Current.Bounds.Width);
        }

        void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            if (VideosItemsWrapGrid == null) return;
            Responsive(VideosItemsWrapGrid);
            TemplateSizer.ComputeCompactVideo(VideosItemsWrapGrid, e.Size.Width);
        }

        void TopVideosControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Current_SizeChanged;
        }

        private void Responsive(ItemsWrapGrid itemsWrap)
        {
            if (Window.Current.Bounds.Width < 650)
            {
                itemsWrap.Orientation = Orientation.Horizontal;
            }
            else
            {
                itemsWrap.Orientation = Orientation.Vertical;
            }
        }
    }
}
