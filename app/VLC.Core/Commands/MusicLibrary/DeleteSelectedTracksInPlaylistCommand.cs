using System;
using Windows.UI.Core;
using VLC.Helpers;
using VLC.Model.Music;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.MusicLibrary
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
                await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        await
                            Locator.MediaLibrary.RemoveTrackInPlaylist(trackItem.Id,
                                Locator.MusicLibraryVM.CurrentTrackCollection.Id);
                        Locator.MusicLibraryVM.CurrentTrackCollection.Remove(trackItem);
                    }
                    catch (Exception exception)
                    {
                        LogHelper.Log(StringsHelper.ExceptionToString(exception));
                    }
                });
            }
            await
                DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal,
                    () => Locator.MusicLibraryVM.CurrentTrackCollection.SelectedTracks.Clear());
        }
    }
}
