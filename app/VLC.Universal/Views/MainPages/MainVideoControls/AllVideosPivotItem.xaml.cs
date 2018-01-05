using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC.Model.Video;
using VLC.ViewModels;
using Windows.UI.Xaml.Input;
using Windows.System;
using VLC.UI.Views.UserControls.Flyouts;
using VLC.Model;

namespace VLC.UI.Views.MainPages.MainVideoControls
{
    public sealed partial class AllVideosPivotItem : Page
    {
        private ListViewItem focussedListViewItem;

        public AllVideosPivotItem()
        {
            this.InitializeComponent();
        }

        private void VideosWrapGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TemplateSizer.ComputeCompactVideo(sender as ItemsWrapGrid, this.ActualWidth);
        }

        private void AllVideosListView_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            ListView list = (ListView)sender;
            if (Locator.NavigationService.CurrentPage == VLCPage.MainPageVideo
                && e.Key == VirtualKey.GamepadView)
            {
                var menu = new VideoInformationFlyout(AllVideosListView.ItemFromContainer(focussedListViewItem));
                menu.ShowAt(focussedListViewItem);
            }
        }

        private void AllVideosListView_GotFocus(object sender, RoutedEventArgs e)
        {
            ListView list = (ListView)sender;
            if (FocusManager.GetFocusedElement() is ListViewItem)
            {
                focussedListViewItem = (ListViewItem)FocusManager.GetFocusedElement();
                var videoItem = focussedListViewItem?.ContentTemplateRoot as UserControls.VideoItem;
                videoItem?.StartAutoScroll();
            }
        }

        void AllVideosListView_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var videoItem = focussedListViewItem?.ContentTemplateRoot as UserControls.VideoItem;
            videoItem?.StopAutoScroll();
        }
    }
}
