using System;
using System.Diagnostics;
using System.Linq;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT.ViewModels;
using VLC_WINRT.ViewModels.MainPage;
using VLC_WINRT.Views;
using Windows.UI.Xaml.Media.Animation;
using VLC_WINRT.Utility.Helpers;

namespace VLC_WINRT.Utility.Commands
{
    public class PlayAlbumCommand : AlwaysExecutableCommand
    {
        public async override void Execute(object parameter)
        {
            MusicLibraryViewModel.AlbumItem album = parameter as MusicLibraryViewModel.AlbumItem;
            Locator.MusicPlayerVM.TrackCollection.ResetCollection();
            Locator.MusicPlayerVM.TrackCollection.AddTrack(album.Tracks.ToList());
            Locator.MusicPlayerVM.PlayNext();

            var frame = App.ApplicationFrame;
            var page = frame.Content as MainPage;
            if (page != null)
            {
                var sB = page.Resources["FadeOutPage"] as Storyboard;
                if (sB != null)
                {
                    await sB.BeginAsync();
                    NavigationService.NavigateTo(typeof(PlayMusic));
                }
            }
        }
    }
}