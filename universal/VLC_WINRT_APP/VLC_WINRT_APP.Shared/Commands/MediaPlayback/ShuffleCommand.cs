using System;
using System.Collections.ObjectModel;
using System.Linq;
using VLC_WINRT.Common;
using VLC_WinRT.Model.Music;
using VLC_WinRT.ViewModels;
using WinRTXamlToolkit.Tools;

namespace VLC_WinRT.Commands.MediaPlayback
{
    public class ShuffleCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            if (Locator.MediaPlaybackViewModel.TrackCollection == null
                || Locator.MediaPlaybackViewModel.TrackCollection.Playlist == null
                || !Locator.MediaPlaybackViewModel.TrackCollection.Playlist.Any()
                || Locator.MediaPlaybackViewModel.TrackCollection.Playlist.Count < 3) return;
            await Locator.MediaPlaybackViewModel.TrackCollection.Shuffle();
        }
    }
}
