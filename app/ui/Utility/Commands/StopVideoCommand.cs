using Microsoft.Practices.ServiceLocation;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT.Views;

namespace VLC_WINRT.Utility.Commands
{
    public class StopVideoCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            var mediaplayerService = ServiceLocator.Current.GetInstance<MediaPlayerService>();
            mediaplayerService.Stop();
            mediaplayerService.Close();
            NavigationService.NavigateTo(new MainPage());
        }
    }
}