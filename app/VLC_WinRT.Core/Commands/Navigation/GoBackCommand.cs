using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.Navigation
{
    public class GoBackCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            Locator.NavigationService.GoBack_Specific();
        }
    }
}
