using VLC_WINRT.Common;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MusicVM;

namespace VLC_WINRT_APP.Commands.Music
{
    public class PinArtistCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            if (parameter is ArtistItem)
            {
                var artist = parameter as ArtistItem;
                artist.IsPinned = !artist.IsPinned;
                await Locator.MusicLibraryVM._artistDataRepository.Update(artist);
                UpdateTileHelper.CreateOrReplaceSecondaryTile(VLCItemType.Artist, artist.Id, artist.Name);
            }
        }
    }
}
