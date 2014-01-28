using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Helpers;
using VLC_WINRT.ViewModels;
using VLC_WINRT.ViewModels.MainPage;

namespace VLC_WINRT.Utility.Commands.VideoPlayer
{
    public class FavoriteVideoCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (parameter as MediaViewModel != null)
            {
                (parameter as MediaViewModel).Favorite = !(parameter as MediaViewModel).Favorite;
                //SerializationHelper.SerializeAsJson(Locator.MainPageVM.VideoVM.Media, "VideosDB.json", null,
                //    CreationCollisionOption.ReplaceExisting);
            }
        }
    }
}