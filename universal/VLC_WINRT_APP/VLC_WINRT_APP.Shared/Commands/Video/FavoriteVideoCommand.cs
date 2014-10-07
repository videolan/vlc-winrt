/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using VLC_WINRT.Common;
using VLC_WINRT_APP.Model.Video;
using VLC_WINRT_APP.ViewModels.VideoVM;

namespace VLC_WINRT_APP.Commands.Video
{
    public class FavoriteVideoCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (parameter as VideoVM != null)
            {
                (parameter as VideoVM).Favorite = !(parameter as VideoVM).Favorite;
                //SerializationHelper.SerializeAsJson(Locator.MainVM.VideoVM.Media, "VideosDB.json", null,
                //    CreationCollisionOption.ReplaceExisting);
            }
        }
    }
}
