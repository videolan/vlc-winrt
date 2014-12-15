using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Core;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Model.Video;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.VideoVM;
using WinRTXamlToolkit.IO.Extensions;

namespace VLC_WINRT_APP.Helpers.VideoLibrary
{
    public static class VideoLibraryManagement
    {
        public static async Task GetViewedVideos()
        {
            var result = await VideoLibraryVM._lastVideosRepository.Load();

            foreach (VideoItem videoVm in result)
            {
                try
                {
                    StorageFile file = await StorageFile.GetFileFromPathAsync(videoVm.FilePath);
                    videoVm.File = file;
                    await videoVm.GenerateThumbnail();
                    await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                    {
                        Locator.VideoLibraryVM.ViewedVideos.Add(videoVm);
                    });
                }
                catch (Exception)
                {
                    // If the video file was deleted, we can't add it to the last viewed files.
                    // We "should" keep the file in the list, and if the user selects it either tell them that the file
                    // is now gone (and let them try and find it again, in case they moved it, so we can keep it in the DB)
                    // but that will require quite a bit of code work to make happen. So for now, we'll catch the error
                    // and not add it to the list.
                    Debug.WriteLine("File not found");
                }
            }
        }

        public static async Task GetVideos()
        {
            var resourceLoader = new ResourceLoader();
#if WINDOWS_APP
            StorageLibrary videoLibrary = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Videos);
            foreach (StorageFolder storageFolder in videoLibrary.Folders)
#else
            StorageFolder storageFolder = KnownFolders.VideosLibrary;
#endif
            {
                try
                {
                    IReadOnlyList<StorageFile> files = await GetMediaFromFolder(storageFolder);

                    foreach (StorageFile storageFile in files)
                    {
                        // Analyse to see if it's a tv show
                        // if the file is from a tv show, we push it to this tvshow item
                        Dictionary<string, string> showInfoDictionary = TitleDecrapifier.tvShowEpisodeInfoFromString(storageFile.DisplayName);
                        bool isTvShow = showInfoDictionary != null && showInfoDictionary.Count > 0;

                        VideoItem mediaVM = !isTvShow ? new VideoItem() : new VideoItem(showInfoDictionary["season"], showInfoDictionary["episode"]);
                        await mediaVM.Initialize(storageFile);
                        if (string.IsNullOrEmpty(mediaVM.Title))
                            continue;
                        VideoItem searchVideo = Locator.VideoLibraryVM.ViewedVideos.FirstOrDefault(x => x.Title == mediaVM.Title);
                        if (searchVideo != null)
                        {
                            mediaVM.TimeWatched = searchVideo.TimeWatched;
                        }

                        if (isTvShow)
                        {
#if WINDOWS_APP
                            if (Locator.VideoLibraryVM.Panels.Count == 1)
                            {
                                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                    Locator.VideoLibraryVM.Panels.Add(new Panel(resourceLoader.GetString("Shows"), 1, 0.4, null)));
                            }
#endif
                            TvShow show = Locator.VideoLibraryVM.Shows.FirstOrDefault(x => x.ShowTitle == showInfoDictionary["tvShowName"]);
                            if (show == null)
                            {
                                show = new TvShow(showInfoDictionary["tvShowName"]);
                                await DispatchHelper.InvokeAsync(() =>
                                {
                                    show.Episodes.Add(mediaVM as VideoItem);
                                    Locator.VideoLibraryVM.Shows.Add(show);
                                    if (Locator.VideoLibraryVM.Shows.Count == 1)
                                        Locator.VideoLibraryVM.CurrentShow = Locator.VideoLibraryVM.Shows[0];
                                });
                            }
                            else
                            {
                                await DispatchHelper.InvokeAsync(() =>
                                show.Episodes.Add(mediaVM as VideoItem));
                            }
                        }
                        // Get back to UI thread
                        await DispatchHelper.InvokeAsync(() =>
                        {
                            if (!isTvShow)
                            {
                                Locator.VideoLibraryVM.Videos.Add(mediaVM);
                            }
                            //#if WINDOWS_APP
                            if (Locator.VideoLibraryVM.ViewedVideos.Count < 6 && Locator.VideoLibraryVM.ViewedVideos.FirstOrDefault(x => x.FilePath == mediaVM.FilePath && x.TimeWatched == TimeSpan.Zero) == null)
                                Locator.VideoLibraryVM.ViewedVideos.Add(mediaVM);
                            //#endif
                        });
                    }
                }
                catch
                {
                    Debug.WriteLine("An error occured while indexing a video folder");
                }
            }
            if (Locator.VideoLibraryVM.Videos.Count > 0)
            {
                await DispatchHelper.InvokeAsync(() => Locator.VideoLibraryVM.HasNoMedia = false);
            }
            else
            {
                await DispatchHelper.InvokeAsync(() => Locator.VideoLibraryVM.HasNoMedia = true);
            }
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {

                //if (NewVideos.Any())
                //{
#if WINDOWS_APP
                //Panels.Add(new Panel("new", 1, 0.4, App.Current.Resources["HomePath"].ToString()));
#endif
                //}
                Locator.VideoLibraryVM.LoadingState = LoadingState.Loaded;
            });

        }

        public static async Task GetVideosFromCameraRoll()
        {
            try
            {
                if (await KnownFolders.PicturesLibrary.ContainsFolderAsync("Camera Roll"))
                {
                    var cameraRoll = await KnownFolders.PicturesLibrary.GetFolderAsync("Camera Roll");
                    var videos = await GetMediaFromFolder(cameraRoll);
                    foreach (var storageFile in videos)
                    {
                        var mediaVM = new VideoItem();
                        await mediaVM.Initialize(storageFile);
                        if (string.IsNullOrEmpty(mediaVM.Title))
                            continue;
                        await DispatchHelper.InvokeAsync(() =>
                        {
                            Locator.VideoLibraryVM.CameraRoll.Add(mediaVM);
                        });
                    }
                }
            }
            catch (FileNotFoundException fileNotFoundException)
            {
                LogHelper.Log("Failed to get videos from Camera Roll. Aborting.");
            }
        }

        private static async Task GetFilesFromSubFolders(StorageFolder folder, List<StorageFile> files)
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

        private static async Task<List<StorageFile>> GetMediaFromFolder(StorageFolder folder)
        {
            var videoFiles = new List<StorageFile>();
            try
            {
                await GetFilesFromSubFolders(folder, videoFiles);
                return videoFiles;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("exception listing files");
                Debug.WriteLine(ex.ToString());
            }
            return null;
        }

        public static async Task GenerateAllThumbnails()
        {
#if WINDOWS_PHONE_APP
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                StatusBarHelper.UpdateTitle("Getting video thumbnails ...");
            });
#endif
            await ThreadPool.RunAsync(async (work) =>
            {
                foreach (var videoItem in Locator.VideoLibraryVM.Videos)
                {
                    await videoItem.GenerateThumbnail();
                }
                foreach (var tvShow in Locator.VideoLibraryVM.Shows)
                {
                    foreach (var videoItem in tvShow.Episodes)
                    {
                        await videoItem.GenerateThumbnail();
                    }
                }
                foreach (var videoItem in Locator.VideoLibraryVM.CameraRoll)
                {
                    await videoItem.GenerateThumbnail();
                }
            });
#if WINDOWS_PHONE_APP
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (App.ApplicationFrame != null)
                    StatusBarHelper.SetDefaultForPage(App.ApplicationFrame.SourcePageType);
            });
#endif
        }
    }
}