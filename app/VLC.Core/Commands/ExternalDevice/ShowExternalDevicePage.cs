using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.ExternalDevice
{
    public class ShowExternalDevicePage : AlwaysExecutableCommand
    {
        public override void Execute(object param)
        {
            Locator.NavigationService.Go(Model.VLCPage.ExternalStorageInclude, param);
        }
    }
}
