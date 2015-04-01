using Windows.UI.Core;
using VLC_WINRT.Common;
using VLC_WinRT.Views.MusicPages;
using System;

namespace VLC_WinRT.Commands.Music
{
    public class ShowCreateNewPlaylistPane : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var createPlaylist = new CreateNewPlaylist();
                App.RootPage.SplitShell.RightFlyoutContent = createPlaylist;
                App.RootPage.SplitShell.ShowFlyout();
            });
        }
    }
}
