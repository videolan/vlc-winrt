﻿using VLC_WINRT.Common;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Commands.Music
{
    public class ArtistAlbumsSemanticZoomInvertZoomCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            Locator.MusicLibraryVM.IsMainPageMusicArtistAlbumsSemanticZoomViewedIn =
                !Locator.MusicLibraryVM.IsMainPageMusicArtistAlbumsSemanticZoomViewedIn;
        }
    }
}