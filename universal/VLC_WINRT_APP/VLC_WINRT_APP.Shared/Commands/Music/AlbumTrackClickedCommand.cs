using Windows.UI.Xaml.Controls;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Helpers.MusicPlayer;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Views.MusicPages;

namespace VLC_WINRT_APP.Commands.Music
{
    public class AlbumTrackClickedCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            if (App.ApplicationFrame.CurrentSourcePageType != typeof(MusicPlayerPage))
                App.ApplicationFrame.Navigate(typeof(MusicPlayerPage));
            Locator.MusicLibraryVM.IsAlbumPageShown = false;
            TrackItem track = null;
            if (parameter is ItemClickEventArgs)
            {
                var args = parameter as ItemClickEventArgs;
                track = args.ClickedItem as TrackItem;
            }
            if (track == null)
            {
                // if the track is still null (for some reason), we need to break early.
                return;
            }
            await PlayMusicHelper.AddAlbumToPlaylist(track.AlbumId, true, true, track);
        }
    }
}
