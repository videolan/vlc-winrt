using System;
using Windows.UI.Core;
using Windows.UI.Popups;
using VLC.Helpers.MusicLibrary;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.MusicLibrary
{
    public class DeletePlaylistCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            var md = new MessageDialog(Strings.YourPlaylistWontBeAccessible, Strings.AreYouSure );
            md.Commands.Add(new UICommand("yes", async command =>
            {
                await Locator.MediaLibrary.DeletePlaylist(Locator.MusicLibraryVM.CurrentTrackCollection);
                Locator.MusicLibraryVM.CurrentTrackCollection = null;
                Locator.NavigationService.GoBack_Specific();
            }));
            md.Commands.Add(new UICommand("no"));
            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, async () => await md.ShowAsync());
        }
    }
}
