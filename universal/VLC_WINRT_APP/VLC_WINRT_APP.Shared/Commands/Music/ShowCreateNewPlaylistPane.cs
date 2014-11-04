using Windows.UI.Core;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Views.MusicPages;

namespace VLC_WINRT_APP.Commands.Music
{
    public class ShowCreateNewPlaylistPane : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
#if WINDOWS_PHONE_APP
                var createPlaylist = new CreateNewPlaylist();
                createPlaylist.ShowAsync();
#endif
            });
        }
    }
}
