using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Views.MusicPages;

namespace VLC_WINRT_APP.Commands.Music
{
    public class AlbumClickedCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            App.Transition.Edge = EdgeTransitionLocation.Right;
#if WINDOWS_PHONE_APP
            App.ApplicationFrame.Navigate(typeof (AlbumPage));
            Locator.MusicLibraryVM.IsAlbumPageShown = true;
            AlbumItem album = parameter as AlbumItem;

            if (album == null)
            {
                ItemClickEventArgs args = parameter as ItemClickEventArgs;
                album = args.ClickedItem as AlbumItem;
            }
            Locator.MusicLibraryVM.CurrentArtist =
                Locator.MusicLibraryVM.Artists.FirstOrDefault(x => x.Id == album.ArtistId);
            Locator.MusicLibraryVM.CurrentAlbum = album;
#endif
        }
    }
}
