using VLC_WinRT.Helpers;
using VLC_WinRT.Model;
using VLC_WinRT.Model.Music;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.MusicLibrary
{
    public class PinArtistCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            if (parameter is ArtistItem)
            {
                var artist = parameter as ArtistItem;
                if (await UpdateTileHelper.CreateOrReplaceSecondaryTile(VLCItemType.Artist, artist.Id, artist.Name))
                {
                    artist.IsPinned = !artist.IsPinned;
                    await Locator.MediaLibrary.Update(artist);
                }
            }
        }
    }
}
