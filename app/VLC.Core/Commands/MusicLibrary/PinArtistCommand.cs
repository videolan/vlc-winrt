using VLC.Helpers;
using VLC.Model;
using VLC.Model.Music;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.MusicLibrary
{
    public class PinArtistCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            if (parameter is ArtistItem)
            {
                var artist = parameter as ArtistItem;
                //if (await TileHelper.CreateOrReplaceSecondaryTile(VLCItemType.Artist, artist.Id, artist.Name))
                //{
                //    artist.IsPinned = !artist.IsPinned;
                //    Locator.MediaLibrary.Update(artist);
                //}
            }
        }
    }
}
