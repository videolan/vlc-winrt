using System;
using System.Collections.Generic;
using System.Text;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.VideoPlayer
{
    public class InitPiPCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            Locator.NavigationService.Go(Locator.SettingsVM.HomePage);
            App.RootPage.StartCompositionAnimationOnSwapChain(true);
        }
    }
}
