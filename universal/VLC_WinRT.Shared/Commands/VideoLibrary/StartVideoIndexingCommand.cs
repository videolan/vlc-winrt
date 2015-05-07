using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.VideoLibrary
{
    public class StartVideoIndexingCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            await Locator.VideoLibraryVM.PerformRoutineCheckIfNotBusy();
        }
    }
}
