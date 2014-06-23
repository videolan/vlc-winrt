/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.Diagnostics;
using System.IO;
using System.Linq;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Helpers;
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT.ViewModels;
using VLC_WINRT.ViewModels.MainPage;
using VLC_WINRT_APP;
#if NETFX_CORE
using VLC_WINRT.Views;
#endif
#if WINDOWS_PHONE_APP
using VLC_WINPRT;
#endif

using Windows.UI.Xaml.Media.Animation;

namespace VLC_WINRT.Utility.Commands
{
    public class PlayAlbumCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            try
            {

                MusicLibraryViewModel.AlbumItem album = parameter as MusicLibraryViewModel.AlbumItem;
                Locator.MusicPlayerVM.TrackCollection.ResetCollection();
                Locator.MusicPlayerVM.TrackCollection.AddTrack(album.Tracks.ToList());
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
            #if NETFX_CORE
            var page = frame.Content as Views.MainPage;
            #endif
            #if WINDOWS_PHONE_APP
            var page = frame.Content as VLC_WINPRT.Views.MainPage;
            #endif
            if (page != null)
            {
                var sB = page.Resources["FadeOutPage"] as Storyboard;
                if (sB != null)
                {
                    #if NETFX_CORE
                    await sB.BeginAsync();
                    NavigationService.NavigateTo(typeof (PlayMusic));
                    #endif


                }
            }
        }
    }
}
