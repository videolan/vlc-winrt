using System;
using System.Collections.Generic;
using System.Text;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.MusicLibrary
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
