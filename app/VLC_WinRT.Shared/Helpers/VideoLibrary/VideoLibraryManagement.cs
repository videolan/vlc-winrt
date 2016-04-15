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
using VLC_WinRT.Model;
using VLC_WinRT.Model.Video;
using VLC_WinRT.ViewModels;
using WinRTXamlToolkit.IO.Extensions;
using VLC_WinRT.Database;
using VLC_WinRT.Services.Interface;
using VLC_WinRT.Utils;
using VLC_WinRT.Model.Music;
using Windows.UI.Xaml;
using System.Linq.Expressions;

namespace VLC_WinRT.Helpers.VideoLibrary
{
    public class VideoLibrary
    {
        #region properties
        private bool _alreadyIndexedOnce;

        IThumbnailService ThumbsService => App.Container.Resolve<IThumbnailService>();
        #endregion
        #region databases
        readonly VideoRepository videoDatabase = new VideoRepository();
        #endregion
        #region collections
        public SmartCollection<VideoItem> Videos { get; private set; }
        public SmartCollection<VideoItem> ViewedVideos { get; private set; }
        public SmartCollection<VideoItem> CameraRoll { get; private set; }
        public SmartCollection<TvShow> Shows { get; private set; }
        #endregion
        #region mutexes
        static readonly SemaphoreSlim VideoItemDiscovererSemaphoreSlim = new SemaphoreSlim(1);
        static readonly SemaphoreSlim VideoThumbnailFetcherSemaphoreSlim = new SemaphoreSlim(1);

        async Task FetchVideoThumbnailOrWaitAsync(VideoItem videoVm)
        {
            await VideoThumbnailFetcherSemaphoreSlim.WaitAsync();
            try
            {
                var isChanged = await GenerateThumbnail(videoVm);
                if (isChanged)
                    await videoDatabase.Update(videoVm);
            }
            finally
            {
                VideoThumbnailFetcherSemaphoreSlim.Release();
            }
        }

        async Task DiscoverVideoItemOrWaitAsync(StorageFile storageItem, bool cameraRoll = false)
        {
            await VideoItemDiscovererSemaphoreSlim.WaitAsync();
            try
            {
                await CreateDatabaseFromVideoFile(storageItem, cameraRoll);
            }
            catch (Exception e)
            {
                LogHelper.Log(StringsHelper.ExceptionToString(e));
                VideoItemDiscovererSemaphoreSlim.Release();
            }
            finally
            {
                VideoItemDiscovererSemaphoreSlim.Release();
            }
        }
        #endregion

        #region Load Collections from DB
        public void DropTablesIfNeeded()
        {
            if (!Numbers.NeedsToDrop()) return;
            videoDatabase.Drop();
            videoDatabase.Initialize();
        }

        public Task PerformRoutineCheckIfNotBusy()
        {
            return PerformVideoLibraryIndexing();
        }

        public async Task Initialize()
        {
            Videos = new SmartCollection<VideoItem>();
            ViewedVideos = new SmartCollection<VideoItem>();
            CameraRoll = new SmartCollection<VideoItem>();
            Shows = new SmartCollection<TvShow>();

            if (_alreadyIndexedOnce) return;
            _alreadyIndexedOnce = true;

            if (await IsVideoDatabaseEmpty())
            {
                await StartIndexing();
            }
            else
            {
                await PerformVideoLibraryIndexing();
            }
        }

        Task<bool> IsVideoDatabaseEmpty()
        {
            return videoDatabase.IsEmpty();
        }

        async Task StartIndexing()
        {
            videoDatabase.DeleteAll();

            Videos?.Clear();
            ViewedVideos?.Clear();
            CameraRoll?.Clear();
            Shows?.Clear();
            await PerformVideoLibraryIndexing();
        }

        public async Task LoadVideosFromDatabase()
        {
            try
            {
                Videos?.Clear();
                LogHelper.Log("Loading videos from VideoDB ...");
                var videos = await LoadVideos(x => x.IsCameraRoll == false && x.IsTvShow == false);
                LogHelper.Log($"Found {videos.Count} artists from VideoDB");
                Videos.AddRange(videos.OrderBy(x => x.Name).ToObservable());
            }
            catch { }
        }

