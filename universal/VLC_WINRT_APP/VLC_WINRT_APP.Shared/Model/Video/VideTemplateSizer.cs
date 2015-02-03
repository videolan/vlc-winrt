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
        public static void ComputeCompactVideo(ItemsWrapGrid wrapGrid)
        {
            var width = Window.Current.Bounds.Width;
            var splitScreen = 2;
            if (!DisplayHelper.IsPortrait())
                splitScreen = 4;
#if WINDOWS_APP
            width = (width > 600) ? width : 470;
            if (width == 470) splitScreen = 2;
#endif
            var itemWidth = (width / splitScreen);
            var itemHeight = (itemWidth * 9 / 16) + 78;
            wrapGrid.ItemWidth = itemWidth - 10;
            wrapGrid.ItemHeight = itemHeight;
        }

        public static void ComputeAlbums(ItemsWrapGrid wrapGrid, TemplateSize size = TemplateSize.Compact, double? width = null)
        {
            if (width == null) width = Window.Current.Bounds.Width;
            var splitScreen = (size == TemplateSize.Compact) ? 3 : 2;
#if WINDOWS_PHONE_APP
            if(width == 400)
                size = TemplateSize.Normal;
            if (!DisplayHelper.IsPortrait())
                splitScreen = 5;
            var itemWidth = (width / splitScreen);
            var itemHeight = itemWidth*1.33;
            wrapGrid.ItemWidth = itemWidth -7;
            wrapGrid.ItemHeight = itemHeight;
#else
            if (width > 600)
            {
                wrapGrid.ItemWidth = 200;
                wrapGrid.ItemHeight = 266;
            }
            else
            {
                wrapGrid.ItemWidth = (width.Value / splitScreen) - 20;
                wrapGrid.ItemHeight = wrapGrid.ItemWidth * 1.33;
            }
#endif
        }
    }
}
