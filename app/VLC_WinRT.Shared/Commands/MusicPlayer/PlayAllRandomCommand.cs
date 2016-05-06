using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VLC_WinRT.Helpers;
using VLC_WinRT.Model;
using VLC_WinRT.Model.Music;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;
using WinRTXamlToolkit.Tools;

namespace VLC_WinRT.Commands.MusicPlayer
{
    public class PlayAllRandomCommand : AttemptCommand
    {
        public override async Task<bool> Execute(object parameter)
        {
            var tracks = await Locator.MediaLibrary.LoadTracks();
            if (tracks == null || !tracks.Any()) return await Task.FromResult<bool>(false);
            Locator.NavigationService.Go(VLCPage.MusicPlayerPage);
            var shuffledTracks = tracks.Shuffle();
            await PlaylistHelper.AddTrackCollectionToPlaylistAndPlay(shuffledTracks.ToPlaylist());
            return await Task.FromResult<bool>(false);
        }
    }
}
