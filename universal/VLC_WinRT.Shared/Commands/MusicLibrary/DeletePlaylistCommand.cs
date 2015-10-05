using System;
using Windows.UI.Core;
using Windows.UI.Popups;
using VLC_WinRT.Helpers.MusicLibrary;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.MusicLibrary
{
    public class DeletePlaylistCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            var md = new MessageDialog(Strings.YourPlaylistWontBeAccessible, Strings.AreYouSure );
            md.Commands.Add(new UICommand("yes", async command =>
            {
                await MusicLibraryManagement.DeletePlaylist(Locator.MusicLibraryVM.CurrentTrackCollection);
                Locator.NavigationService.GoBack_Specific();
            }));
            md.Commands.Add(new UICommand("no"));
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => await md.ShowAsync());
        }
    }
}
