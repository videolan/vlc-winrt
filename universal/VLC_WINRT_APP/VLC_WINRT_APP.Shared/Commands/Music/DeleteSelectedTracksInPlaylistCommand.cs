using System;
using Windows.UI.Core;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MusicVM;

namespace VLC_WINRT_APP.Commands.Music
{
    public class DeleteSelectedTracksInPlaylistCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            var selectedTracks = Locator.MusicLibraryVM.CurrentTrackCollection.SelectedTracks;
            foreach (var selectedItem in selectedTracks)
            {
                var trackItem = selectedItem as TrackItem;
                if (trackItem == null) continue;
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        await
                            Locator.MusicLibraryVM.TracklistItemRepository.Remove(trackItem.Id,
                                Locator.MusicLibraryVM.CurrentTrackCollection.Id);
                        Locator.MusicLibraryVM.CurrentTrackCollection.Remove(trackItem);
                    }
                    catch (Exception exception)
                    {
                        LogHelper.Log(exception);
                    }
                });
            }
            await
                App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => Locator.MusicLibraryVM.CurrentTrackCollection.SelectedTracks.Clear());
        }
    }
}
