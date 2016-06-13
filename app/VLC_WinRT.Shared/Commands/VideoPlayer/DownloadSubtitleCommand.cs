using System;
using System.Collections.Generic;
using System.Text;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;
using Windows.Storage;

namespace VLC_WinRT.Commands.VideoPlayer
{
    public class DownloadSubtitleCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            Locator.VideoPlayerVm.IsLoadingSubtitle = true;
            Locator.VideoPlayerVm.LoadingSubtitleText = Strings.Loading;

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
