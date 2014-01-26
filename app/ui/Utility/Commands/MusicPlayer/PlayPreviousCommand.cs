using VLC_WINRT.Common;
using VLC_WINRT.ViewModels;

namespace VLC_WINRT.Utility.Commands.MusicPlayer
{
    public class PlayPreviousCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            Locator.MusicPlayerVM.PlayPrevious();
        }
    }
}
