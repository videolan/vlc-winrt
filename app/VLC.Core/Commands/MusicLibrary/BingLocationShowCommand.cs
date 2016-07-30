using System;
using Windows.System;
using Windows.UI.Xaml.Controls;
using VLC.MusicMetaFetcher.Models.MusicEntities;
using VLC.Utils;

namespace VLC.Commands.MusicLibrary
{
    public class BingLocationShowCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            var itemClick = parameter as ItemClickEventArgs;
            if (itemClick != null)
            {
                var showItem = itemClick.ClickedItem as Show;
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
