using System.Linq;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.MediaPlayback
{
    public class ShuffleCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            Locator.PlaybackService.ShufflePlaylist();
        }
    }
}
