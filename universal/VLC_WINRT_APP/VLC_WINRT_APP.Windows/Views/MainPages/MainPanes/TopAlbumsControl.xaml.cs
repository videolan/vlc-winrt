
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Model.Video;

namespace VLC_WINRT_APP.Views.MainPages.MainPanes
{
    public sealed partial class TopAlbumsControl : UserControl
    {
        ItemsWrapGrid TopAlbumsItemsWrapGrid;
        public TopAlbumsControl()
        {
            this.InitializeComponent();
        }

        void TopAlbumsControl_Loaded(object sender, RoutedEventArgs e)
        {
            TopAlbumsItemsWrapGrid = (sender as ItemsWrapGrid);
            (sender as ItemsWrapGrid).Unloaded += TopAlbumsControl_Unloaded;
            Window.Current.SizeChanged += Current_SizeChanged;
            TemplateSizer.ComputeAlbums(TopAlbumsItemsWrapGrid, TemplateSize.Compact, Window.Current.Bounds.Width);
        }

        void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            if (TopAlbumsItemsWrapGrid == null) return;
            Responsive(TopAlbumsItemsWrapGrid);
            TemplateSizer.ComputeAlbums(TopAlbumsItemsWrapGrid, TemplateSize.Compact, e.Size.Width);
        }

        void TopAlbumsControl_Unloaded(object sender, RoutedEventArgs e)
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
