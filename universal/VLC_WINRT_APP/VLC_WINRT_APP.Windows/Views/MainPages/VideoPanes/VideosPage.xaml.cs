using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VLC_WINRT_APP.Views.MainPages.VideoPanes
{
    public sealed partial class VideosPage : Page
    {
        public VideosPage()
        {
            this.InitializeComponent();
        }

        private void ItemsWrapGrid_Loaded(object sender, RoutedEventArgs e)
        {
            Responsive(sender as ItemsWrapGrid);
        }

        private void ItemsWrapGrid_Loaded(object sender, SizeChangedEventArgs e)
        {
            Responsive(sender as ItemsWrapGrid);
        }

        void Responsive(ItemsWrapGrid itemsWrap)
        {
            int usefulWidth = (int)Window.Current.Bounds.Width;
            Debug.WriteLine(usefulWidth);
            int sidebar;
            if (Window.Current.Bounds.Width < 400)
            {
                sidebar = 30;
            }
            else if (Window.Current.Bounds.Width < 1080)
            {
                sidebar = 140;
            }
            else
            {
                sidebar = 400;
            }
            usefulWidth -= sidebar;

            if (usefulWidth < 400)
            {
                itemsWrap.ItemWidth = usefulWidth;
                itemsWrap.ItemHeight = usefulWidth * 0.561;
            }
            else if (usefulWidth < 890)
            {
                itemsWrap.ItemWidth = usefulWidth / 2;
                itemsWrap.ItemHeight = (usefulWidth / 2) * 0.561;
            }
            else if (usefulWidth < 1300)
            {
                itemsWrap.ItemWidth = usefulWidth / 3;
                itemsWrap.ItemHeight = (usefulWidth / 3) * 0.561;
            }
            else
            {
                itemsWrap.ItemWidth = usefulWidth / 4;
                itemsWrap.ItemHeight = (usefulWidth / 4) * 0.561;
            }
        }
    }
}
