using System.Collections.ObjectModel;
using System.Linq;
using VLC_WINRT.Common;
using VLC_WinRT.Helpers.MusicPlayer;
using VLC_WinRT.Model;
using VLC_WinRT.Model.Music;
using VLC_WinRT.ViewModels;
using VLC_WinRT.ViewModels.MusicVM;
using VLC_WinRT.Views.MusicPages;
using WinRTXamlToolkit.Tools;

namespace VLC_WinRT.Commands.Music
{
    public class PlayAllRandomCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            if (Locator.MusicLibraryVM.Tracks == null || !Locator.MusicLibraryVM.Tracks.Any()) return;
            var shuffledTracks = Locator.MusicLibraryVM.Tracks.Shuffle();
            await PlayMusicHelper.AddTrackCollectionToPlaylistAndPlay(new ObservableCollection<TrackItem>(shuffledTracks));
            if (App.ApplicationFrame.CurrentSourcePageType != typeof (MusicPlayerPage))
                App.ApplicationFrame.Navigate(typeof (MusicPlayerPage));
        }
    }
}
