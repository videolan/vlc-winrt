using System.Linq;
using Windows.UI.Xaml.Controls;
using VLC_WinRT.Model;
using VLC_WinRT.Model.Music;
using VLC_WinRT.ViewModels;
using System.Collections.ObjectModel;
using VLC_WinRT.Utils;
using VLC_WinRT.Helpers;

namespace VLC_WinRT.Commands.MusicPlayer
{
    public class PlayAllSongsCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            if (Locator.MusicLibraryVM.Tracks == null || !Locator.MusicLibraryVM.Tracks.Any()) return;
            var itemClickArgs = parameter as ItemClickEventArgs;
            if (itemClickArgs != null)
            {
                var selectedTrack = itemClickArgs.ClickedItem as TrackItem;
                var index = Locator.MusicLibraryVM.Tracks.IndexOf(selectedTrack);
                if (selectedTrack != null)
                {
                    await PlaylistHelper.AddTrackCollectionToPlaylistAndPlay(Locator.MusicLibraryVM.Tracks.ToPlaylist(), true, index);
                    Locator.NavigationService.Go(VLCPage.MusicPlayerPage);
                }
            }
        }
    }
}
