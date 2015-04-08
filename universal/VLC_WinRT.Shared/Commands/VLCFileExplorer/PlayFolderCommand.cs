using System.Linq;
using System.Collections.ObjectModel;
using Windows.Storage;
using System.Collections.Generic;
using VLC_WinRT.Model;
using VLC_WinRT.Helpers.MusicLibrary;
using VLC_WinRT.Helpers.MusicPlayer;
using VLC_WinRT.Model.Video;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Commands.VLCFileExplorer
{
    public class PlayFolderCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            var items = (ObservableCollection<IStorageItem>)parameter;
            var files = new List<StorageFile>();
            foreach (var item in items)
            {
                var file = item as StorageFile;
                if (file != null && VLCFileExtensions.Supported.Contains(file.FileType.ToLower()))
                    files.Add((StorageFile)item);
            }
            var playlist = new ObservableCollection<IVLCMedia>();
            foreach (var file in files)
            {
                if(VLCFileExtensions.AudioExtensions.Contains(file.FileType.ToLower()))
                {
                    var trackItem = await MusicLibraryManagement.GetTrackItemFromFile(file);
                    playlist.Add(trackItem);
                }
                else if(VLCFileExtensions.VideoExtensions.Contains(file.FileType.ToLower()))
                {
                    var videoVm = new VideoItem();
                    await videoVm.Initialize(file);
                    playlist.Add(videoVm);
                }
            }
            await PlayMusicHelper.AddTrackCollectionToPlaylistAndPlay(playlist, true, 0);
        }
    }
}
