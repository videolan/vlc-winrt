using System;
using System.Collections.Generic;
using System.Text;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.VideoPlayer
{
    public class InitPopupCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            Locator.NavigationService.Go(Locator.SettingsVM.HomePage);
            App.RootPage.StartCompositionAnimationOnSwapChain(true);
        }
    }
}
