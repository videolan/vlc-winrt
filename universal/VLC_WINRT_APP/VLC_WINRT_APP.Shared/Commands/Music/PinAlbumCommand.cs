using VLC_WINRT.Common;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MusicVM;

namespace VLC_WINRT_APP.Commands.Music
{
    public class PinAlbumCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            if (parameter is AlbumItem)
            {
                var album = parameter as AlbumItem;
                album.IsPinned = !album.IsPinned;
                await Locator.MusicLibraryVM._albumDataRepository.Update(album);
                UpdateTileHelper.CreateOrReplaceSecondaryTile(VLCItemType.Album, album.Id, album.Name);
            }
        }
    }
}
