﻿using VLC_WinRT.Utils;
﻿using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.MusicLibrary
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