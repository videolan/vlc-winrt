
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.MusicLibrary
{
    public class StartMusicIndexingCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            if (parameter != null && parameter.ToString() == "True")
            {
                await Locator.MusicLibraryVM.StartIndexing();
            }
            else await Locator.MusicLibraryVM.PerformRoutineCheckIfNotBusy();
        }
    }
}
