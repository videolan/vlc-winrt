using System;
using System.Collections.Generic;
using System.Text;
using VLC_WinRT.Model;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.Navigation
{
    public class ChangeSettingsViewCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            // TODO: Create an enum for SettingsView
            int index = 0;
            if (int.TryParse(parameter.ToString(), out index))
            {
                switch (index)
                {
                    case -1: // Home settings
                        Locator.NavigationService.Go(VLCPage.SettingsPage);
                        break;
                    case 0: // UserInterface
                        Locator.NavigationService.Go(VLCPage.SettingsPageUI);
                        break;
                    case 1:
                        Locator.NavigationService.Go(VLCPage.SettingsPageVideo);
                        break;
                    case 2:
                        Locator.NavigationService.Go(VLCPage.SettingsPageMusic);
                        break;
                }
            }
        }
    }
}