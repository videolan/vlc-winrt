using System;
using VLC_WINRT.Common;
using VLC_WINRT_APP.ViewModels;
using WinRTXamlToolkit.Tools;

namespace VLC_WINRT_APP.Commands.MediaPlayback
{
    public class ShuffleCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            Random r = new Random();
            for (int i = 0; i < Locator.MusicPlayerVM.TrackCollection.Playlist.Count; i++)
            {
                int index1 = r.Next(Locator.MusicPlayerVM.TrackCollection.Playlist.Count);
                int index2 = r.Next(Locator.MusicPlayerVM.TrackCollection.Playlist.Count);
                Locator.MusicPlayerVM.TrackCollection.Playlist.Move(index1, index2);
            }
        }
    }
}
