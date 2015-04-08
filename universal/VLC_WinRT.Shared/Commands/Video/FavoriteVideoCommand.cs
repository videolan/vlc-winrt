/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using VLC_WinRT.Model.Video;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Commands.Video
{
    public class FavoriteVideoCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (parameter as VideoItem != null)
            {
                (parameter as VideoItem).Favorite = !(parameter as VideoItem).Favorite;
                //SerializationHelper.SerializeAsJson(Locator.MainVM.VideoVM.Media, "VideosDB.json", null,
                //    CreationCollisionOption.ReplaceExisting);
            }
        }
    }
}
