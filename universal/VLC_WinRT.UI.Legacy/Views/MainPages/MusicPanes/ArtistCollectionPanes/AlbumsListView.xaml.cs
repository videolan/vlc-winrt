using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WinRT.Model.Music;
using VLC_WinRT.Model.Video;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Views.MainPages.MusicPanes.ArtistCollectionPanes
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
            TemplateSizer.ComputeAlbums(sender as ItemsWrapGrid, this.ActualWidth, Window.Current.Bounds.Width > 900 ? TemplateSize.Compact : TemplateSize.Normal);
#else
            TemplateSizer.ComputeAlbums(sender as ItemsWrapGrid, this.ActualWidth);
#endif
        }


        private async void AlbumsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!e.AddedItems.Any()) return;
            var album = e.AddedItems[0] as AlbumItem;
            await App.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
            {
                if (Window.Current.Bounds.Width > 1220)
                    Locator.MusicLibraryVM.CurrentAlbum = album;
                else
                    Locator.MusicLibraryVM.AlbumClickedCommand.Execute(album);
            });
        }
    }
}
