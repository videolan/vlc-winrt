
using VLC_WINRT.Common;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Commands.Music
{
    public class StartMusicIndexingCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            await Locator.MusicLibraryVM.PerformRoutineCheckIfNotBusy();
        }
    }
}
