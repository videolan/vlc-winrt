/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using Windows.Media;


namespace VLC_WINRT.Utility.Services.Interface
{
#if WINDOWS_PHONE_APP
    public class MediaControl
    {        
        // Summary:
        //     Gets or sets the path to the artwork for the album cover.
        //
        // Returns:
        //     Path to the artwork for the album cover.
        public static Uri AlbumArt { get; set; }
        //
        // Summary:
        //     Gets or sets the name of the artist.
        //
        // Returns:
        //     The name of the artist.
        public static string ArtistName { get; set; }
        //
        // Summary:
        //     Gets or sets the state of the Play button.
        //
        // Returns:
        //     The state of the Play button.
        public static bool IsPlaying { get; set; }
        //
        // Summary:
        //     Gets the current sound level.
        //
        // Returns:
        //     The current sound level.
        public static SoundLevel SoundLevel { get; set; }
        //
        // Summary:
        //     Gets or sets the track name.
        //
        // Returns:
        //     The name of the track.
        public static string TrackName { get; set; }

        // Summary:
        //     Event raised when a ChannelDown command is issued to the application.
        public static event EventHandler<object> ChannelDownPressed;
        //
        // Summary:
        //     Event raised when a ChannelUp command is issued to the application.
        public static event EventHandler<object> ChannelUpPressed;
        //
        // Summary:
        //     Event raised when a FastForward command is issued to the application.
        public static event EventHandler<object> FastForwardPressed;
        //
        // Summary:
        //     Event raised when a NextTrack command is issued to the application.
        public static event EventHandler<object> NextTrackPressed;
        //
        // Summary:
        //     Event raised when a Pause command is issued to the application.
        public static event EventHandler<object> PausePressed;
        //
        // Summary:
        //     Event raised when a PlayPauseToggle command is issued to the application.
        public static event EventHandler<object> PlayPauseTogglePressed;
        //
        // Summary:
        //     Event raised when a Play command is issued to the application.
        public static event EventHandler<object> PlayPressed;
        //
        // Summary:
        //     Event raised when a PreviousTrack command is issued to the application.
        public static event EventHandler<object> PreviousTrackPressed;
        //
        // Summary:
        //     Event raised when a Record command is issued to the application.
        public static event EventHandler<object> RecordPressed;
        //
        // Summary:
        //     Event raised when a Rewind command is issued to the application.
        public static event EventHandler<object> RewindPressed;
        //
        // Summary:
        //     Event raised when the sound level changes.
        public static event EventHandler<object> SoundLevelChanged;
        //
        // Summary:
        //     Event raised when a Stop command is issued to the application.
        public static event EventHandler<object> StopPressed;
    }
#endif
}
