using Windows.Media;
using VLC_WINRT.Common;
using VLC_WINRT.ViewModels;

namespace VLC_WINRT.Utility.Commands.MusicPlayer
{
    public class MusicPlayOrPauseCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (MediaControl.IsPlaying)
                Locator.MusicPlayerVM.Pause();
            else
                Locator.MusicPlayerVM.Resume();
        }
    }
}
