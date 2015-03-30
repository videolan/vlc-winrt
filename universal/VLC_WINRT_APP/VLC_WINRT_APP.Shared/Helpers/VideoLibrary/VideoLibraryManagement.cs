using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Autofac;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Model.Video;
using VLC_WINRT_APP.ViewModels;
using WinRTXamlToolkit.IO.Extensions;
using VLC_WINRT_APP.DataRepository;
using VLC_WINRT_APP.Services.Interface;

namespace VLC_WINRT_APP.Helpers.VideoLibrary
{
    public static class VideoLibraryManagement
    {
        static readonly IThumbnailService ThumbsService = App.Container.Resolve<IThumbnailService>();
        static readonly SemaphoreSlim VideoThumbnailFetcherSemaphoreSlim = new SemaphoreSlim(1);

        public static async Task FetchVideoThumbnailOrWaitAsync(VideoItem videoVm)
        {
            await VideoThumbnailFetcherSemaphoreSlim.WaitAsync();
            try
            {
                var isChanged = await VideoLibraryManagement.GenerateThumbnail(videoVm);
                if (isChanged)
                    await Locator.VideoLibraryVM.VideoRepository.Update(videoVm);
            }
            finally
            {
                VideoThumbnailFetcherSemaphoreSlim.Release();
            }
        }

        public static async Task GetViewedVideos()
        {
            var result = await Locator.VideoLibraryVM.VideoRepository.GetLastViewed(6).ConfigureAwait(false);

            foreach (VideoItem videoVm in result)
            {
                try
                {
                    if (videoVm.Path != null)
                    {
                        StorageFile file = await StorageFile.GetFileFromPathAsync(videoVm.Path);
                        videoVm.File = file;
                    }
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
                    LogHelper.Log("File not found");
                }
            }
        }

        private static async Task AddVideo(VideoRepository videoRepo, IReadOnlyList<StorageFile> files, bool isCameraRoll)
        {
            try
            {
                foreach (StorageFile storageFile in files)
                {
                    Dictionary<string, string> showInfoDictionary = isCameraRoll
                        ? null
                        : TitleDecrapifier.tvShowEpisodeInfoFromString(storageFile.DisplayName);
                    bool isTvShow = showInfoDictionary != null && showInfoDictionary.Count > 0;

                    // Check if we know the file:
                    //FIXME: We need to check if the files in DB still exist on disk
                    var mediaVM = await videoRepo.GetFromPath(storageFile.Path).ConfigureAwait(false);
                    if (mediaVM != null)
                    {
                        Debug.Assert(isTvShow == mediaVM.IsTvShow);
                        mediaVM.File = storageFile;
                        if (isTvShow)
                            await AddTvShow(showInfoDictionary["tvShowName"], mediaVM);
                    }
                    else
                    {
                        // Analyse to see if it's a tv show
                        // if the file is from a tv show, we push it to this tvshow item

                        mediaVM = !isTvShow
                            ? new VideoItem()
                            : new VideoItem(showInfoDictionary["season"], showInfoDictionary["episode"]);
                        await mediaVM.Initialize(storageFile);
                        mediaVM.IsCameraRoll = isCameraRoll;
                        if (string.IsNullOrEmpty(mediaVM.Name))
                            continue;
                        VideoItem searchVideo =
                            Locator.VideoLibraryVM.ViewedVideos.FirstOrDefault(x => x.Name == mediaVM.Name);
                        if (searchVideo != null)
                        {
                            mediaVM.TimeWatched = searchVideo.TimeWatched;
                        }

                        if (isTvShow)
                            await AddTvShow(showInfoDictionary["tvShowName"], mediaVM);
                        await videoRepo.Insert(mediaVM);
                    }
                    // Get back to UI thread
                    await DispatchHelper.InvokeAsync(() =>
                    {
                        if (mediaVM.IsCameraRoll)
                        {
                            // TODO: Find a more efficient way to know if it's already in the list or not
                            if (Locator.VideoLibraryVM.CameraRoll.FirstOrDefault(x => x.Id == mediaVM.Id) == null)
                                Locator.VideoLibraryVM.CameraRoll.Add(mediaVM);
                        }
                        else if (!mediaVM.IsTvShow)
                        {
                            if (Locator.VideoLibraryVM.Videos.FirstOrDefault(x => x.Id == mediaVM.Id) == null)
                                Locator.VideoLibraryVM.Videos.Add(mediaVM);
                        }
                        if (Locator.VideoLibraryVM.ViewedVideos.Count < 6 &&
                            Locator.VideoLibraryVM.ViewedVideos.FirstOrDefault(
                                x => x.Path == mediaVM.Path && x.TimeWatched == TimeSpan.Zero) == null)
                        {
                            if (Locator.VideoLibraryVM.ViewedVideos.FirstOrDefault(x => x.Id == mediaVM.Id) == null)
                                Locator.VideoLibraryVM.ViewedVideos.Add(mediaVM);
                        }
                    });
                }
            }
            catch (Exception e)
            {
                ExceptionHelper.CreateMemorizedException("VideoLibraryManagement.AddVideo", e);
            }
        }

