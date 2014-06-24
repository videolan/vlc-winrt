/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using VLC_WINRT.Common;
using VLC_WINRT.ViewModels.MainPage;

namespace VLC_WINRT_APP.Commands.VideoPlayer
{
    public class FavoriteVideoCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (parameter as MediaViewModel != null)
            {
                (parameter as MediaViewModel).Favorite = !(parameter as MediaViewModel).Favorite;
                //SerializationHelper.SerializeAsJson(Locator.MainVM.VideoVM.Media, "VideosDB.json", null,
                //    CreationCollisionOption.ReplaceExisting);
            }
        }
    }
}
