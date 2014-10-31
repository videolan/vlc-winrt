using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VLC_WINRT_APP.Model.Video
{
    public static class VideoTemplateSizer
    {
#if WINDOWS_PHONE_APP
        public static void ComputeCompactVideo(ItemsWrapGrid wrapGrid)
        {
            var width = Window.Current.Bounds.Width;
            var itemWidth = (width / 2) - 12;
            wrapGrid.ItemWidth = itemWidth;
            wrapGrid.ItemHeight = itemWidth * 1.33;
        }

        public static void ComputerAlbums(ItemsWrapGrid wrapGrid)
        {
            
        }

#endif
    }
}
