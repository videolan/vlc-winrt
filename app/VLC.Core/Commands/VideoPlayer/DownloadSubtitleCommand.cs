using System;
using System.Collections.Generic;
using System.Text;
using VLC.Utils;
using VLC.ViewModels;
using Windows.Storage;

namespace VLC.Commands.VideoPlayer
{
    public class DownloadSubtitleCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            Locator.VideoPlayerVm.IsLoadingSubtitle = true;
            Locator.VideoPlayerVm.LoadingSubtitleText = Strings.Loading;

            if (Locator.VideoPlayerVm.CurrentVideo == null)
            {
                Locator.VideoPlayerVm.LoadingSubtitleText = Strings.NoResults;
                Locator.VideoPlayerVm.IsLoadingSubtitle = false;
                return;
            }
            var success = false;
            if (Locator.VideoPlayerVm.CurrentVideo.IsSubtitlePreLoaded)
                success = true;
            else
                success = await Locator.VideoMetaService.GetMovieSubtitle(Locator.VideoPlayerVm.CurrentVideo);

            if (success)
            {
                var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(Locator.VideoPlayerVm.CurrentVideo.SubtitleUri));
                Locator.MediaPlaybackViewModel.OpenSubtitleCommand.Execute(file);
                Locator.VideoPlayerVm.LoadingSubtitleText = string.Empty;
            }
            else
            {
                Locator.VideoPlayerVm.LoadingSubtitleText = Strings.NoResults;
            }

            Locator.VideoPlayerVm.IsLoadingSubtitle = false;
        }
    }
}
