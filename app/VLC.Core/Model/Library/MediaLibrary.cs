using Autofac;
using libVLCX;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using VLC.Database;
using VLC.Helpers;
using VLC.Model.Music;
using VLC.Model.Stream;
using VLC.Model.Video;
using VLC.Services.RunTime;
using VLC.Utils;
using VLC.ViewModels;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;

namespace VLC.Model.Library
{
    public class MediaLibrary
    {
        public MediaLibrary()
        {
            Locator.ExternalDeviceService.MustIndexExternalDevice += ExternalDeviceService_MustIndexExternalDevice;
            Locator.ExternalDeviceService.MustUnindexExternalDevice += ExternalDeviceService_MustUnindexExternalDevice;
        }

        private Task ExternalDeviceService_MustIndexExternalDevice(string deviceId)
        {
            return Task.Run(async () =>
            {
                await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Low, () => MediaLibraryIndexingState = LoadingState.Loading);
                StorageFolder folder;
                // Windows.Devices.Portable.StorageDevice.FromId blocks forever on Xbox... so work around
                if (Helpers.DeviceHelper.GetDeviceType() != DeviceTypeEnum.Xbox &&
                    Helpers.DeviceHelper.GetDeviceType() != DeviceTypeEnum.Phone)
                    folder = Windows.Devices.Portable.StorageDevice.FromId(deviceId);
                else
                {
                    var devices = KnownFolders.RemovableDevices;
                    var allFolders = await devices.GetFoldersAsync();
                    folder = allFolders.Last();
                }

                if (!StorageApplicationPermissions.FutureAccessList.CheckAccess(folder))
                    StorageApplicationPermissions.FutureAccessList.Add(folder);
                await MediaLibraryHelper.ForeachSupportedFile(folder, async (IReadOnlyList<StorageFile> files) => await DiscoverMediaItems(files));
                await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Low, () => MediaLibraryIndexingState = LoadingState.Loaded);
            });
        }

        private Task ExternalDeviceService_MustUnindexExternalDevice()
        {
            return Task.Run(async () => await CleanMediaLibrary().ConfigureAwait(false));
        }

        #region properties
        private object discovererLock = new object();
        private bool _alreadyIndexedOnce = false;
        public bool AlreadyIndexedOnce => _alreadyIndexedOnce;

        ThumbnailService ThumbsService => App.Container.Resolve<ThumbnailService>();
        private LoadingState _mediaLibraryIndexingState = LoadingState.NotLoaded;
        public LoadingState MediaLibraryIndexingState
        {
            get { return _mediaLibraryIndexingState; }
            private set
            {
                _mediaLibraryIndexingState = value;
                OnIndexing?.Invoke(value);
            }
        }
        public event Action<LoadingState> OnIndexing;
        #endregion

        #region databases
        readonly MusicDatabase musicDatabase = new MusicDatabase();
        readonly TracklistItemRepository tracklistItemRepository = new TracklistItemRepository();
        readonly TrackCollectionRepository trackCollectionRepository = new TrackCollectionRepository();
        
        readonly VideoRepository videoDatabase = new VideoRepository();
        
        readonly StreamsDatabase streamsDatabase = new StreamsDatabase();
        #endregion

        #region collections
        public SmartCollection<ArtistItem> Artists { get; private set; } = new SmartCollection<ArtistItem>();
        public SmartCollection<AlbumItem> Albums { get; private set; } = new SmartCollection<AlbumItem>();
        public SmartCollection<TrackItem> Tracks { get; private set; } = new SmartCollection<TrackItem>();
        public SmartCollection<PlaylistItem> TrackCollections { get; private set; } = new SmartCollection<PlaylistItem>();

        public ObservableCollection<VideoItem> Videos { get; } = new ObservableCollection<VideoItem>();
        public ObservableCollection<VideoItem> CameraRoll { get; private set; } = new ObservableCollection<VideoItem>();
        public SmartCollection<TvShow> Shows { get; private set; } = new SmartCollection<TvShow>();

        public SmartCollection<StreamMedia> Streams { get; private set; } = new SmartCollection<StreamMedia>();


        Dictionary<string, MediaDiscoverer> discoverers;
        public event MediaListItemAdded MediaListItemAdded;
        public event MediaListItemDeleted MediaListItemDeleted;
        #endregion
        #region mutexes
        public TaskCompletionSource<bool> ContinueIndexing { get; set; }
        public TaskCompletionSource<bool> MusicCollectionLoaded = new TaskCompletionSource<bool>();

        static readonly SemaphoreSlim MediaItemDiscovererSemaphoreSlim = new SemaphoreSlim(1);

        static readonly SemaphoreSlim VideoThumbnailFetcherSemaphoreSlim = new SemaphoreSlim(1);

        readonly SemaphoreSlim AlbumCoverFetcherSemaphoreSlim = new SemaphoreSlim(4);
        readonly SemaphoreSlim ArtistPicFetcherSemaphoreSlim = new SemaphoreSlim(4);

        public async Task FetchAlbumCoverOrWaitAsync(AlbumItem albumItem)
        {
            await AlbumCoverFetcherSemaphoreSlim.WaitAsync();
            try
            {
                await Locator.MusicMetaService.GetAlbumCover(albumItem);
            }
            finally
            {
                AlbumCoverFetcherSemaphoreSlim.Release();
            }
        }

        public async Task FetchArtistPicOrWaitAsync(ArtistItem artistItem)
        {
            await ArtistPicFetcherSemaphoreSlim.WaitAsync();
            try
            {
                Debug.WriteLine($"{DateTime.Now} -- loading pic : {artistItem.Name}");
                await Locator.MusicMetaService.GetArtistPicture(artistItem);
                Debug.WriteLine($"{DateTime.Now} -- loading operation DONE: {artistItem.Name}");
            }
            catch
            {
                ArtistPicFetcherSemaphoreSlim.Release();
            }
            finally
            {
                ArtistPicFetcherSemaphoreSlim.Release();
            }
        }

        public async Task<bool> DiscoverMediaItemOrWaitAsync(StorageFile storageItem, bool isCameraRoll)
        {
            await MediaItemDiscovererSemaphoreSlim.WaitAsync();
            bool success;
            try
            {
                success = await ParseMediaFile(storageItem, isCameraRoll);
            }
            catch (Exception e)
            {
                LogHelper.Log(StringsHelper.ExceptionToString(e));
                success = false;
            }
            finally
            {
                MediaItemDiscovererSemaphoreSlim.Release();
            }
            return success;
        }

        public Task AskVideoThumbnail(VideoItem videoVm)
        {
            return Task.Run(async () =>
            {
                await VideoThumbnailFetcherSemaphoreSlim.WaitAsync();
                try
                {
                    await GenerateThumbnail(videoVm);
                    if (videoVm.Type == ".mkv")
                        await Locator.VideoMetaService.GetMoviePicture(videoVm);
                }
                catch (Exception e)
                {
                    LogHelper.Log(StringsHelper.ExceptionToString(e));
                }
                finally
                {
                    VideoThumbnailFetcherSemaphoreSlim.Release();
                }
            });
        }

        #endregion

        #region IndexationLogic
        public void DropTablesIfNeeded()
        {
            if (!Numbers.NeedsToDrop()) return;
            trackCollectionRepository.Drop();
            tracklistItemRepository.Drop();
            musicDatabase.Drop();
            trackCollectionRepository.Initialize();
            tracklistItemRepository.Initialize();
            musicDatabase.Initialize();

            videoDatabase.Drop();
            videoDatabase.Initialize();
        }
        
        public Task RescanLibrary()
        {
            _alreadyIndexedOnce = false;
            return Initialize();
        }

        public async Task Initialize()
        {
            Artists.Clear();
            Albums.Clear();
            Tracks.Clear();
            TrackCollections.Clear();

            if (_alreadyIndexedOnce) return;
            _alreadyIndexedOnce = true;
            // Doing full indexing from scratch if 0 tracks are found
            if (IsMusicDatabaseEmpty() && IsVideoDatabaseEmpty())
                await clearDatabase();
            else // Restore the database
            {
                await loadVideosFromDatabase();
                await loadShowsFromDatabase();
                await loadCameraRollFromDatabase();
            }
            await PerformMediaLibraryIndexing();
        }

        private async Task clearDatabase()
        {
            musicDatabase.DeleteAll();
            trackCollectionRepository.DeleteAll();
            tracklistItemRepository.DeleteAll();

            musicDatabase.Initialize();
            trackCollectionRepository.Initialize();
            tracklistItemRepository.Initialize();

            await DispatchHelper.InvokeInUIThreadHighPriority(() =>
            {
                Artists.Clear();
                Albums.Clear();
                Tracks.Clear();
                TrackCollections.Clear();
            });

            videoDatabase.DeleteAll();

            await DispatchHelper.InvokeInUIThreadHighPriority(() =>
            {
                Videos.Clear();
                CameraRoll.Clear();
                Shows.Clear();
            });
        }

        async Task PerformMediaLibraryIndexing()
        {
            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Low, () => MediaLibraryIndexingState = LoadingState.Loading);

            StorageFolder folder = await FileUtils.GetLocalStorageMediaFolder();
            await MediaLibraryHelper.ForeachSupportedFile(folder, async (IReadOnlyList<StorageFile> files) => await DiscoverMediaItems(files));
            await MediaLibraryHelper.ForeachSupportedFile(KnownFolders.VideosLibrary, async (IReadOnlyList<StorageFile> files) => await DiscoverMediaItems(files));
            await MediaLibraryHelper.ForeachSupportedFile(KnownFolders.MusicLibrary, async (IReadOnlyList<StorageFile> files) => await DiscoverMediaItems(files));
            await MediaLibraryHelper.ForeachSupportedFile(KnownFolders.CameraRoll, async (IReadOnlyList<StorageFile> files) => await DiscoverMediaItems(files, true));

            // Cortana gets all those artists, albums, songs names
            var artists = LoadArtists(null);
            if (artists != null)
                await CortanaHelper.SetPhraseList("artistName", artists.Where(x => !string.IsNullOrEmpty(x.Name)).Select(x => x.Name).ToList());

            var albums = LoadAlbums(null);
            if (albums != null)
                await CortanaHelper.SetPhraseList("albumName", albums.Where(x => !string.IsNullOrEmpty(x.Name)).Select(x => x.Name).ToList());

            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Low, () => MediaLibraryIndexingState = LoadingState.Loaded);
        }

        async Task DiscoverMediaItems(IReadOnlyList<StorageFile> files, bool isCameraRoll = false)
        {
            foreach (var item in files)
            {
                if (ContinueIndexing != null)
                {
                    await ContinueIndexing.Task;
                    ContinueIndexing = null;
                }
                await DiscoverMediaItemOrWaitAsync(item, isCameraRoll);
            }
        }

        async Task<bool> ParseMediaFile(StorageFile item, bool isCameraRoll)
        {
            try
            {
                if (VLCFileExtensions.AudioExtensions.Contains(item.FileType.ToLower()))
                {
                    if (musicDatabase.ContainsTrack(item.Path))
                        return true;

                    // Groove Music puts its cache into this folder in Music.
                    // If the file is in this folder or subfolder, don't add it to the collection,
                    // since we can't play it anyway because of the DRM.
                    if (item.Path.Contains("Music Cache") || item.Path.Contains("Podcast"))
                        return false;

                    var media = await Locator.PlaybackService.GetMediaFromPath(item.Path);
                    var mP = await Locator.PlaybackService.GetMusicProperties(media);
                    if (mP == null || (string.IsNullOrEmpty(mP.Artist) && string.IsNullOrEmpty(mP.Album) && (string.IsNullOrEmpty(mP.Title) || mP.Title == item.Name)))
                    {
                        var props = await item.Properties.GetMusicPropertiesAsync();
                        mP = new MediaProperties()
                        {
                            Album = props.Album,
                            AlbumArtist = props.AlbumArtist,
                            Artist = props.Artist,
                            Title = props.Title,
                            Tracknumber = props.TrackNumber,
                            Genre = (props.Genre != null && props.Genre.Any()) ? props.Genre[0] : null,
                        };
                    }
                    if (mP != null)
                    {
                        var artistName = mP.Artist?.Trim();
                        var albumArtistName = mP.AlbumArtist?.Trim();
                        ArtistItem artist = LoadViaArtistName(string.IsNullOrEmpty(albumArtistName) ? artistName : albumArtistName);
                        if (artist == null)
                        {
                            artist = new ArtistItem();
                            artist.Name = string.IsNullOrEmpty(albumArtistName) ? artistName : albumArtistName;
                            artist.PlayCount = 0;
                            musicDatabase.Add(artist);
                            Artists.Add(artist);
                        }

                        var albumName = mP.Album?.Trim();
                        var albumYear = mP.Year;
                        AlbumItem album = musicDatabase.LoadAlbumFromName(artist.Id, albumName);
                        if (album == null)
                        {
                            string albumSimplifiedUrl = null;
                            if (!string.IsNullOrEmpty(mP.AlbumArt) && mP.AlbumArt.StartsWith("file://"))
                            {
                                // The Uri will be like
                                // ms-appdata:///local/vlc/art/artistalbum/30 Seconds To Mars/B-sides & Rarities/art.jpg
                                var indexStart = mP.AlbumArt.IndexOf("vlc/art/artistalbum/", StringComparison.Ordinal);
                                if (indexStart != -1)
                                {
                                    albumSimplifiedUrl = mP.AlbumArt.Substring(indexStart, mP.AlbumArt.Length - indexStart);
                                    Debug.WriteLine("VLC : found album cover with TagLib - " + albumName);
                                }
                            }

                            album = new AlbumItem
                            {
                                Name = string.IsNullOrEmpty(albumName) ? string.Empty : albumName,
                                AlbumArtist = albumArtistName,
                                Artist = string.IsNullOrEmpty(albumArtistName) ? artistName : albumArtistName,
                                ArtistId = artist.Id,
                                Favorite = false,
                                Year = albumYear,
                                AlbumCoverUri = albumSimplifiedUrl
                            };
                            musicDatabase.Add(album);
                            Albums.Add(album);
                        }

                        TrackItem track = new TrackItem
                        {
                            AlbumId = album.Id,
                            AlbumName = album.Name,
                            ArtistId = artist.Id,
                            ArtistName = artistName,
                            CurrentPosition = 0,
                            Duration = mP.Duration,
                            Favorite = false,
                            Name = string.IsNullOrEmpty(mP.Title) ? item.DisplayName : mP.Title,
                            Path = item.Path,
                            Index = mP.Tracknumber,
                            DiscNumber = mP.DiscNumber,
                            Genre = mP.Genre,
                            IsAvailable = true,
                        };
                        musicDatabase.Add(track);
                        Tracks.Add(track);
                    }
                }
                else if (VLCFileExtensions.VideoExtensions.Contains(item.FileType.ToLower()))
                {
                    if (videoDatabase.DoesMediaExist(item.Path))
                        return true;

                    var video = await MediaLibraryHelper.GetVideoItem(item);

                    // Insert first in the database to get the id.
                    videoDatabase.Insert(video);

                    // Add to collections.
                    if (video.IsTvShow)
                    {
                        await AddTvShow(video);
                    }
                    else if (isCameraRoll)
                    {
                        video.IsCameraRoll = true;
                        await DispatchHelper.InvokeInUIThreadHighPriority(() => CameraRoll.Add(video));
                    }
                    else
                    {
                        await DispatchHelper.InvokeInUIThreadHighPriority(() => Videos.Add(video));
                    }
                }
                else
                {
                    Debug.WriteLine($"{item.Path} is not a media file");
                    return false;
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return true;
        }

        // Remove items that are no longer reachable.
        public async Task CleanMediaLibrary()
        {
            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Low, () => MediaLibraryIndexingState = LoadingState.Loading);
            // Clean videos
            var videos = LoadVideos(x => true);
            foreach (var video in videos)
            {
                try
                {
                    var file = await StorageFile.GetFileFromPathAsync(video.Path);
                }
                catch
                {
                    await RemoveMediaFromCollectionAndDatabase(video);
                }
            }

            // Clean tracks
            var tracks = LoadTracks();
            foreach (var track in tracks)
            {
                try
                {
                    var file = await StorageFile.GetFileFromPathAsync(track.Path);
                }
                catch
                {
                    await RemoveMediaFromCollectionAndDatabase(track);
                }
            }
            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Low, () => MediaLibraryIndexingState = LoadingState.Loaded);
        }

        public async Task AddTvShow(VideoItem episode)
        {
            episode.IsTvShow = true;
            try
            {
                TvShow show = await DispatchHelper.InvokeInUIThread<TvShow>(CoreDispatcherPriority.High,
                    () => Shows.FirstOrDefault(x => x.ShowTitle == episode.ShowTitle));

                if (show == null)
                {
                    // Generate a thumbnail for the show
                    await Locator.MediaLibrary.AskVideoThumbnail(episode);

                    show = new TvShow(episode.ShowTitle);
                    await DispatchHelper.InvokeInUIThreadHighPriority(() => show.Episodes.Add(episode));
                    await DispatchHelper.InvokeInUIThreadHighPriority(() => Shows.Add(show));
                }
                else
                {
                    VideoItem epVideoItem = await DispatchHelper.InvokeInUIThread<VideoItem>(CoreDispatcherPriority.High,
                        () => show.Episodes.FirstOrDefault(x => x.Id == episode.Id));
                    if (epVideoItem == null)
                        await DispatchHelper.InvokeInUIThreadHighPriority(() => show.Episodes.Add(episode));
                }
            }
            catch (Exception e)
            {
                LogHelper.Log(StringsHelper.ExceptionToString(e));
            }
        }

        public async Task<bool> InitDiscoverer()
        {
            if (Locator.PlaybackService.Instance == null)
            {
                await Locator.PlaybackService.Initialize();
            }
            await Locator.PlaybackService.PlayerInstanceReady.Task;
            if (Locator.PlaybackService.Instance == null)
                return false;

            await MediaItemDiscovererSemaphoreSlim.WaitAsync();
            try
            {
                var tcs = new TaskCompletionSource<bool>();
                await Task.Run(() =>
                {
                    lock (discovererLock)
                    {
                        if (discoverers == null)
                        {
                            discoverers = new Dictionary<string, MediaDiscoverer>();
                            var discoverersDesc = Locator.PlaybackService.Instance.mediaDiscoverers(MediaDiscovererCategory.Lan);
                            foreach (var discDesc in discoverersDesc)
                            {
                                var discoverer = new MediaDiscoverer(Locator.PlaybackService.Instance, discDesc.name());

                                var mediaList = discoverer.mediaList();
                                if (mediaList == null)
                                    tcs.TrySetResult(false);

                                var eventManager = mediaList.eventManager();
                                eventManager.onItemAdded += MediaListItemAdded;
                                eventManager.onItemDeleted += MediaListItemDeleted;

                                discoverers.Add(discDesc.name(), discoverer);
                            }
                        }

                        foreach (var discoverer in discoverers)
                        {
                            if (!discoverer.Value.isRunning())
                                discoverer.Value.start();
                        }
                        tcs.TrySetResult(true);
                    }
                });
                await tcs.Task;
                return tcs.Task.Result;
            }
            finally
            {
                MediaItemDiscovererSemaphoreSlim.Release();
            }
        }

        public async Task<MediaList> DiscoverMediaList(Media media)
        {
            if (media.parsedStatus() == ParsedStatus.Done)
                return media.subItems();

            await MediaItemDiscovererSemaphoreSlim.WaitAsync();
            try
            {
                await media.parseWithOptionsAsync(ParseFlags.Local | ParseFlags.Network | ParseFlags.Interact, 0);
            }
            finally
            {
                MediaItemDiscovererSemaphoreSlim.Release();
            }
            return media.subItems();
        }
        #endregion

        //============================================
        #region DataLogic
        #region audio
        public SmartCollection<AlbumItem> OrderAlbums(OrderType orderType, OrderListing orderListing)
        {
            if (Albums == null)
                return null;

            if (orderType == OrderType.ByArtist)
            {
                if (orderListing == OrderListing.Ascending)
                {
                    return Albums.OrderBy(x => x.Artist).ToObservable();
                }
                else if (orderListing == OrderListing.Descending)
                {
                    return Albums.OrderByDescending(x => x.Artist).ToObservable();
                }
            }
            else if (orderType == OrderType.ByDate)
            {
                if (orderListing == OrderListing.Ascending)
                {
                    return Albums.OrderBy(x => x.Year).ToObservable();
                }
                else if (orderListing == OrderListing.Descending)
                {
                    return Albums.OrderByDescending(x => x.Year).ToObservable();
                }
            }
            else if (orderType == OrderType.ByAlbum)
            {
                if (orderListing == OrderListing.Ascending)
                {
                    return Albums.OrderBy(x => x.Name).ToObservable();
                }
                else if (orderListing == OrderListing.Descending)
                {
                    return Albums.OrderByDescending(x => x.Name).ToObservable();
                }
            }

            return null;
        }

        public ObservableCollection<GroupItemList<ArtistItem>> OrderArtists()
        {
            var groupedArtists = new ObservableCollection<GroupItemList<ArtistItem>>();
            var groupQuery = from artist in Artists
                             group artist by Strings.HumanizedArtistFirstLetter(artist.Name) into a
                             orderby a.Key
                             select new { GroupName = a.Key, Items = a };
            foreach (var g in groupQuery)
            {
                GroupItemList<ArtistItem> artists = new GroupItemList<ArtistItem>();
                artists.Key = g.GroupName;
                foreach (var artist in g.Items)
                {
                    artists.Add(artist);
                }
                groupedArtists.Add(artists);
            }
            return groupedArtists;
        }

        public ObservableCollection<GroupItemList<TrackItem>> OrderTracks()
        {
            var groupedTracks = new ObservableCollection<GroupItemList<TrackItem>>();
            var groupQuery = from track in Tracks
                             group track by Strings.HumanizedArtistFirstLetter(track.Name) into a
                             orderby a.Key
                             select new { GroupName = a.Key, Items = a };
            foreach (var g in groupQuery)
            {
                GroupItemList<TrackItem> tracks = new GroupItemList<TrackItem>();
                tracks.Key = g.GroupName;
                foreach (var artist in g.Items)
                {
                    tracks.Add(artist);
                }
                groupedTracks.Add(tracks);
            }
            return groupedTracks;
        }
        #endregion
        #region DatabaseLogic
        public void LoadAlbumsFromDatabase()
        {
            Albums.Clear();
            LogHelper.Log("Loading albums from MusicDB ...");
            var albums = musicDatabase.LoadAlbums().ToObservable();
            var orderedAlbums = albums.OrderBy(x => x.Artist).ThenBy(x => x.Name);
            Albums.AddRange(orderedAlbums);
        }

        public List<AlbumItem> LoadRecommendedAlbumsFromDatabase()
        {
            var albums = musicDatabase.LoadAlbums().ToObservable();
            var recommendedAlbums = albums?.Where(x => x.Favorite).ToList();
            return recommendedAlbums;
        }


        public List<AlbumItem> Contains(string column, string value)
        {
            return musicDatabase.LoadAlbumsFromColumnValue(column, value);
        }

        public void LoadArtistsFromDatabase()
        {
            Artists.Clear();
            LogHelper.Log("Loading artists from MusicDB ...");
            var artists = LoadArtists(null);
            LogHelper.Log("Found " + artists.Count + " artists from MusicDB");
            Artists.AddRange(artists.OrderBy(x => x.Name).ToObservable());
        }
        
        public void LoadTracksFromDatabase()
        {
            Tracks.Clear();
            Tracks.AddRange(musicDatabase.LoadTracks());
            LogHelper.Log("Error selecting tracks from database.");
        }

        bool IsMusicDatabaseEmpty()
        {
            return musicDatabase.IsEmpty();
        }

        public async Task LoadPlaylistsFromDatabase()
        {
            var trackColl = await trackCollectionRepository.LoadTrackCollections().ToObservableAsync();
            foreach (var trackCollection in trackColl)
            {
                var observableCollection = await tracklistItemRepository.LoadTracks(trackCollection);
                foreach (TracklistItem tracklistItem in observableCollection)
                {
                    TrackItem item = musicDatabase.LoadTrackFromId(tracklistItem.TrackId);
                    trackCollection.Playlist.Add(item);
                }
            }
            TrackCollections = trackColl;
        }
        #endregion
        #region video
        bool IsVideoDatabaseEmpty()
        {
            return videoDatabase.IsEmpty();
        }

        private async Task loadVideosFromDatabase()
        {
            await DispatchHelper.InvokeInUIThreadHighPriority(() => Videos.Clear());
            var videos = LoadVideos(x => x.IsCameraRoll == false && x.IsTvShow == false);
            LogHelper.Log($"Found {videos.Count} videos.");
            var newVideos = videos.OrderBy(x => x.Name);
            foreach (var v in newVideos)
                await DispatchHelper.InvokeInUIThreadHighPriority(() => Videos.Add(v));
        }

        private async Task loadShowsFromDatabase()
        {
            await DispatchHelper.InvokeInUIThreadHighPriority(() => Shows.Clear());
            var shows = LoadVideos(x => x.IsTvShow);
            LogHelper.Log($"Found {shows.Count} videos.");
            foreach (var item in shows)
                await AddTvShow(item);
        }

        private async Task loadCameraRollFromDatabase()
        {
            await DispatchHelper.InvokeInUIThreadHighPriority(() => CameraRoll.Clear());
            var camVideos = LoadVideos(x => x.IsCameraRoll);
            LogHelper.Log($"Found {camVideos.Count} camera videos.");
            var newVideos = camVideos.OrderBy(x => x.Name);
            foreach (var item in newVideos)
                CameraRoll.Add(item);
        }
        #endregion
        #region streams
        public async Task LoadStreamsFromDatabase()
        {
            await DispatchHelper.InvokeInUIThreadHighPriority(() => Streams.Clear());
            var streams = await LoadStreams();
            await DispatchHelper.InvokeInUIThreadHighPriority(() => Streams.AddRange(streams));
        }

        public async Task<StreamMedia> LoadStreamFromDatabaseOrCreateOne(string mrl)
        {
            var stream = await LoadStream(mrl);
            if (stream == null)
            {
                stream = new StreamMedia(mrl);
                await streamsDatabase.Insert(stream);
            }
            return stream;
        }

        public Task Update(StreamMedia stream)
        {
            return streamsDatabase.Update(stream);
        }
        #endregion
        #endregion

        #region TODO:STUFF???
        // Returns false is no snapshot generation was required, true otherwise
        private async Task<Boolean> GenerateThumbnail(VideoItem videoItem)
        {
            if (videoItem.HasThumbnail)
                return false;
            try
            {
                if (ContinueIndexing != null)
                {
                    await ContinueIndexing.Task;
                    ContinueIndexing = null;
                }

                WriteableBitmap image = null;
                StorageItemThumbnail thumb = null;
                // If file is a mkv, we save the thumbnail in a VideoPic folder so we don't consume CPU and resources each launch
                if (VLCFileExtensions.MFSupported.Contains(videoItem.Type.ToLower()))
                {
                    if (await videoItem.LoadFileFromPath())
                        thumb = await ThumbsService.GetThumbnail(videoItem.File);
                }
                // If MF thumbnail generation failed or wasn't supported:
                if (thumb == null)
                {
                    if (await videoItem.LoadFileFromPath() || !string.IsNullOrEmpty(videoItem.Token))
                    {
                        var res = await ThumbsService.GetScreenshot(videoItem.GetMrlAndFromType(true).Item2);
                        image = res?.Bitmap();
                    }
                }

                if (thumb == null && image == null)
                    return false;

                await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Low, async () =>
                {
                    if (thumb != null)
                    {
                        image = new WriteableBitmap((int)thumb.OriginalWidth, (int)thumb.OriginalHeight);
                        await image.SetSourceAsync(thumb);
                    }
                    await DownloadAndSaveHelper.WriteableBitmapToStorageFile(image, videoItem.Id.ToString());
                    videoItem.HasThumbnail = true;
                });

                videoDatabase.Update(videoItem);
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log(ex.ToString());
            }
            return false;
        }

        public async Task<PlaylistItem> AddNewPlaylist(string trackCollectionName)
        {
            if (string.IsNullOrEmpty(trackCollectionName))
                return null;
            PlaylistItem trackCollection = null;
            trackCollection = await trackCollectionRepository.LoadFromName(trackCollectionName);
            if (trackCollection != null)
            {
                await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () => ToastHelper.Basic(Strings.PlaylistAlreadyExists));
            }
            else
            {
                trackCollection = new PlaylistItem();
                trackCollection.Name = trackCollectionName;
                await trackCollectionRepository.Add(trackCollection);
                TrackCollections.Add(trackCollection);
            }
            return trackCollection;
        }

        public Task DeletePlaylistTrack(TrackItem track, PlaylistItem trackCollection)
        {
            return tracklistItemRepository.Remove(track.Id, trackCollection.Id);
        }

        public async Task DeletePlaylist(PlaylistItem trackCollection)
        {
            await trackCollectionRepository.Remove(trackCollection);
            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () =>
            {
                TrackCollections.Remove(trackCollection);
            });
        }

        public async Task AddToPlaylist(TrackItem trackItem, bool displayToastNotif = true)
        {
            if (Locator.MusicLibraryVM.CurrentTrackCollection == null) return;
            if (Locator.MusicLibraryVM.CurrentTrackCollection.Playlist.Contains(trackItem))
            {
                ToastHelper.Basic(Strings.TrackAlreadyExistsInPlaylist);
                return;
            }
            Locator.MusicLibraryVM.CurrentTrackCollection.Playlist.Add(trackItem);
            await tracklistItemRepository.Add(new TracklistItem()
            {
                TrackId = trackItem.Id,
                TrackCollectionId = Locator.MusicLibraryVM.CurrentTrackCollection.Id,
            });
            if (displayToastNotif)
                ToastHelper.Basic(string.Format(Strings.TrackAddedToYourPlaylist, trackItem.Name), false, string.Empty, "playlistview");
        }

        public async Task AddToPlaylist(AlbumItem albumItem)
        {
            if (Locator.MusicLibraryVM.CurrentTrackCollection == null) return;
            var playlistId = Locator.MusicLibraryVM.CurrentTrackCollection.Id;
            Locator.MusicLibraryVM.CurrentTrackCollection.Playlist.AddRange(albumItem.Tracks);
            foreach (TrackItem trackItem in albumItem.Tracks)
            {
                await tracklistItemRepository.Add(new TracklistItem()
                {
                    TrackId = trackItem.Id,
                    TrackCollectionId = playlistId,
                }).ConfigureAwait(false);
            }
            ToastHelper.Basic(string.Format(Strings.TrackAddedToYourPlaylist, albumItem.Name), false, string.Empty, "playlistview");
        }

        public async Task AddToPlaylist(ArtistItem artistItem)
        {
            if (Locator.MusicLibraryVM.CurrentTrackCollection == null) return;
            var playlistId = Locator.MusicLibraryVM.CurrentTrackCollection.Id;

            var songs = Locator.MediaLibrary.LoadTracksByArtistId(artistItem.Id);
            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () => Locator.MusicLibraryVM.CurrentTrackCollection.Playlist.AddRange(songs));

            foreach (TrackItem trackItem in songs)
            {
                await tracklistItemRepository.Add(new TracklistItem()
                {
                    TrackId = trackItem.Id,
                    TrackCollectionId = playlistId,
                });
            }

            ToastHelper.Basic(string.Format(Strings.TrackAddedToYourPlaylist, artistItem.Name), false, string.Empty, "playlistview");
        }

        public async Task UpdateTrackCollection(PlaylistItem trackCollection)
        {
            var loadTracks = await tracklistItemRepository.LoadTracks(trackCollection);
            foreach (TracklistItem tracklistItem in loadTracks)
            {
                await tracklistItemRepository.Remove(tracklistItem);
            }
            foreach (TrackItem trackItem in trackCollection.Playlist)
            {
                var trackListItem = new TracklistItem { TrackId = trackItem.Id, TrackCollectionId = trackCollection.Id };
                await tracklistItemRepository.Add(trackListItem);
            }
        }

        public async Task RemoveMediaFromCollectionAndDatabase(IMediaItem media)
        {
            if (media is TrackItem)
            {
                var trackItem = media as TrackItem;
                var trackDB = LoadTrackById(trackItem.Id);
                if (trackDB == null)
                    return;
                musicDatabase.Remove(trackDB);
                await DispatchHelper.InvokeInUIThreadHighPriority(() =>
                    Tracks.Remove(Tracks.FirstOrDefault(x => x.Path == trackItem.Path)));

                var albumDB = LoadAlbum(trackItem.AlbumId);
                if (albumDB != null)
                {
                    var albumTracks = LoadTracksByAlbumId(albumDB.Id);
                    if (!albumTracks.Any())
                    {
                        musicDatabase.Remove(albumDB);
                        await DispatchHelper.InvokeInUIThreadHighPriority(() =>
                            Albums.Remove(Albums.FirstOrDefault(x => x.Id == trackItem.AlbumId)));
                    }
                }

                var artistDB = LoadArtist(trackItem.ArtistId);
                if (artistDB != null)
                {
                    var artistAlbums = LoadAlbums(artistDB.Id);
                    if (!artistAlbums.Any())
                    {
                        musicDatabase.Remove(artistDB);
                        await DispatchHelper.InvokeInUIThreadHighPriority(() =>
                            Artists.Remove(Artists.FirstOrDefault(x => x.Id == trackItem.ArtistId)));
                    }
                }

                await Locator.PlaybackService.RemoveMedia(trackItem);
            }
            else if (media is VideoItem)
            {
                var videoItem = media as VideoItem;
                var videoDb = LoadVideoById(videoItem.Id);
                if (videoDb == null)
                    return;
                videoDatabase.Remove(videoDb);
                await DispatchHelper.InvokeInUIThreadHighPriority(
                    () => Videos.Remove(Videos.FirstOrDefault(x => x.Path == videoItem.Path)));

                if (videoItem.IsTvShow)
                {
                    // Try to find an other episode that belongs to the same show.
                    VideoItem episode = await DispatchHelper.InvokeInUIThread<VideoItem>(CoreDispatcherPriority.High,
                        () => Videos.FirstOrDefault(x => x.ShowTitle == videoItem.ShowTitle));

                    if (episode == null)
                    {
                        // No other episode was found. We can remove the show from the collection.
                        await DispatchHelper.InvokeInUIThreadHighPriority(
                            () => Shows.Remove(Shows.FirstOrDefault(x => x.ShowTitle == videoItem.ShowTitle)));
                    }
                }
            }
        }

        public bool AddAlbumToPlaylist(object args)
        {
            if (Locator.MusicLibraryVM.CurrentTrackCollection == null)
            {
                ToastHelper.Basic(Strings.HaveToSelectPlaylist, false, "selectplaylist");
                return false;
            }

            Locator.MusicLibraryVM.AddToPlaylistCommand.Execute(Locator.MusicLibraryVM.CurrentAlbum);
            return true;
        }

        public async Task<TrackItem> GetTrackItemFromFile(StorageFile track, string token = null)
        {
            //TODO: Warning, is it safe to consider this a good idea?
            var trackItem = musicDatabase.LoadTrackFromPath(track.Path);
            if (trackItem != null)
            {
                return trackItem;
            }

            MusicProperties trackInfos = null;
            try
            {
                trackInfos = await track.Properties.GetMusicPropertiesAsync();
            }
            catch
            {

            }
            trackItem = new TrackItem
            {
                ArtistName = (string.IsNullOrEmpty(trackInfos?.Artist)) ? Strings.UnknownArtist : trackInfos?.Artist,
                AlbumName = trackInfos?.Album ?? Strings.UnknownAlbum,
                Name = (string.IsNullOrEmpty(trackInfos?.Title)) ? track.DisplayName : trackInfos?.Title,
                Path = track.Path,
                Duration = trackInfos?.Duration ?? TimeSpan.Zero,
                File = track,
            };
            if (!string.IsNullOrEmpty(token))
            {
                trackItem.Token = token;
            }
            return trackItem;
        }

        public async Task PopulateTracks(AlbumItem album)
        {
            try
            {
                var tracks = musicDatabase.LoadTracksFromAlbumId(album.Id);
                await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () =>
                {
                    album.Tracks = tracks;
                });
            }
            catch (Exception e)
            {
                LogHelper.Log(StringsHelper.ExceptionToString(e));
            }
        }

        public async Task PopulateAlbums(ArtistItem artist)
        {
            try
            {
                var albums = musicDatabase.LoadAlbumsFromArtistId(artist.Id).ToObservable();
                await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () =>
                {
                    artist.Albums = albums;
                });
            }
            catch (Exception e)
            {
                LogHelper.Log(StringsHelper.ExceptionToString(e));
            }
        }

        public async Task PopulateAlbumsWithTracks(ArtistItem artist)
        {
            try
            {
                var albums = musicDatabase.LoadAlbumsFromIdWithTracks(artist.Id).ToObservable();
                var groupedAlbums = new ObservableCollection<GroupItemList<TrackItem>>();
                var groupQuery = from album in albums
                                 orderby album.Name
                                 group album.Tracks by album into a
                                 select new { GroupName = a.Key, Items = a };
                foreach (var g in groupQuery)
                {
                    GroupItemList<TrackItem> tracks = new GroupItemList<TrackItem>();
                    tracks.Key = g.GroupName;
                    foreach (var track in g.Items)
                    {
                        tracks.AddRange(track);
                    }
                    groupedAlbums.Add(tracks);
                }

                await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () =>
                {
                    artist.Albums = albums;
                    artist.AlbumsGrouped = groupedAlbums;
                });
            }
            catch { }
        }

        public async Task RemoveStreamFromCollectionAndDatabase(StreamMedia stream)
        {
            var collectionStream = await DispatchHelper.InvokeInUIThread<StreamMedia>(CoreDispatcherPriority.High,
                () => Streams?.FirstOrDefault(x => x.Path == stream.Path));
            if (collectionStream != null)
            {
                await DispatchHelper.InvokeInUIThreadHighPriority(() => Streams.Remove(collectionStream));
            }
            await DeleteStream(stream);
        }
        #endregion
        #region database operations
        #region audio
        public Task<List<TracklistItem>> LoadTracks(PlaylistItem trackCollection)
        {
            return tracklistItemRepository.LoadTracks(trackCollection);
        }

        public TrackItem LoadTrackById(int id)
        {
            return musicDatabase.LoadTrackFromId(id);
        }

        public List<TrackItem> LoadTracksByArtistId(int id)
        {
            return musicDatabase.LoadTracksFromArtistId(id);
        }

        public List<TrackItem> LoadTracksByAlbumId(int id)
        {
            return musicDatabase.LoadTracksFromAlbumId(id);
        }

        public ArtistItem LoadArtist(int id)
        {
            return musicDatabase.LoadArtistFromId(id);
        }

        public ArtistItem LoadViaArtistName(string name)
        {
            return musicDatabase.LoadFromArtistName(name);
        }

        public AlbumItem LoadAlbum(int id)
        {
            return musicDatabase.LoadAlbumFromId(id);
        }

        public List<AlbumItem> LoadAlbums(int artistId)
        {
            return musicDatabase.LoadAlbumsFromArtistId(artistId);
        }

        public int LoadAlbumsCount(int artistId)
        {
            return musicDatabase.LoadAlbumsCountFromId(artistId);
        }

        public void Update(ArtistItem artist)
        {
            musicDatabase.Update(artist);
        }

        public void Update(AlbumItem album)
        {
            musicDatabase.Update(album);
        }

        public void Update(TrackItem track)
        {
            musicDatabase.Update(track);
        }

        public Task Remove(TracklistItem tracklist)
        {
            return tracklistItemRepository.Remove(tracklist);
        }

        public Task RemoveTrackInPlaylist(int trackid, int playlistid)
        {
            return tracklistItemRepository.Remove(trackid, playlistid);
        }

        public int ArtistCount()
        {
            return musicDatabase.ArtistsCount();
        }

        public ArtistItem ArtistAt(int index)
        {
            return musicDatabase.ArtistAt(index);
        }

        public List<ArtistItem> LoadArtists(Expression<Func<ArtistItem, bool>> predicate)
        {
            return musicDatabase.LoadArtists(predicate);
        }

        public List<AlbumItem> LoadAlbums(Expression<Func<AlbumItem, bool>> predicate)
        {
            return musicDatabase.Load(predicate);
        }

        public List<TrackItem> LoadTracks()
        {
            return musicDatabase.LoadTracks();
        }

        #endregion
        #region video
        public List<VideoItem> LoadVideos(Expression<Func<VideoItem, bool>> predicate)
        {
            return videoDatabase.Load(predicate);
        }

        public VideoItem LoadVideoById(int id)
        {
            return videoDatabase.LoadVideo(id);
        }

        public void UpdateVideo(VideoItem video)
        {
            videoDatabase.Update(video);
        }

        public List<VideoItem> ContainsVideo(string column, string val)
        {
            return videoDatabase.Contains(column, val);
        }
        #endregion
        #region streams
        private Task<List<StreamMedia>> LoadStreams()
        {
            return streamsDatabase.Load();
        }
    
        private Task<StreamMedia> LoadStream(string mrl)
        {
            return streamsDatabase.Get(mrl);
        }

        private Task DeleteStream(StreamMedia media)
        {
            return streamsDatabase.Delete(media);
        }
        #endregion
        #endregion
    }
}
