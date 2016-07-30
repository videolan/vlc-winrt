using System;
using System.Collections.Generic;
using System.Text;
using VLC.Model.Stream;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.StreamsLibrary
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
