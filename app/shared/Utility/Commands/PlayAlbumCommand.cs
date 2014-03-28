/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.Linq;
using VLC_WINRT.Common;
using VLC_WINRT.ViewModels;
using VLC_WINRT.ViewModels.MainPage;
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
        public async override void Execute(object parameter)
        {
            MusicLibraryViewModel.AlbumItem album = parameter as MusicLibraryViewModel.AlbumItem;
            Locator.MusicPlayerVM.TrackCollection.ResetCollection();
            Locator.MusicPlayerVM.TrackCollection.AddTrack(album.Tracks.ToList());
            await Locator.MusicPlayerVM.Play();

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
