using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using VLC.Helpers;
using VLC.Model;
using VLC.UI.Legacy.Views.MusicPages;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.Navigation
{
    public class CreateMiniPlayerView : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            Locator.NavigationService.Go(VLCPage.MiniPlayerView);
        }
    }
}
