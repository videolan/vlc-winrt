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
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.Unloaded += AllVideosPivotItem_Unloaded;
            Locator.VideoLibraryVM.OnNavigatedToAllVideos();
        }

        private async void AllVideosPivotItem_Unloaded(object sender, RoutedEventArgs e)
        {
            await Locator.VideoLibraryVM.OnNavigatedFromAllVideos();
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
            if (FocusManager.GetFocusedElement() as ListViewItem != null)
            {
                focussedListViewItem = (ListViewItem)FocusManager.GetFocusedElement();
            }
        }
    }
}
