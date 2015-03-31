using System;
using VLC_WINRT.Common;
using VLC_WinRT.Views.MusicPages;

namespace VLC_WinRT.Commands.Music
{
    public class OpenAddAlbumToPlaylistDialog : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            var addToPlaylist = new AddAlbumToPlaylist();
#if WINDOWS_PHONE_APP
            var _ = await addToPlaylist.ShowAsync();
#else
            addToPlaylist.ShowIndependent();
#endif
        }
    }
}
