﻿using VLC_WINRT.Common;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.Music
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