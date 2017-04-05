using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VLC.Model;
using VLC.Model.Music;
using VLC.Utils;
using VLC.ViewModels;
using Windows.Storage;

namespace VLC.Commands.MediaLibrary
{
    public class DeleteFromLibraryCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            if (parameter is IMediaItem)
            {
                var media = parameter as IMediaItem;
                
                // delete the file
                try
                {
                    var fileToDelete = media.File ?? await StorageFile.GetFileFromPathAsync(media.Path);
                    await fileToDelete.DeleteAsync();
                }
                catch (FileNotFoundException)
                {
                    // it is already deleted
                }

                // remove MediaLibrary entries
                await Locator.MediaLibrary.RemoveMediaFromCollectionAndDatabase(parameter as IMediaItem);
            }
            else if (parameter is AlbumItem)
            {
                var album = parameter as AlbumItem;
                var tracks = album.Tracks;
                foreach (var t in tracks)
                {
                    var fileToDelete = t.File ?? await StorageFile.GetFileFromPathAsync(t.Path);
                    await fileToDelete.DeleteAsync();
                    await Locator.MediaLibrary.RemoveMediaFromCollectionAndDatabase(t);
                }

            }
        }
    }
}
