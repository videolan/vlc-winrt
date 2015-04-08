using Windows.UI.Core;
using System;
using VLC_WinRT.Model;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.Music
{
    public class ShowCreateNewPlaylistPane : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Locator.NavigationService.Go(VLCPage.CreateNewPlaylistDialog);
            });
        }
    }
}
