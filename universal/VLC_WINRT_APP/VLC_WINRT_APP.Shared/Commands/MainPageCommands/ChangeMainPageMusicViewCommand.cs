using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Controls;
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
#if WINDOWS_PHONE_APP
            if (App.ApplicationFrame.CurrentSourcePageType != typeof(MainPageHome)) return;
            if ((App.ApplicationFrame.Content as MainPageHome).MainPivot.SelectedIndex != 2) return;
#else
            if (App.ApplicationFrame.CurrentSourcePageType != typeof (MainPageMusic)) return;
#endif
#if WINDOWS_PHONE_APP
            var frame = (App.ApplicationFrame.Content as MainPageHome).MainPageMusicPivotItem.MainPageMusicContentPresenter;
#else
            var frame = (App.ApplicationFrame.Content as MainPageMusic).MainPageMusicContentPresenter;
#endif
            switch (index)
            {
                case 0:
                    if(frame.CurrentSourcePageType != typeof(AlbumsPivotItem))
                    {
                        frame.Navigate(typeof(AlbumsPivotItem));
                    }
                    break;
                case 1:
                    if (frame.CurrentSourcePageType != typeof(ArtistsPivotItem))
                        frame.Navigate(typeof(ArtistsPivotItem));
                    break;
#if WINDOWS_PHONE_APP
                case 2:
                    if ((App.ApplicationFrame.Content as MainPageHome).MainPageMusicPivotItem.MainPageMusicContentPresenter.CurrentSourcePageType != typeof(SongsPivotItem))
                        (App.ApplicationFrame.Content as MainPageHome).MainPageMusicPivotItem.MainPageMusicContentPresenter
                            .Navigate(typeof(SongsPivotItem));
                    break;
#endif
                case 3:
                    if (frame.CurrentSourcePageType != typeof (PlaylistPivotItem))
                        frame.Navigate(typeof (PlaylistPivotItem));
                    break;
                default:
                    frame.Navigate(typeof (AlbumsPivotItem));
                    break;
            }
        }
    }
}