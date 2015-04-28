using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.MainPageCommands
{
    public class DisplayMenuBarControlToggleCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            Locator.MainVM.MenuBarDisplayed = !Locator.MainVM.MenuBarDisplayed;
        }
    }
}
