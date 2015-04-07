using System.Collections.ObjectModel;
using VLC_WINRT.Common;
using Windows.Storage;
using System.Collections.Generic;
using VLC_WinRT.Model;
using VLC_WinRT.Helpers.MusicLibrary;
using VLC_WinRT.Model.Music;
using VLC_WinRT.Helpers.MusicPlayer;

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
            var playlist = new ObservableCollection<TrackItem>();
            foreach (var file in files)
            {
                var trackItem = await MusicLibraryManagement.GetTrackItemFromFile(file);
                playlist.Add(trackItem);
            }
            await PlayMusicHelper.AddTrackCollectionToPlaylistAndPlay(playlist, true, 0);
        }
    }
}
