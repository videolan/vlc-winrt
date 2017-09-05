using VLC.Model;
using VLC.Model.Video;
using VLC.UI.Views.UserControls.Flyouts;
using VLC.ViewModels;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace VLC.UI.Views.VideoPages.TVShowsViews
{
    public sealed partial class ShowEpisodesView : Page
    {
        private ListViewItem focussedListViewItem;

        public ShowEpisodesView()
        {
            this.InitializeComponent();
        }

        private void VideosWrapGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TemplateSizer.ComputeCompactVideo(sender as ItemsWrapGrid, this.ActualWidth);
        }

        private void EpisodesListView_GotFocus(object sender, RoutedEventArgs e)
        {
            ListView list = (ListView)sender;
            if (FocusManager.GetFocusedElement() as ListViewItem != null)
            {
                focussedListViewItem = (ListViewItem)FocusManager.GetFocusedElement();
            }
        }

        private void EpisodesListView_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            ListView list = (ListView)sender;
            if (Locator.NavigationService.CurrentPage == VLCPage.TvShowView
                && e.Key == VirtualKey.GamepadView)
            {
                var menu = new VideoInformationFlyout(EpisodesListView.ItemFromContainer(focussedListViewItem));
                menu.ShowAt(focussedListViewItem);
            }
        }
    }
}
