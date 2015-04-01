using System;
using VLC_WINRT.Common;
using VLC_WinRT.Views.MusicPages;
using VLC_WinRT.Views.MusicPages.PlaylistControls;

namespace VLC_WinRT.Commands.Music
{
    public class OpenAddAlbumToPlaylistDialog : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            var addToPlaylist = new AddAlbumToPlaylistBase();
            App.RootPage.SplitShell.RightFlyoutContent = addToPlaylist;
            App.RootPage.SplitShell.ShowFlyout();
        }
    }
}
