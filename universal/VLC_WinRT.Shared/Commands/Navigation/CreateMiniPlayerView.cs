using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using VLC_WinRT.Helpers;
using VLC_WinRT.Model;
using VLC_WinRT.UI.Legacy.Views.MusicPages;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.Navigation
{
    public class CreateMiniPlayerView : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            Locator.NavigationService.Go(VLCPage.MiniPlayerView);
        }
    }
}
