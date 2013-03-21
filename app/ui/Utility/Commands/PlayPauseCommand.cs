using VLC_WINRT.Common;
using VLC_WINRT.ViewModels;

namespace VLC_WINRT.Utility.Commands
{
    public class PlayPauseCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (ViewModelLocator.PlayVideoVM.IsPlaying)
            {
                ViewModelLocator.PlayVideoVM.Pause();
            }
            else
            {
                ViewModelLocator.PlayVideoVM.Play();
            }
        }
    }
}