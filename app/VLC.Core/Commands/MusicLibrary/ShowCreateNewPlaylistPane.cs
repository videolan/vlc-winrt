using Windows.UI.Core;
using System;
using VLC.Model;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.MusicLibrary
{
    public class ShowCreateNewPlaylistPane : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () =>
            {
                Locator.NavigationService.Go(VLCPage.CreateNewPlaylistDialog);
            });
        }
    }
}
