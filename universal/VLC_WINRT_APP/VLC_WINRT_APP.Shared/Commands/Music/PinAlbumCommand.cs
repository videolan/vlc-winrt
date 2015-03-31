using VLC_WINRT.Common;
using VLC_WinRT.Helpers;
using VLC_WinRT.Model;
using VLC_WinRT.Model.Music;
using VLC_WinRT.ViewModels;
using VLC_WinRT.ViewModels.MusicVM;

namespace VLC_WinRT.Commands.Music
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
