using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.Model.Video;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Views.MainPages.MusicPanes.ArtistCollectionPanes
{
    public sealed partial class AlbumsListView : Grid
    {
        public AlbumsListView()
        {
            this.InitializeComponent();
        }

        private void ItemsWrapGrid_Loaded(object sender, RoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            TemplateSizer.ComputeAlbums(sender as ItemsWrapGrid, Window.Current.Bounds.Width > 900 ? TemplateSize.Compact : TemplateSize.Normal, this.ActualWidth);
#else
            TemplateSizer.ComputeAlbums(sender as ItemsWrapGrid, TemplateSize.Compact, this.ActualWidth);
#endif
        }

        private void AlbumsList_OnItemClick(object sender, ItemClickEventArgs e)
        {
            var album = e.ClickedItem as AlbumItem;
            if (Window.Current.Bounds.Width > 900)
                Locator.MusicLibraryVM.CurrentAlbum = album;
            else Locator.MusicLibraryVM.AlbumClickedCommand.Execute(album);
        }
    }
}
