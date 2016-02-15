using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VLC_WinRT.Model.Video
{
    public enum TemplateSize
    {
        Normal,
        Compact
    }

    public static class TemplateSizer
    {
        private const int tileSize = 180;
        private const double titlesGridHeight = 40;
        private const double titlesGridHeightAlbum = 50;

        public static void ComputeCompactVideo(ItemsWrapGrid wrapGrid, double? width = null)
        {
            if (width == null) width = Window.Current.Bounds.Width;
            const int splitScreen = 2;
            if (width > 600)
            {
                wrapGrid.ItemWidth = 307;
                wrapGrid.ItemHeight = tileSize + titlesGridHeight;
            }
            else
            {
                var itemWidth = width.Value / splitScreen;
                var itemHeight = itemWidth * 0.5625 + titlesGridHeight;
                wrapGrid.ItemHeight = itemHeight;
                wrapGrid.ItemWidth = itemWidth;
            }
        }

        public static void ComputeAlbums(ItemsWrapGrid wrapGrid, double? width = null, TemplateSize size = TemplateSize.Compact)
        {
            if (width == null) width = Window.Current.Bounds.Width;
            var splitScreen = (size == TemplateSize.Compact) ? 3 : 2;
            if (width < 400)
            {
                splitScreen = 2;
            }

            if (width > tileSize * splitScreen)
            {
                wrapGrid.ItemWidth = tileSize;
                wrapGrid.ItemHeight = tileSize + titlesGridHeightAlbum;
            }
            else
            {
                wrapGrid.ItemWidth = (width.Value / splitScreen);
                wrapGrid.ItemHeight = wrapGrid.ItemWidth + titlesGridHeightAlbum;
            }
        }

        public static void ComputeAlbumTracks(ref ItemsWrapGrid wrapGrid, double? width = null)
        {
            if (width == null) width = Window.Current.Bounds.Width;

            if (width > 700)
            {
                wrapGrid.ItemWidth = width.Value/2;
            }
            else wrapGrid.ItemWidth = width.Value;
        }
    }
}
