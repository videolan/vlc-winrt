using System.Linq;
using Windows.UI.Xaml.Controls;
using VLC_WINRT.Common;
using VLC_WinRT.Model.Music;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Model;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Commands.Music
{
    public class AlbumClickedCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
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
            
            Locator.NavigationService.Go(VLCPage.AlbumPage);
        }
    }
}
