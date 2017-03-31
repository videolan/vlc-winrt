using System.Linq;
using VLC.Helpers.MusicLibrary;
using VLC.Model.Music;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.MusicLibrary
{
    public class DeletePlaylistTrackCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            foreach (var item in Locator.MusicLibraryVM.CurrentTrackCollection.SelectedTracks.ToList())
            {
                Locator.MusicLibraryVM.CurrentTrackCollection.Remove(item);
                if (item is TrackItem)
                    Locator.MediaLibrary.DeletePlaylistTrack(item as TrackItem, Locator.MusicLibraryVM.CurrentTrackCollection);
            }
        }
    }
}
