using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.VideoLibrary
{
    public class StartVideoIndexingCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            // Reset the current in memory video library.
            // So if the user removed items, they will be removed in their video library
            // without having to restart VLC.
            Locator.VideoLibraryVM.ResetLibrary();
            await Locator.VideoLibraryVM.PerformRoutineCheckIfNotBusy();
        }
    }
}
