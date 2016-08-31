using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC.Model.Video;
using VLC.ViewModels;
using Windows.UI.Xaml.Input;
using VLC.Model;
using Windows.System;
using VLC.Views.UserControls.Flyouts;

namespace VLC.Views.MainPages.MusicPanes
{
    public sealed partial class AlbumCollectionBase : UserControl
    {
        ListViewItem focussedListViewItem;

        public AlbumCollectionBase()
        {
            this.InitializeComponent();
            this.SizeChanged += AlbumCollectionBase_SizeChanged;
        }

        private void AlbumCollectionBase_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (AlbumsZoomedInView.ItemsPanelRoot == null) return;
            TemplateSizer.ComputeAlbums(AlbumsZoomedInView.ItemsPanelRoot as ItemsWrapGrid, AlbumsZoomedInView.ItemsPanelRoot.ActualWidth - 6);
        }
        
        private void AlbumsZoomedInView_GotFocus(object sender, RoutedEventArgs e)
        {
            ListView list = (ListView)sender;
            if (FocusManager.GetFocusedElement() as ListViewItem != null)
            {
                focussedListViewItem = (ListViewItem)FocusManager.GetFocusedElement();
            }
        }

        private void AlbumsZoomedInView_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            ListView list = (ListView)sender;
            if (Locator.NavigationService.CurrentPage == VLCPage.MainPageMusic
                && e.Key == VirtualKey.GamepadView)
            {
                var menu = new AlbumMenuFlyout(AlbumsZoomedInView.ItemFromContainer(focussedListViewItem));
                menu.ShowAt(focussedListViewItem);
            }
        }
    }
}
