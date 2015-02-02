using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Views.UserControls
{
    public sealed partial class AlbumItem : UserControl
    {
        public AlbumItem()
        {
            this.InitializeComponent();
        }

        private void RootAlbumItem_Loaded(object sender, RoutedEventArgs e)
        {
#if WINDOWS_APP
            var grid = sender as Grid;
            if (grid != null)
            {
                grid.Tapped += (o, args) =>
                {
                    var album = (this.DataContext as Model.Music.AlbumItem);
                    Locator.MusicLibraryVM.CurrentArtist = Locator.MusicLibraryVM.Artists.FirstOrDefault(x => x.Id == album.ArtistId);
                    Locator.MusicLibraryVM.CurrentAlbum = album;
                    Flyout.ShowAttachedFlyout((Grid)sender);
                };
            }
#endif
        }

        private void RootAlbumItem_Holding(object sender, HoldingRoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            Flyout.ShowAttachedFlyout((Grid)sender);
#endif
        }
    }
}
