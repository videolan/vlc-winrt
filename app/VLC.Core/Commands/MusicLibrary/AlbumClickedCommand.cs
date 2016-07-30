using System.Linq;
using Windows.UI.Xaml.Controls;
using VLC.Model.Music;
using VLC.ViewModels;
using VLC.Model;
using VLC.Utils;

namespace VLC.Commands.MusicLibrary
{
    public class AlbumClickedCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
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
                album = await Locator.MediaLibrary.LoadAlbum(id);
            }
            try
            {
                if (album != null)
                {
                    Locator.MusicLibraryVM.CurrentArtist = await Locator.MediaLibrary.LoadArtist(album.ArtistId);
                    Locator.MusicLibraryVM.CurrentAlbum = album;
                }
            }
            catch { }
            if (album != null)
                Locator.NavigationService.Go(VLCPage.AlbumPage);
        }
    }
}
