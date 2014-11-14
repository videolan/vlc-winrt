using Windows.UI.Core;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Views.MusicPages;
using System;

namespace VLC_WINRT_APP.Commands.Music
{
    public class ShowCreateNewPlaylistPane : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
#if WINDOWS_PHONE_APP
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                var createPlaylist = new CreateNewPlaylist();
                await createPlaylist.ShowAsync();
            });
#endif
        }
    }
}
