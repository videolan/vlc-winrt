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
            var itemWidth = (width / splitScreen);
            var itemHeight = (itemWidth * 9/16) + 78;
            //var itemHeight = (itemWidth);
            wrapGrid.ItemWidth = itemWidth - 10;
            wrapGrid.ItemHeight = itemHeight;
        }

        public static void ComputeAlbums(ItemsWrapGrid wrapGrid, TemplateSize size = TemplateSize.Compact)
        {
#if WINDOWS_PHONE_APP
            var width = Window.Current.Bounds.Width;
            if(width == 400)
                size = TemplateSize.Normal;
            var splitScreen = (size == TemplateSize.Compact) ? 3 : 2;
            if (!DisplayHelper.IsPortrait())
                splitScreen = 5;
            var itemWidth = (width / splitScreen);
            var itemHeight = itemWidth*1.33;
            wrapGrid.ItemWidth = itemWidth -7;
            wrapGrid.ItemHeight = itemHeight;
#else
            wrapGrid.ItemWidth = 200;
            wrapGrid.ItemHeight = 266;
#endif
        }
    }
}
