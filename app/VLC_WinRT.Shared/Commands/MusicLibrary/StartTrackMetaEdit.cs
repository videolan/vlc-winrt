using VLC_WinRT.Model;
using VLC_WinRT.Model.Music;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.MusicLibrary
{
    public class StartTrackMetaEdit : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (parameter is TrackItem)
            {
                Locator.MusicLibraryVM.CurrentTrack = parameter as TrackItem;
                Locator.NavigationService.Go(VLCPage.TrackEditorPage);
            }
        }
    }
}
