using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Popups;
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
#if WINDOWS_PHONE_APP
            if (App.ApplicationFrame.CurrentSourcePageType != typeof (AlbumPage))
            {
                App.ApplicationFrame.Navigate(typeof (AlbumPage));
            }
            Locator.MusicLibraryVM.IsAlbumPageShown = true;
            AlbumItem album = null;
            if (parameter is AlbumItem)
            {
                album = parameter as AlbumItem;
            }
            else if (parameter is ItemClickEventArgs)
            {
                var args = parameter as ItemClickEventArgs;
                album = args.ClickedItem as AlbumItem;
            }
            // searching artist from his id
            else if (parameter is int)
            {
                var id = (int)parameter;
                album = Locator.MusicLibraryVM.Albums.FirstOrDefault(x => x.Id == id);
            }

            Locator.MusicLibraryVM.CurrentArtist =
                Locator.MusicLibraryVM.Artists.FirstOrDefault(x => x.Id == album.ArtistId);
            Locator.MusicLibraryVM.CurrentAlbum = album;
#endif
        }
    }
}
