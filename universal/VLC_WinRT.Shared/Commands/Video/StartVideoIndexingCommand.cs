using VLC_WINRT.Common;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.Video
{
    public class StartVideoIndexingCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            await Locator.VideoLibraryVM.PerformRoutineCheckIfNotBusy();
        }
    }
}
