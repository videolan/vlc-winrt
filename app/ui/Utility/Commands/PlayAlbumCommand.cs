using System;
using System.Diagnostics;
using System.Linq;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT.ViewModels;
using VLC_WINRT.ViewModels.MainPage;

namespace VLC_WINRT.Utility.Commands
{
    public class PlayAlbumCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            MusicLibraryViewModel.AlbumItem album = parameter as MusicLibraryViewModel.AlbumItem;
            Locator.MusicPlayerVM.TrackCollection.ResetCollection();
            Locator.MusicPlayerVM.TrackCollection.AddTrack(album.Tracks.ToList());
            Locator.MusicPlayerVM.PlayNext();
        }
    }
}