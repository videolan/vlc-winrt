using System.Collections.ObjectModel;
using System.Linq;
using VLC_WinRT.Helpers;
using VLC_WinRT.Model;
using VLC_WinRT.Model.Music;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;
using WinRTXamlToolkit.Tools;

namespace VLC_WinRT.Commands.MusicPlayer
{
    public class PlayAllRandomCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            if (Locator.MusicLibraryVM.Tracks == null || !Locator.MusicLibraryVM.Tracks.Any()) return;
            var shuffledTracks = Locator.MusicLibraryVM.Tracks.Shuffle();
            await PlaylistHelper.AddTrackCollectionToPlaylistAndPlay(shuffledTracks.ToPlaylist());
            Locator.NavigationService.Go(VLCPage.MusicPlayerPage);
        }
    }
}
