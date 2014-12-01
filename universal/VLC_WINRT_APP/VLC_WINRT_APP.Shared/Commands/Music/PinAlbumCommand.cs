using System;
using System.Collections.Generic;
using System.Text;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Converters;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Model.Music;

namespace VLC_WINRT_APP.Commands.Music
{
    public class PinAlbumCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (parameter is AlbumItem)
            {
                var album = parameter as AlbumItem;
                UpdateTileHelper.CreateOrReplaceSecondaryTile(VLCItemType.Album, album.Id, album.Name);
            }
        }
    }
}
