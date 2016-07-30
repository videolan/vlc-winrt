using Windows.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Core;
using Autofac;
using VLC_WinRT.Model;
using VLC_WinRT.Model.Music;
using VLC_WinRT.Services.RunTime;
using VLC_WinRT.ViewModels;
using VLC_WinRT.ViewModels.MusicVM;
using WinRTXamlToolkit.IO.Extensions;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using VLC_WinRT.Utils;
using VLC_WinRT.Views.MusicPages.PlaylistControls;
using WinRTXamlToolkit.Controls.Extensions;
using System.Collections.Generic;
using libVLCX;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Windows.Foundation;
using VLC_WinRT.Database;

namespace VLC_WinRT.Helpers.MusicLibrary
{
    public class MusicLibrary
    {
        async Task GetAllMusicFolders()
        {
            try
            {
                StorageFolder musicLibrary = KnownFolders.MusicLibrary;
                LogHelper.Log("Searching for music from Phone MusicLibrary ...");
                await CreateDatabaseFromMusicFolder(musicLibrary);
            }
            catch (Exception e)
            {
                LogHelper.Log(StringsHelper.ExceptionToString(e));
            }
        }

        async Task CreateDatabaseFromMusicFolder(StorageFolder musicFolder)
        {
            //try
            //{
            //    if (Locator.MediaLibrary.ContinueIndexing != null) // We prevent indexing new folder and files recursively when a Video is playing
            //    {
            //        await Locator.MediaLibrary.ContinueIndexing.Task;
            //        Locator.MediaLibrary.ContinueIndexing = null;
            //    }
            //    if (musicFolder.Name != Strings.PodcastFolderName)
            //    {
            //        var folders = await musicFolder.GetFoldersAsync();
            //        if (folders.Any())
            //        {
            //            foreach (var storageFolder in folders)
            //            {
            //                await CreateDatabaseFromMusicFolder(storageFolder);
            //            }
            //        }
            //        var folderFiles = await musicFolder.GetFilesAsync();
            //        if (folderFiles != null && folderFiles.Any())
            //        {
            //            foreach (var storageFile in folderFiles)
            //            {
            //                await Locator.MediaLibrary.ParseMediaOrWaitAsync(storageFile);
            //            }
            //        }
            //    }
            //}
            //catch (Exception e)
            //{
            //    LogHelper.Log(StringsHelper.ExceptionToString(e));
            //}
        }
    }
}
