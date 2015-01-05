/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/
#if WINDOWS_PHONE_APP
using Windows.Media.Playback;
#endif
using Autofac;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Services.Interface;

namespace VLC_WINRT_APP.Commands.MediaPlayback
{
    public class PlayPauseCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
#if WINDOWS_APP
            var playerService = App.Container.Resolve<IMediaService>();
            playerService.Pause();
#else
            if (BackgroundMediaPlayer.Current != null
                || BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Paused
                || BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Playing)
            {
                switch (BackgroundMediaPlayer.Current.CurrentState)
                {
                    case MediaPlayerState.Paused:
                        BackgroundMediaPlayer.Current.Play();
                        break;
                    case MediaPlayerState.Playing:
                        BackgroundMediaPlayer.Current.Pause();
                        break;
                }
            }
            else
            {
                var playerService = App.Container.Resolve<IMediaService>();
                playerService.Pause();
            }
#endif
        }
    }
}
