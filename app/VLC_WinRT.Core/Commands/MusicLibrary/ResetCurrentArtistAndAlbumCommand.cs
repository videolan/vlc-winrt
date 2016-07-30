using System;
using System.Collections.Generic;
using System.Text;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.MusicLibrary
{
    public class ResetCurrentArtistAndAlbumCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            Locator.MusicLibraryVM.CurrentArtist = null;
            Locator.MusicLibraryVM.CurrentAlbum = null;
        }
    }
}
