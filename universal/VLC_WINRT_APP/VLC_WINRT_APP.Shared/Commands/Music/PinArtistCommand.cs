using VLC_WINRT.Common;
using VLC_WinRT.Helpers;
using VLC_WinRT.Model;
using VLC_WinRT.Model.Music;
using VLC_WinRT.ViewModels;
using VLC_WinRT.ViewModels.MusicVM;

namespace VLC_WinRT.Commands.Music
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
