using System;
using System.Collections.Generic;
using System.Text;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Model.Music;

namespace VLC_WINRT_APP.Commands.Music
{
    public class PinArtistCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (parameter is ArtistItem)
            {
                var artist = parameter as ArtistItem;
                UpdateTileHelper.CreateOrReplaceSecondaryTile(VLCItemType.Artist, artist.Id, artist.Name);
            }
        }
    }
}
