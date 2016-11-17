using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
using Autofac;
using VLC.Model;
using VLC.Model.Video;
using VLC.ViewModels;
using WinRTXamlToolkit.IO.Extensions;
using VLC.Database;
using VLC.Utils;
using VLC.Model.Music;
using Windows.UI.Xaml;
using System.Linq.Expressions;

namespace VLC.Helpers.VideoLibrary
{
    public class VideoLibrary
    {
        #region Video Library Indexation Logic

        private static async Task GetFilesFromSubFolders(StorageFolder folder, List<StorageFile> files)
        {
            try
            {
                var items = await folder.GetItemsAsync();
                foreach (IStorageItem storageItem in items)
                {
                    if (storageItem.IsOfType(StorageItemTypes.Folder))
                        await GetFilesFromSubFolders(storageItem as StorageFolder, files);
                    else if (storageItem.IsOfType(StorageItemTypes.File))
                    {
                        var file = storageItem as StorageFile;
                        if (VLCFileExtensions.VideoExtensions.Contains(file.FileType.ToLower()))
                        {
                            files.Add(file);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Log(StringsHelper.ExceptionToString(e));
            }
        }

        private async Task<List<StorageFile>> GetMediaFromFolder(StorageFolder folder)
        {
            var queryOptions = new QueryOptions { FolderDepth = FolderDepth.Deep };
            foreach (var type in VLCFileExtensions.VideoExtensions)
                queryOptions.FileTypeFilter.Add(type);
            var fileQueryResult = KnownFolders.VideosLibrary.CreateFileQueryWithOptions(queryOptions);
            var files = await fileQueryResult.GetFilesAsync();
            return files.ToList();
        }


        #endregion

    }
}