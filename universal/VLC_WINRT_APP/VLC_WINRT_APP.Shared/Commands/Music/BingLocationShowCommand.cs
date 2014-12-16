using System;
using Windows.System;
using Windows.UI.Xaml.Controls;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Model.Music;

namespace VLC_WINRT_APP.Commands.Music
{
    public class BingLocationShowCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            var itemClick = parameter as ItemClickEventArgs;
            if (itemClick != null)
            {
                var showItem = itemClick.ClickedItem as ShowItem;
                if (showItem != null)
                {
                    var uri =
                        new Uri("bingmaps:?collection=point." + showItem.Latitude + "_" + showItem.Longitude + "_" +
                                showItem.Title);
                    var launcher = await Launcher.LaunchUriAsync(uri);
                }
            }
        }
    }
}
