using VLC_WINRT.Common;
using VLC_WINRT.Utility.Services.RunTime;

namespace VLC_WINRT.Utility.Commands
{
    public class PlayPauseCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            var playerService = IoC.IoC.GetInstance<MediaPlayerService>();
            if (playerService.CurrentState == MediaPlayerService.MediaPlayerState.Playing)
            {
                playerService.Pause();
            }
            else
            {
                playerService.Play();
            }
        }
    }
}