using System;
using Windows.UI.Core;
using Windows.UI.Popups;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Helpers.MusicLibrary;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Commands.Music
{
    public class DeletePlaylistCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            var md = new MessageDialog("Your playlist will no longer be accessible", "Are you sure?");
            md.Commands.Add(new UICommand("yes", async command =>
            {
                await MusicLibraryManagement.DeletePlaylist(Locator.MusicLibraryVM.CurrentTrackCollection);
                App.ApplicationFrame.GoBack();
            }));
            md.Commands.Add(new UICommand("no"));
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => md.ShowAsync());
        }
    }
}
