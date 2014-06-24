/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

#if NETFX_CORE
#endif
#if WINDOWS_PHONE_APP
using VLC_WINPRT;
#endif
using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.UI.Xaml.Media.Animation;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT.ViewModels;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MusicVM;

namespace VLC_WINRT_APP.Commands
{
    public class PlayAlbumCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            try
            {
                MusicLibraryVM.AlbumItem album = parameter as MusicLibraryVM.AlbumItem;
                Locator.MusicPlayerVM.ResetCollection();
                Locator.MusicPlayerVM.AddTrack(album.Tracks.ToList());
                await Locator.MusicPlayerVM.Play();
            }
            catch (FileNotFoundException exception)
            {
                Locator.MainVM.InitMusicM();
                ToastHelper.Basic("The file doesn't exists anymore. Rebuilding the music database", true);
                Debug.WriteLine("It seems that file doesn't exist anymore. Needs to rebuild Music Library");
                return;
            }

        var frame = App.ApplicationFrame;
            //#if NETFX_CORE
            //var page = frame.Content as MainPage;
            //#endif
            //#if WINDOWS_PHONE_APP
            //var page = frame.Content as VLC_WINPRT.Views.MainPage;
            //#endif
            //if (page != null)
            //{
            //    var sB = page.Resources["FadeOutPage"] as Storyboard;
            //    if (sB != null)
            //    {
            //        #if NETFX_CORE
            //        await sB.BeginAsync();
            //        App.ApplicationFrame.Navigate(typeof (ArtistPage));
            //        #endif


            //    }
            //}
        }
    }
}
