using VLC.Model.Video;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.VideoLibrary
{
    public class RestartAndPlayCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if(parameter is VideoItem video)
                video.TimeWatchedSeconds = 0; 
            Locator.VideoLibraryVM.OpenVideo.Execute(parameter);
        }
    }
}