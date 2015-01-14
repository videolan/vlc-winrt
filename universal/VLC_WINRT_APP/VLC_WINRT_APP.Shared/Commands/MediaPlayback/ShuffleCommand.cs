using System;
using System.Collections.ObjectModel;
using System.Linq;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.ViewModels;
using WinRTXamlToolkit.Tools;

namespace VLC_WINRT_APP.Commands.MediaPlayback
{
    public class ShuffleCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            if (Locator.MusicPlayerVM.TrackCollection == null
                || Locator.MusicPlayerVM.TrackCollection.Playlist == null
                || !Locator.MusicPlayerVM.TrackCollection.Playlist.Any()) return;
            await Locator.MusicPlayerVM.TrackCollection.Shuffle();
        }
    }
}
