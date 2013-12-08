using VLC_WINRT.Common;
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT.ViewModels;
using VLC_WINRT.Views;

namespace VLC_WINRT.Utility.Commands
{
    public class StopVideoCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            var mediaplayerService = IoC.IoC.GetInstance<MediaPlayerService>();
            mediaplayerService.Stop();
            mediaplayerService.Close();
            NavigationService.NavigateTo(new MainPage());
        }
    }
}