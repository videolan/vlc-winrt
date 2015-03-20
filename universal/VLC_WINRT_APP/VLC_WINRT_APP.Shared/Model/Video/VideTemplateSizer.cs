using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Helpers;

namespace VLC_WINRT_APP.Model.Video
{
    public enum TemplateSize
    {
        Normal,
        Compact
    }
    public static class TemplateSizer
    {
        private const int tileSize = 173;
        const double contentGrid = 26;
        private const double contentGridWindows = 40;
        public static void ComputeCompactVideo(ItemsWrapGrid wrapGrid, double? width = null)
        {
            if (width == null) width = Window.Current.Bounds.Width;
            var splitScreen = 2;
#if WINDOWS_PHONE_APP
            if (!DisplayHelper.IsPortrait())
                splitScreen = 4;
#endif
#if WINDOWS_APP
            if (width > 614)
            {
                wrapGrid.ItemHeight = tileSize + contentGridWindows;
                wrapGrid.ItemWidth = 307;
            }
            else
            {
                var itemWidth = width.Value / splitScreen;
                var itemHeight = itemWidth * 0.5625 + contentGridWindows;
                wrapGrid.ItemHeight = itemHeight;
                wrapGrid.ItemWidth = itemWidth;
            }
#else
            var itemWidth = (width.Value / splitScreen);
            var itemHeight = (itemWidth * 9 / 16) + contentGrid;
            wrapGrid.ItemWidth = itemWidth - 10;
            wrapGrid.ItemHeight = itemHeight;
#endif
        }

        public static void ComputeAlbums(ItemsWrapGrid wrapGrid, TemplateSize size = TemplateSize.Compact, double? width = null)
        {
            if (width == null) width = Window.Current.Bounds.Width;
            var splitScreen = (size == TemplateSize.Compact) ? 3 : 2;
#if WINDOWS_PHONE_APP
            width -= 5;
            if (width < 401 && width > 399)
                size = TemplateSize.Normal;
            if (!DisplayHelper.IsPortrait())
                splitScreen = 5;
            var itemWidth = (width.Value / splitScreen);
            var itemHeight = itemWidth + 40;
            wrapGrid.ItemWidth = itemWidth;
            wrapGrid.ItemHeight = itemHeight;
#else
            if (width > tileSize*3)
            {
                wrapGrid.ItemWidth = tileSize;
                wrapGrid.ItemHeight = tileSize + contentGridWindows;
            }
            else
            {
                wrapGrid.ItemWidth = (width.Value / splitScreen);
                wrapGrid.ItemHeight = wrapGrid.ItemWidth + contentGridWindows;
            }
#endif
        }
    }
}