        public async Task LoadViewedVideosFromDatabase()
        {
            ViewedVideos?.Clear();

            var result = await videoDatabase.GetLastViewed();
            var orderedResults = result.OrderByDescending(x => x.LastWatched).Take(6);
            foreach (VideoItem videoVm in orderedResults)
            {
                try
                {
                    if (videoVm.Path != null)
                    {
                        StorageFile file = await StorageFile.GetFileFromPathAsync(videoVm.Path);
                        videoVm.File = file;
                    }
                    ViewedVideos.Add(videoVm);
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

        public async Task LoadShowsFromDatabase()
        {
            Shows?.Clear();
            var shows = await LoadVideos(x => x.IsTvShow);
            foreach (var item in shows)
            {
                await AddTvShow(item);
            }
        }
        public async Task LoadCameraRollFromDatabase()
        {
            CameraRoll?.Clear();
            var camVideos = await LoadVideos(x => x.IsCameraRoll);
            CameraRoll.AddRange(camVideos.OrderBy(x => x.Name).ToObservable());
        }
        #endregion
        #region Video Library Indexation Logic
        public async Task PerformVideoLibraryIndexing()
        {
#if WINDOWS_PHONE_APP
            StorageFolder storageFolder = KnownFolders.VideosLibrary;
#endif
            var list = await MediaLibraryHelper.GetSupportedFiles(KnownFolders.VideosLibrary);
            foreach (var item in list)
            {
                await DiscoverVideoItemOrWaitAsync(item);
            }

            try
            {
                if (await KnownFolders.PicturesLibrary.ContainsFolderAsync("Camera Roll"))
                {
                    var cameraRoll = await KnownFolders.PicturesLibrary.GetFolderAsync("Camera Roll");
                    list = await MediaLibraryHelper.GetSupportedFiles(cameraRoll);
                    foreach (var item in list)
                    {
                        await DiscoverVideoItemOrWaitAsync(item, true);
                    }
                }
            }
            catch (FileNotFoundException fileNotFoundException)
            {
                LogHelper.Log("Failed to get videos from Camera Roll. Aborting. " + fileNotFoundException.ToString());
            }
        }

        async Task CreateDatabaseFromVideoFile(StorageFile storageFile, bool isCameraRoll)
        {
            try
            {
                // Check if we know the file:
                //FIXME: We need to check if the files in DB still exist on disk
                var mediaVM = await videoDatabase.GetFromPath(storageFile.Path).ConfigureAwait(false);
                if (mediaVM != null)
                {
                    mediaVM.File = storageFile;
                    if (mediaVM.IsTvShow)
                    {
                        await AddTvShow(mediaVM);
                    }
                }
                else
                {
                    MediaProperties videoProperties = null;
                    if (!isCameraRoll)
                    {
                        videoProperties = TitleDecrapifier.tvShowEpisodeInfoFromString(storageFile.DisplayName);
                        if (videoProperties == null)
                        {
                            var media = Locator.VLCService.GetMediaFromPath(storageFile.Path);
                            videoProperties = Locator.VLCService.GetVideoProperties(media);
                        }
                    }

                    bool isTvShow = !string.IsNullOrEmpty(videoProperties?.ShowTitle) && videoProperties?.Season >= 0 && videoProperties?.Episode >= 0;
                    // Analyse to see if it's a tv show
                    // if the file is from a tv show, we push it to this tvshow item
                    mediaVM = !isTvShow ? new VideoItem() : new VideoItem(videoProperties.ShowTitle, videoProperties.Season, videoProperties.Episode);
                    await mediaVM.Initialize(storageFile);
                    mediaVM.IsCameraRoll = isCameraRoll;
                    if (string.IsNullOrEmpty(mediaVM.Name))
                        return;
                    VideoItem searchVideo = ViewedVideos.FirstOrDefault(x => x.Name == mediaVM.Name);
                    if (searchVideo != null)
                    {
                        mediaVM.TimeWatchedSeconds = searchVideo.TimeWatched.Seconds;
                    }

                    if (isTvShow)
                    {
                        await AddTvShow(mediaVM);
                    }
                    await videoDatabase.Insert(mediaVM);
                }

                if (mediaVM.IsCameraRoll)
                {
                    // TODO: Find a more efficient way to know if it's already in the list or not
                    if (CameraRoll.FirstOrDefault(x => x.Id == mediaVM.Id) == null)
                    {
                        CameraRoll.Add(mediaVM);
                    }
                }
                else if (!mediaVM.IsTvShow)
                {
                    if (Videos.FirstOrDefault(x => x.Id == mediaVM.Id) == null)
                    {
                        Videos.Add(mediaVM);
                    }
                }
                if (ViewedVideos.Count < 6 &&
                    ViewedVideos.FirstOrDefault(x => x.Path == mediaVM.Path && x.TimeWatched == TimeSpan.Zero) == null)
                {
                    if (ViewedVideos.FirstOrDefault(x => x.Id == mediaVM.Id) == null)
                    {
                        ViewedVideos.Add(mediaVM);
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Log(StringsHelper.ExceptionToString(e));
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
                LogHelper.Log(StringsHelper.ExceptionToString(e));
            }
        }

        private async Task<List<StorageFile>> GetMediaFromFolder(StorageFolder folder)
        {
#if WINDOWS_PHONE_APP
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
#else
            var queryOptions = new QueryOptions { FolderDepth = FolderDepth.Deep };
            foreach (var type in VLCFileExtensions.VideoExtensions)
                queryOptions.FileTypeFilter.Add(type);
            var fileQueryResult = KnownFolders.VideosLibrary.CreateFileQueryWithOptions(queryOptions);
            var files = await fileQueryResult.GetFilesAsync();
            return files.ToList();
#endif
        }

        public async Task AddTvShow(VideoItem episode)
        {
            episode.IsTvShow = true;
            try
            {
                TvShow show = Shows.FirstOrDefault(x => x.ShowTitle == episode.ShowTitle);
                if (show == null)
                {
                    show = new TvShow(episode.ShowTitle);
                    show.Episodes.Add(episode);
                    Shows.Add(show);
                }
                else
                {
                    if (show.Episodes.FirstOrDefault(x => x.Id == episode.Id) == null)
                        await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () => show.Episodes.Add(episode));
                }
            }
            catch (Exception e)
            {
                LogHelper.Log(StringsHelper.ExceptionToString(e));
            }
        }

        #endregion

        // Returns false is no snapshot generation was required, true otherwise
        public async Task<Boolean> GenerateThumbnail(VideoItem videoItem)
        {
            if (videoItem.HasThumbnail)
                return false;
            try
            {
                if (Locator.MusicLibrary.ContinueIndexing != null)
                {
                    await Locator.MusicLibrary.ContinueIndexing.Task;
                    Locator.MusicLibrary.ContinueIndexing = null;
                }
#if WINDOWS_PHONE_APP
                if (MemoryUsageHelper.PercentMemoryUsed() > MemoryUsageHelper.MaxRamForResourceIntensiveTasks)
                    return false;
#endif
                WriteableBitmap image = null;
                StorageItemThumbnail thumb = null;
                // If file is a mkv, we save the thumbnail in a VideoPic folder so we don't consume CPU and resources each launch
                if (VLCFileExtensions.MFSupported.Contains(videoItem.File?.FileType.ToLower()))
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
                    await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () => videoItem.Duration = TimeSpan.FromMilliseconds(res.Length()));
                }
                if (thumb != null || image != null)
                {
                    // RunAsync won't await on the lambda it receives, so we need to do it ourselves
                    var tcs = new TaskCompletionSource<bool>();
                    await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, async () =>
                    {
                        if (thumb != null)
                        {
                            image = new WriteableBitmap((int)thumb.OriginalWidth, (int)thumb.OriginalHeight);
                            await image.SetSourceAsync(thumb);
                        }
                        await DownloadAndSaveHelper.WriteableBitmapToStorageFile(image, videoItem.Id.ToString());
                        videoItem.ThumbnailPath = String.Format("{0}{1}.jpg", Strings.VideoPicFolderPath, videoItem.Id);
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

        #region database operations
        public Task<List<VideoItem>> LoadVideos(Expression<Func<VideoItem, bool>> predicate)
        {
            return videoDatabase.Load(predicate);
        }

        public Task UpdateVideo(VideoItem video)
        {
            return videoDatabase.Update(video);
        }

        public Task<List<VideoItem>> ContainsVideo(string column, string val)
        {
            return videoDatabase.Contains(column, val);
        }
        #endregion
    }
}