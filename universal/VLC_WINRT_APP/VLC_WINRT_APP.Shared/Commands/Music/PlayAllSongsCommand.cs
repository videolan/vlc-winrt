using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Controls;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Helpers.MusicPlayer;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Views.MusicPages;

namespace VLC_WINRT_APP.Commands.Music
{
    public class PlayAllSongsCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            if (Locator.MusicLibraryVM.Tracks == null || !Locator.MusicLibraryVM.Tracks.Any()) return;
            var itemClickArgs = parameter as ItemClickEventArgs;
            if (itemClickArgs != null)
            {
                var selectedTrack = itemClickArgs.ClickedItem as TrackItem;
                var index = Locator.MusicLibraryVM.Tracks.IndexOf(selectedTrack);
                if (selectedTrack != null)
                {
                    await PlayMusicHelper.AddTrackCollectionToPlaylistAndPlay(Locator.MusicLibraryVM.Tracks, true, index);
                    if (App.ApplicationFrame.CurrentSourcePageType != typeof(MusicPlayerPage))
                        App.ApplicationFrame.Navigate(typeof(MusicPlayerPage));
                }
            }
        }
    }
}
