using System;
using System.Collections.Generic;
using System.Text;
using VLC_WinRT.Model.Stream;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.StreamsLibrary
{
    public class FavoriteStreamCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            if (parameter is StreamMedia)
            {
                var stream = parameter as StreamMedia;
                stream.Favorite = !stream.Favorite;

                await Locator.MediaLibrary.Update(stream);
                await Locator.MediaLibrary.LoadStreamsFromDatabase();
            }
        }
    }
}
