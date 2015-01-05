using System;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Views.MusicPages;

namespace VLC_WINRT_APP.Commands.Music
{
    public class OpenAddAlbumToPlaylistDialog : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
#if WINDOWS_PHONE_APP
            var addToPlaylist = new AddAlbumToPlaylist();
            var _ = await addToPlaylist.ShowAsync();
#endif
        }
    }
}
