using System;
using System.Collections.Generic;
using System.Text;
using VLC.Utils;
using VLC.ViewModels;
using Windows.UI.ViewManagement;
using Windows.Foundation.Metadata;

namespace VLC.Commands.VideoPlayer
{
    public class InitPiPCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            if (Locator.SettingsVM.CompactOverlayPiP && Locator.VideoPlayerVm.IsCompactOverlaySupported)
            {
                if (ApplicationView.GetForCurrentView().ViewMode == ApplicationViewMode.Default)
                {
                    await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay);
                    Locator.VideoPlayerVm.RequestChangeControlBarVisibility(false);
                    return;
                }

                if (ApplicationView.GetForCurrentView().ViewMode == ApplicationViewMode.CompactOverlay)
                {
                    await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);
                    return;
                }
            }
            else
            {
                Locator.NavigationService.Go(Locator.SettingsVM.HomePage);
                App.RootPage.StartCompositionAnimationOnSwapChain(true);
            } 
        }
    }
}