        public static async Task GetVideos(VideoRepository videoRepo)
        {
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
                    await AddVideo(videoRepo, files, false);
                }
                catch
                {
                    LogHelper.Log("An error occured while indexing a video folder");
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
                Locator.VideoLibraryVM.LoadingState = LoadingState.Loaded;
            });
        }

        public static async Task GetVideosFromCameraRoll(VideoRepository videoRepo)
        {
            try
            {
                if (await KnownFolders.PicturesLibrary.ContainsFolderAsync("Camera Roll"))
                {
                    var cameraRoll = await KnownFolders.PicturesLibrary.GetFolderAsync("Camera Roll");
                    var videos = await GetMediaFromFolder(cameraRoll);
                    await AddVideo(videoRepo, videos, true);
                }
            }
            catch (FileNotFoundException fileNotFoundException)
            {
                LogHelper.Log("Failed to get videos from Camera Roll. Aborting. " + fileNotFoundException.ToString());
            }
        }

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
                ExceptionHelper.CreateMemorizedException("VideoLibraryManagement.GetFilesFromSubFolders", e);
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
                LogHelper.Log("exception listing files");
                LogHelper.Log(ex.ToString());
            }
            return null;
        }


        public static async Task AddTvShow(String name, VideoItem episode)
        {
            try
            {
                TvShow show = Locator.VideoLibraryVM.Shows.FirstOrDefault(x => x.ShowTitle == name);
                if (show == null)
                {
                    show = new TvShow(name);
                    await DispatchHelper.InvokeAsync(() =>
                    {
                        show.Episodes.Add(episode);
                        Locator.VideoLibraryVM.Shows.Add(show);
                        if (Locator.VideoLibraryVM.Shows.Count == 1)
                            Locator.VideoLibraryVM.CurrentShow = Locator.VideoLibraryVM.Shows[0];
                    });
                }
                else
                {
                    if (show.Episodes.FirstOrDefault(x => x.Id == episode.Id) == null)
                        await DispatchHelper.InvokeAsync(() => show.Episodes.Add(episode));
                }
            }
            catch (Exception e)
            {
                ExceptionHelper.CreateMemorizedException("VideoLibaryManagement.AddTvShow", e);
            }
        }

        // Returns false is no snapshot generation was required, true otherwise
        public static async Task<Boolean> GenerateThumbnail(VideoItem videoItem)
        {
            if (videoItem.HasThumbnail)
                return false;
            try
            {
                if (Locator.MediaPlaybackViewModel.ContinueIndexing != null)
                {
                    await Locator.MediaPlaybackViewModel.ContinueIndexing.Task;
                    Locator.MediaPlaybackViewModel.ContinueIndexing = null;
                }
#if WINDOWS_PHONE_APP
                if (MemoryUsageHelper.PercentMemoryUsed() > MemoryUsageHelper.MaxRamForResourceIntensiveTasks)
                    return false;
#endif
                WriteableBitmap image = null;
                StorageItemThumbnail thumb = null;
                // If file is a mkv, we save the thumbnail in a VideoPic folder so we don't consume CPU and resources each launch
                if (VLCFileExtensions.MFSupported.Contains(videoItem.File.FileType.ToLower()))
                {
                    thumb = await ThumbsService.GetThumbnail(videoItem.File).ConfigureAwait(false);
                }
                // If MF thumbnail generation failed or wasn't supported:
                if (thumb == null)
                {
                    var res = await ThumbsService.GetScreenshot(videoItem.File).ConfigureAwait(false);
                    if (res == null)
                        return true;
                    image = res.Bitmap();
                    await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => videoItem.Duration = TimeSpan.FromMilliseconds(res.Length()));
                }
                if (thumb != null || image != null)
                {
                    // RunAsync won't await on the lambda it receives, so we need to do it ourselves
                    var tcs = new TaskCompletionSource<bool>();
                    await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
                    {
                        if (thumb != null)
                        {
                            image = new WriteableBitmap((int)thumb.OriginalWidth, (int)thumb.OriginalHeight);
                            await image.SetSourceAsync(thumb);
                        }
                        await DownloadAndSaveHelper.WriteableBitmapToStorageFile(image, DownloadAndSaveHelper.FileFormat.Jpeg, videoItem.Id.ToString());
                        videoItem.ThumbnailPath = String.Format("ms-appdata:///local/videoPic/{0}.jpg", videoItem.Id);
                        tcs.SetResult(true);
                    });
                    await tcs.Task;
                }
                videoItem.HasThumbnail = true;
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log(ex.ToString());
            }
            return false;
        }

    }
}