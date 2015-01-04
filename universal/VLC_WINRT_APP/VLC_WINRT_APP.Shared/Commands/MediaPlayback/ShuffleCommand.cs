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
        public override void Execute(object parameter)
        {
            if (Locator.MusicPlayerVM.TrackCollection == null
                || Locator.MusicPlayerVM.TrackCollection.Playlist == null
                || !Locator.MusicPlayerVM.TrackCollection.Playlist.Any()) return;
                if (Locator.MusicPlayerVM.TrackCollection.IsShuffled)
                {
                    Locator.MusicPlayerVM.TrackCollection.Playlist = Locator.MusicPlayerVM.TrackCollection.NonShuffledPlaylist;
                    //Locator.MusicPlayerVM.TrackCollection.NonShuffledPlaylist.Clear();
                }
                else
                {
                    Locator.MusicPlayerVM.TrackCollection.NonShuffledPlaylist = new ObservableCollection<TrackItem>(Locator.MusicPlayerVM.TrackCollection.Playlist);
                    Random r = new Random();
                    for (int i = 0; i < Locator.MusicPlayerVM.TrackCollection.Playlist.Count; i++)
                    {
                        int index1 = r.Next(Locator.MusicPlayerVM.TrackCollection.Playlist.Count);
                        int index2 = r.Next(Locator.MusicPlayerVM.TrackCollection.Playlist.Count);
                        Locator.MusicPlayerVM.TrackCollection.Playlist.Move(index1, index2);
                    }   
                }
                Locator.MusicPlayerVM.TrackCollection.IsShuffled = !Locator.MusicPlayerVM.TrackCollection.IsShuffled;
                Locator.MusicPlayerVM.TrackCollection.CurrentTrack =
                    Locator.MusicPlayerVM.TrackCollection.Playlist.IndexOf(
                        Locator.MusicPlayerVM.TrackCollection.Playlist.FirstOrDefault(x => x.IsCurrentPlaying == true));
        }
    }
}
