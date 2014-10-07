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
            Locator.MusicPlayerVM.TrackCollection.Shuffle();
            Random r = new Random();
            for (int i = 0; i < Locator.MusicPlayerVM.TrackCollection.Count; i++)
            {
                int index1 = r.Next(Locator.MusicPlayerVM.TrackCollection.Count);
                int index2 = r.Next(Locator.MusicPlayerVM.TrackCollection.Count);
                Locator.MusicPlayerVM.TrackCollection.Move(index1, index2);
            }
        }
    }
}
