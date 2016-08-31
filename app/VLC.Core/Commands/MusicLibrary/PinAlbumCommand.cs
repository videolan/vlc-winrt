using VLC.Helpers;
using VLC.Model;
using VLC.Model.Music;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.MusicLibrary
{
    public class PinAlbumCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            if (parameter is AlbumItem)
            {
                var album = parameter as AlbumItem;
                if (await TileHelper.CreateOrReplaceSecondaryTile(VLCItemType.Album, album.Id, album.Name))
                {
                    album.IsPinned = !album.IsPinned;
                    Locator.MediaLibrary.Update(album);
                }
            }
        }
    }
}
