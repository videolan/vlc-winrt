using VLC.Model;
using VLC.Model.Music;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.MusicLibrary
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
