using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VLC.Helpers;
using VLC.Model;
using VLC.Model.Music;
using VLC.Utils;
using VLC.ViewModels;
using WinRTXamlToolkit.Tools;

namespace VLC.Commands.MusicPlayer
{
    public class PlayAllRandomCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            var tracks = await Locator.MediaLibrary.LoadTracks();
            if (tracks == null || !tracks.Any())
                return;

            Locator.NavigationService.Go(VLCPage.MusicPlayerPage);
            var shuffledTracks = tracks.Shuffle();

            await Locator.MediaPlaybackViewModel.PlaybackService.SetPlaylist(shuffledTracks, true, true, shuffledTracks[0]);
        }
    }
}
