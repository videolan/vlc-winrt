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
                if (file != null && VLCFileExtensions.Supported.Contains((file.StorageItem as StorageFile).FileType.ToLower()))
                    files.Add((VLCStorageFile)item);
            }
            var playlist = new ObservableCollection<IVLCMedia>();
            foreach (var file in files)
            {
                if(VLCFileExtensions.AudioExtensions.Contains((file.StorageItem as StorageFile).FileType.ToLower()))
                {
                    var trackItem = await Locator.MusicLibraryVM.MusicLibrary.GetTrackItemFromFile(file.StorageItem as StorageFile);
                    playlist.Add(trackItem);
                }
                else if(VLCFileExtensions.VideoExtensions.Contains((file.StorageItem as StorageFile).FileType.ToLower()))
                {
                    var videoVm = new VideoItem();
                    await videoVm.Initialize((file.StorageItem as StorageFile));
                    playlist.Add(videoVm);
                }
            }
            await PlaylistHelper.AddTrackCollectionToPlaylistAndPlay(playlist, true, 0);
        }
    }
}
