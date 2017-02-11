using System;
using System.Collections.Generic;
using System.Text;
using VLC.Utils;
using VLC.ViewModels;
using Windows.UI.ViewManagement;

namespace VLC.Commands.VideoPlayer
{
    public class InitPiPCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            if (ApplicationView.GetForCurrentView().IsViewModeSupported(ApplicationViewMode.CompactOverlay))
            {
                await App.RootPage.StartPopupWindow();
            }
            else
            {
                Locator.NavigationService.Go(Locator.SettingsVM.HomePage);
                App.RootPage.StartCompositionAnimationOnSwapChain(true);
            }
        }
    }
}
