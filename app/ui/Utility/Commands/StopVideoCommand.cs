using VLC_WINRT.Common;
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT.ViewModels.PlayVideo;
using VLC_WINRT.Views;

namespace VLC_WINRT.Utility.Commands
{
    public class StopVideoCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            var vm = (PlayVideoViewModel) parameter;
            vm.Stop();
            NavigationService.NavigateTo(new MainPage());
        }
    }
}