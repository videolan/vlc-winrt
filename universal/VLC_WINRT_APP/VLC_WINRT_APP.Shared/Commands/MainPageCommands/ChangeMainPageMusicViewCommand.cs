using System;
using System.Collections.Generic;
using System.Text;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Views.MainPages;
using VLC_WINRT_APP.Views.MainPages.MainMusicControls;
using VLC_WINRT_APP.Views.MusicPages;

namespace VLC_WINRT_APP.Commands.MainPageCommands
{
    public class ChangeMainPageMusicViewCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            var index = int.Parse(parameter.ToString());
            if (App.ApplicationFrame.CurrentSourcePageType != typeof (MainPageHome)) return;
            if ((App.ApplicationFrame.Content as MainPageHome).MainPivot.SelectedIndex != 2) return;
            switch (index)
            {
                case 0:
                    (App.ApplicationFrame.Content as MainPageHome).MainPageMusicPivotItem.MainPageMusicContentPresenter
                        .Navigate(typeof (AlbumsPivotItem));
                    break;
                case 1:
                    (App.ApplicationFrame.Content as MainPageHome).MainPageMusicPivotItem.MainPageMusicContentPresenter
                        .Navigate(typeof (ArtistsPivotItem));
                    break;
                case 2:
                    (App.ApplicationFrame.Content as MainPageHome).MainPageMusicPivotItem.MainPageMusicContentPresenter
                        .Navigate(typeof(SongsPivotItem));
                    break;
                case 3:
                    (App.ApplicationFrame.Content as MainPageHome).MainPageMusicPivotItem.MainPageMusicContentPresenter
                        .Navigate(typeof (PlaylistPivotItem));
                    break;
            }
        }
    }
}
