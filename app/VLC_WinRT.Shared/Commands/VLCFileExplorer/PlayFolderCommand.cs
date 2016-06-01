using System.Linq;
using System.Collections.ObjectModel;
using Windows.Storage;
using System.Collections.Generic;
using VLC_WinRT.Model;
using VLC_WinRT.Helpers.MusicLibrary;
using VLC_WinRT.Model.Video;
using VLC_WinRT.Utils;
using VLC_WinRT.Helpers;
using VLC_WinRT.Model.FileExplorer;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Model.Stream;

namespace VLC_WinRT.Commands.VLCFileExplorer
{
    public class PlayFolderCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            var items = (ObservableCollection<IVLCStorageItem>)parameter;
            var files = new List<VLCStorageFile>();
            foreach (var item in items)
            {
                var file = item as VLCStorageFile;
                if (file == null)
                    continue;
                
                if (file.Media != null || VLCFileExtensions.Supported.Contains((file.StorageItem as StorageFile)?.FileType.ToLower()))
                    files.Add((VLCStorageFile)item);
            }
            var playlist = new SmartCollection<IMediaItem>();
            foreach (var file in files)
            {
                if (VLCFileExtensions.AudioExtensions.Contains((file.StorageItem as StorageFile)?.FileType.ToLower()))
                {
                    var trackItem = await Locator.MediaLibrary.GetTrackItemFromFile(file.StorageItem as StorageFile);
                    playlist.Add(trackItem);
                }
                else if (VLCFileExtensions.VideoExtensions.Contains((file.StorageItem as StorageFile)?.FileType.ToLower()))
                {
                    var video = await MediaLibraryHelper.GetVideoItem(file.StorageItem as StorageFile);
                    playlist.Add(video);
                }
                else
                {
                    var stream = await MediaLibraryHelper.GetStreamItem(file);
                    playlist.Add(stream);
                }

            }
            await Locator.MediaPlaybackViewModel.PlaybackService.SetPlaylist(playlist, true, true, playlist[0]);
        }
    }
}
