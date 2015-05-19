using System.Linq;
using VLC_WinRT.Helpers.MusicLibrary;
using VLC_WinRT.Model.Music;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.MusicLibrary
{
    public class DeletePlaylistTrackCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            foreach (var item in Locator.MusicLibraryVM.CurrentTrackCollection.SelectedTracks.ToList())
            {
                Locator.MusicLibraryVM.CurrentTrackCollection.Remove(item);
                if (item is TrackItem)
                    await MusicLibraryManagement.DeletePlaylistTrack(item as TrackItem, Locator.MusicLibraryVM.CurrentTrackCollection);
            }
            //Locator.MusicLibraryVM.CurrentTrackCollection.SelectedTracks.Clear();
        }
    }
}
