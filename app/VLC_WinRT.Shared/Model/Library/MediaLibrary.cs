using Autofac;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using VLC_WinRT.Database;
using VLC_WinRT.Helpers;
using VLC_WinRT.Model.Music;
using VLC_WinRT.Model.Video;
using VLC_WinRT.Services.Interface;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels.MusicVM;
using Windows.Storage;
using WinRTXamlToolkit.IO.Extensions;
using VLC_WinRT.ViewModels;
using Windows.UI.Core;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;
using VLC_WinRT.Helpers.MusicLibrary;
using VLC_WinRT.Helpers.VideoLibrary;
using VLC_WinRT.Model.Stream;
using VLC_WinRT.Services.RunTime;

namespace VLC_WinRT.Model.Library
{
    public class MediaLibrary
    {
        #region properties
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
        readonly ArtistDatabase artistDatabase = new ArtistDatabase();
        readonly TrackDatabase trackDatabase = new TrackDatabase();
        readonly AlbumDatabase albumDatabase = new AlbumDatabase();
        readonly TracklistItemRepository tracklistItemRepository = new TracklistItemRepository();
        readonly TrackCollectionRepository trackCollectionRepository = new TrackCollectionRepository();
        
        readonly VideoRepository videoDatabase = new VideoRepository();
        
        readonly StreamsDatabase streamsDatabase = new StreamsDatabase();
        #endregion

        #region collections
        public SmartCollection<ArtistItem> Artists { get; private set; } = new SmartCollection<ArtistItem>();
        public SmartCollection<AlbumItem> Albums { get; private set; } = new SmartCollection<AlbumItem>();
        public SmartCollection<TrackItem> Tracks { get; private set; } = new SmartCollection<TrackItem>();
        public SmartCollection<PlaybackService> TrackCollections { get; private set; } = new SmartCollection<PlaybackService>();

        public SmartCollection<VideoItem> Videos { get; private set; } = new SmartCollection<VideoItem>();
        public SmartCollection<VideoItem> ViewedVideos { get; private set; } = new SmartCollection<VideoItem>();
        public SmartCollection<VideoItem> CameraRoll { get; private set; } = new SmartCollection<VideoItem>();
        public SmartCollection<TvShow> Shows { get; private set; } = new SmartCollection<TvShow>();

        public SmartCollection<StreamMedia> Streams { get; private set; } = new SmartCollection<StreamMedia>();
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

        async Task DiscoverMediaItemOrWaitAsync(StorageFile storageItem, bool isCameraRoll)
        {
            await MediaItemDiscovererSemaphoreSlim.WaitAsync();
            try
            {
                await ParseMediaFile(storageItem, isCameraRoll);
            }
            catch (Exception e)
            {
                LogHelper.Log(StringsHelper.ExceptionToString(e));
            }
            finally
            {
                MediaItemDiscovererSemaphoreSlim.Release();
            }
        }

        public async Task FetchVideoThumbnailOrWaitAsync(VideoItem videoVm)
        {
            await VideoThumbnailFetcherSemaphoreSlim.WaitAsync();
            try
            {
                await GenerateThumbnail(videoVm);
                if (videoVm.Type == ".mkv")
                    await Locator.VideoMetaService.GetMoviePicture(videoVm).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                LogHelper.Log(StringsHelper.ExceptionToString(e));
            }
            finally
            {
                VideoThumbnailFetcherSemaphoreSlim.Release();
            }
        }

        #endregion

        #region IndexationLogic
        public void DropTablesIfNeeded()
        {
            if (!Numbers.NeedsToDrop()) return;
            trackCollectionRepository.Drop();
            tracklistItemRepository.Drop();
            albumDatabase.Drop();
            artistDatabase.Drop();
            trackDatabase.Drop();
            trackCollectionRepository.Initialize();
            tracklistItemRepository.Initialize();
            albumDatabase.Initialize();
            artistDatabase.Initialize();
            trackDatabase.Initialize();

            videoDatabase.Drop();
            videoDatabase.Initialize();
        }

        public async Task PerformRoutineCheckIfNotBusy()
        {
            // Routine check to add new files if there are new ones
            //if (!IsBusy)
            //{
            //    await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () =>
            //    {
            //        IsBusy = true;
            //    });
            await StartIndexing();
            //    await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () =>
            //    {
            //        IsBusy = false;
            //        Locator.MainVM.InformationText = "";
            //    });
            //}
        }

        public async Task Initialize()
        {
            MediaLibraryIndexingState = LoadingState.Loading;
            Artists.Clear();
            Albums.Clear();
            Tracks.Clear();
            TrackCollections.Clear();

            Videos.Clear();
            ViewedVideos.Clear();
            CameraRoll.Clear();
            Shows.Clear();

            if (_alreadyIndexedOnce) return;
            _alreadyIndexedOnce = true;
            // Doing full indexing from scratch if 0 tracks are found
            if (await IsMusicDatabaseEmpty() && await IsVideoDatabaseEmpty())
            {
                await StartIndexing();
            }
            else
            {
                // Else, perform a Routine Indexing (without dropping tables)
                await PerformMediaLibraryIndexing();
            }
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () => MediaLibraryIndexingState = LoadingState.Loaded);
        }

        async Task StartIndexing()
        {
            artistDatabase.DeleteAll();
            albumDatabase.DeleteAll();
            trackDatabase.DeleteAll();
            trackCollectionRepository.DeleteAll();
            tracklistItemRepository.DeleteAll();

            artistDatabase.Initialize();
            albumDatabase.Initialize();
            trackDatabase.Initialize();
            trackCollectionRepository.Initialize();
            tracklistItemRepository.Initialize();

            Artists?.Clear();
            Albums?.Clear();
            Tracks?.Clear();
            TrackCollections?.Clear();

            videoDatabase.DeleteAll();

            Videos?.Clear();
            ViewedVideos?.Clear();
            CameraRoll?.Clear();
            Shows?.Clear();
            await PerformMediaLibraryIndexing();
        }

        async Task PerformMediaLibraryIndexing()
        {
            try
            {
#if WINDOWS_PHONE_APP
                await GetAllMusicFolders();
                return;
#endif
                await DiscoverMediaItems(await MediaLibraryHelper.GetSupportedFiles(KnownFolders.VideosLibrary));

                await DiscoverMediaItems(await MediaLibraryHelper.GetSupportedFiles(KnownFolders.MusicLibrary));

                if (await KnownFolders.PicturesLibrary.ContainsFolderAsync("Camera Roll"))
                {
                    var cameraRoll = await KnownFolders.PicturesLibrary.GetFolderAsync("Camera Roll");
                    await DiscoverMediaItems(await MediaLibraryHelper.GetSupportedFiles(cameraRoll), true);
                }

                // Cortana gets all those artists, albums, songs names
                var artists = await LoadArtists(null);
                if (artists != null)
                    await CortanaHelper.SetPhraseList("artistName", artists.Select(x => x.Name).ToList());

                var songs = await LoadTracks();
                if (songs != null)
                    await CortanaHelper.SetPhraseList("songName", songs.Select(x => x.Name).ToList());
            }
            catch (Exception e)
            {
                LogHelper.Log(StringsHelper.ExceptionToString(e));
            }
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

        async Task ParseMediaFile(StorageFile item, bool isCameraRoll)
        {
            try
            {
                if (VLCFileExtensions.AudioExtensions.Contains(item.FileType.ToLower()))
                {
                    if (await trackDatabase.DoesTrackExist(item.Path))
                        return;

                    var media = await Locator.VLCService.GetMediaFromPath(item.Path);
                    var mP = await Locator.VLCService.GetMusicProperties(media);
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
                        ArtistItem artist = await LoadViaArtistName(string.IsNullOrEmpty(albumArtistName) ? artistName : albumArtistName);
                        if (artist == null)
                        {
                            artist = new ArtistItem();
                            artist.Name = string.IsNullOrEmpty(albumArtistName) ? artistName : albumArtistName;
                            artist.PlayCount = 0;
                            await artistDatabase.Add(artist);
                            AddArtist(artist);
                        }

                        var albumName = mP.Album?.Trim();
                        var albumYear = mP.Year;
                        AlbumItem album = await albumDatabase.LoadAlbumViaName(artist.Id, albumName);
                        if (album == null)
                        {
                            var albumUrl = await Locator.VLCService.GetArtworkUrl(media);
                            string albumSimplifiedUrl = null;
                            if (!string.IsNullOrEmpty(albumUrl) && albumUrl.StartsWith("file://"))
                            {
                                // The Uri will be like
                                // ms-appdata:///local/vlc/art/artistalbum/30 Seconds To Mars/B-sides & Rarities/art.jpg
                                var indexStart = albumUrl.IndexOf("vlc/art/artistalbum/", StringComparison.Ordinal);
                                if (indexStart != -1)
                                {
                                    albumSimplifiedUrl = albumUrl.Substring(indexStart, albumUrl.Length - indexStart);
                                    Debug.WriteLine("VLC_WinRT : found album cover with TagLib - " + albumName);
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
                            await albumDatabase.Add(album);
                            AddAlbum(album);
                            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () =>
                            {
                                Locator.MainVM.InformationText = string.Format(Strings.AlbumsFound, Albums.Count);
                            });
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
                            Genre = mP.Genre
                        };
                        await trackDatabase.Add(track);
                        AddTrack(track);
                    }
                }
                else if (VLCFileExtensions.VideoExtensions.Contains(item.FileType.ToLower()))
                {
                    if (await videoDatabase.DoesMediaExist(item.Path))
                        return;

                    var video = await MediaLibraryHelper.GetVideoItem(item);
                    
                    await videoDatabase.Insert(video);

                    if (video.IsTvShow)
                    {
                        await AddTvShow(video);
                    }
                    else if (isCameraRoll)
                    {
                        video.IsCameraRoll = true;
                        CameraRoll.Add(video);
                    }
                    else
                    {
                        Videos.Add(video);
                    }
                }
                else
                {
                    Debug.WriteLine($"{item.Path} is not a media file");
                }
            }
            catch (Exception e)
            {
#if DEBUG
                throw e;
#endif
            }
        }



        public void AddArtist(ArtistItem artist)
        {
            Artists.Add(artist);
        }

        public void AddAlbum(AlbumItem album)
        {
            Albums.Add(album);
        }

        public void AddTrack(TrackItem track)
        {
            Tracks.Add(track);
        }

        public async Task AddTvShow(VideoItem episode)
        {
            episode.IsTvShow = true;
            try
            {
                TvShow show = Shows.FirstOrDefault(x => x.ShowTitle == episode.ShowTitle);
                if (show == null)
                {
                    // Generate a thumbnail for the show
                    await episode.ResetVideoPicture();

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

        //============================================
        #region DataLogic
        #region audio
        public ObservableCollection<GroupItemList<AlbumItem>> OrderAlbums(OrderType orderType, OrderListing orderListing)
        {
            if (Albums == null) return null;
            var groupedAlbums = new ObservableCollection<GroupItemList<AlbumItem>>();
            if (orderType == OrderType.ByArtist)
            {
                if (orderListing == OrderListing.Ascending)
                {
                    var groupQuery = from album in Albums
                                     group album by Strings.HumanizedArtistName(album.Artist) into a
                                     orderby a.Key
                                     select new { GroupName = a.Key, Items = a };
                    foreach (var g in groupQuery)
                    {
                        GroupItemList<AlbumItem> albums = new GroupItemList<AlbumItem>();
                        albums.Key = g.GroupName;
                        foreach (var album in g.Items)
                        {
                            albums.Add(album);
                        }
                        groupedAlbums.Add(albums);
                    }
                }
                else if (orderListing == OrderListing.Descending)
                {
                    var groupQuery = from album in Albums
                                     group album by Strings.HumanizedArtistName(album.Artist) into a
                                     orderby a.Key descending
                                     select new { GroupName = a.Key, Items = a };
                    foreach (var g in groupQuery)
                    {
                        GroupItemList<AlbumItem> albums = new GroupItemList<AlbumItem>();
                        albums.Key = g.GroupName;
                        foreach (var album in g.Items)
                        {
                            albums.Add(album);
                        }
                        groupedAlbums.Add(albums);
                    }
                }
            }
            else if (orderType == OrderType.ByDate)
            {
                if (orderListing == OrderListing.Ascending)
                {
                    var groupQuery = from album in Albums
                                     orderby album.Year
                                     group album by Strings.HumanizedYear(album.Year) into a
                                     select new { GroupName = a.Key, Items = a };
                    foreach (var g in groupQuery)
                    {
                        GroupItemList<AlbumItem> albums = new GroupItemList<AlbumItem>();
                        albums.Key = g.GroupName;
                        foreach (var album in g.Items)
                        {
                            albums.Add(album);
                        }
                        groupedAlbums.Add(albums);
                    }
                }
                else if (orderListing == OrderListing.Descending)
                {
                    var groupQuery = from album in Albums
                                     orderby album.Year descending
                                     group album by Strings.HumanizedYear(album.Year) into a
                                     select new { GroupName = a.Key, Items = a };
                    foreach (var g in groupQuery)
                    {
                        GroupItemList<AlbumItem> albums = new GroupItemList<AlbumItem>();
                        albums.Key = g.GroupName;
                        foreach (var album in g.Items)
                        {
                            albums.Add(album);
                        }
                        groupedAlbums.Add(albums);
                    }
                }
            }
            else if (orderType == OrderType.ByAlbum)
            {
                if (orderListing == OrderListing.Ascending)
                {
                    var groupQuery = from album in Albums
                                     group album by Strings.HumanizedAlbumFirstLetter(album.Name) into a
                                     orderby a.Key
                                     select new { GroupName = a.Key, Items = a };
                    foreach (var g in groupQuery)
                    {
                        GroupItemList<AlbumItem> albums = new GroupItemList<AlbumItem>();
                        albums.Key = g.GroupName;
                        foreach (var album in g.Items)
                        {
                            albums.Add(album);
                        }
                        groupedAlbums.Add(albums);
                    }
                }
                else if (orderListing == OrderListing.Descending)
                {
                    var groupQuery = from album in Albums
                                     group album by Strings.HumanizedAlbumFirstLetter(album.Name) into a
                                     orderby a.Key descending
                                     select new { GroupName = a.Key, Items = a };
                    foreach (var g in groupQuery)
                    {
                        GroupItemList<AlbumItem> albums = new GroupItemList<AlbumItem>();
                        albums.Key = g.GroupName;
                        foreach (var album in g.Items)
                        {
                            albums.Add(album);
                        }
                        groupedAlbums.Add(albums);
                    }
                }
            }
            return groupedAlbums;
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

        public IEnumerable<IGrouping<char, TrackItem>> OrderTracks()
        {
            return Tracks?.GroupBy(x => string.IsNullOrEmpty(x.Name) ? Strings.UnknownChar : (char.IsLetter(x.Name.ElementAt(0)) ? x.Name.ToUpper().ElementAt(0) : Strings.UnknownChar));
        }
        #endregion
        #region DatabaseLogic
        public async Task LoadAlbumsFromDatabase()
        {
            try
            {
                Albums?.Clear();
                LogHelper.Log("Loading albums from MusicDB ...");
                var albums = await albumDatabase.LoadAlbums().ToObservableAsync();
                var orderedAlbums = albums.OrderBy(x => x.Artist).ThenBy(x => x.Name);
                Albums.AddRange(orderedAlbums);
            }
            catch
            {
                LogHelper.Log("Error selecting albums from database.");
            }
        }

        public async Task<List<AlbumItem>> LoadRecommendedAlbumsFromDatabase()
        {
            try
            {
                var albums = await albumDatabase.LoadAlbums().ToObservableAsync();
                var recommendedAlbums = albums?.Where(x => x.Favorite).ToList();
                if (recommendedAlbums.Count() <= 10) // TODO : Magic number
                {
                    var nonfavAlbums = albums.Where(x => !x.Favorite).Take(10);
                    if (nonfavAlbums.Any())
                    {
                        foreach (var nonFavAlbum in nonfavAlbums)
                        {
                            recommendedAlbums.Add(nonFavAlbum);
                        }
                    }
                }
                return recommendedAlbums;
            }
            catch (Exception)
            {
                LogHelper.Log("Error selecting random albums from database.");
            }
            return new List<AlbumItem>();
        }


        public Task<List<AlbumItem>> Contains(string column, string value)
        {
            return albumDatabase.Contains(column, value);
        }

        public async Task LoadArtistsFromDatabase()
        {
            try
            {
                Artists?.Clear();
                LogHelper.Log("Loading artists from MusicDB ...");
                var artists = await LoadArtists(null);
                LogHelper.Log("Found " + artists.Count + " artists from MusicDB");
                Artists.AddRange(artists.OrderBy(x => x.Name).ToObservable());
            }
            catch { }
        }

        public async Task<ObservableCollection<ArtistItem>> LoadRandomArtistsFromDatabase()
        {
            try
            {
                var topArtists = (await LoadArtists(x => x.PlayCount > 10).ToObservableAsync()).Take(20);
                // We use user top artists to search for similar artists in its collection, to recommend them
                if (topArtists.Any())
                {
                    var random = new Random().Next(0, topArtists.Count() - 1);
                    var suggestedArtists = await MusicFlow.GetFollowingArtistViaSimilarity(topArtists.ElementAt(random));
                    if (suggestedArtists != null)
                        return new ObservableCollection<ArtistItem>(suggestedArtists);
                }
            }
            catch (Exception)
            {
                LogHelper.Log("Error selecting random and recommended artists from database.");
            }
            return new ObservableCollection<ArtistItem>();
        }

        public async Task LoadTracksFromDatabase()
        {
            try
            {
                Tracks = await trackDatabase.LoadTracks().ToObservableAsync();
            }
            catch (Exception)
            {
                LogHelper.Log("Error selecting tracks from database.");
            }
        }

        Task<bool> IsMusicDatabaseEmpty()
        {
            return trackDatabase.IsEmpty();
        }

        public async Task LoadPlaylistsFromDatabase()
        {
            try
            {
                var trackColl = await trackCollectionRepository.LoadTrackCollections().ToObservableAsync();
                foreach (var trackCollection in trackColl)
                {
                    var observableCollection = await tracklistItemRepository.LoadTracks(trackCollection);
                    foreach (TracklistItem tracklistItem in observableCollection)
                    {
                        TrackItem item = await trackDatabase.LoadTrack(tracklistItem.TrackId);
                        trackCollection.Playlist.Add(item);
                    }
                }
                TrackCollections = trackColl;
            }
            catch (Exception)
            {
                LogHelper.Log("Error getting database.");
            }
        }
        #endregion
        #region video
        Task<bool> IsVideoDatabaseEmpty()
        {
            return videoDatabase.IsEmpty();
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
                    LogHelper.Log($"File not found : {videoVm.Path}");
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
        #region streams
        public async Task LoadStreamsFromDatabase()
        {
            Streams?.Clear();
            var streams = await LoadStreams();
            Streams.AddRange(streams);
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
        #endregion
        #endregion

        #region TODO:STUFF???
        // Returns false is no snapshot generation was required, true otherwise
        private async Task<Boolean> GenerateThumbnail(VideoItem videoItem)
        {
            if (videoItem.IsPictureLoaded)
                return false;
            try
            {
                if (ContinueIndexing != null)
                {
                    await ContinueIndexing.Task;
                    ContinueIndexing = null;
                }
#if WINDOWS_PHONE_APP
                if (MemoryUsageHelper.PercentMemoryUsed() > MemoryUsageHelper.MaxRamForResourceIntensiveTasks)
                    return false;
#endif
                var thumbnailTask = new TaskCompletionSource<bool>();
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, async () =>
                {
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
                            videoItem.IsPictureLoaded = true;
                            tcs.SetResult(true);
                        });
                        await tcs.Task;

                        videoItem.IsPictureLoaded = true;
                        await videoDatabase.Update(videoItem);
                        await videoItem.ResetVideoPicture();
                        thumbnailTask.TrySetResult(true);
                    }
                    else
                    {
                        thumbnailTask.TrySetResult(false);
                    }
                });
                return await thumbnailTask.Task;
            }
            catch (Exception ex)
            {
                LogHelper.Log(ex.ToString());
            }
            return false;
        }

        public async Task AddNewPlaylist(string trackCollectionName)
        {
            if (string.IsNullOrEmpty(trackCollectionName)) return;
            PlaybackService trackCollection = null;
            trackCollection = await trackCollectionRepository.LoadFromName(trackCollectionName);
            if (trackCollection != null)
            {
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () => ToastHelper.Basic(Strings.PlaylistAlreadyExists));
            }
            else
            {
                trackCollection = new PlaybackService();
                trackCollection.Name = trackCollectionName;
                await trackCollectionRepository.Add(trackCollection);
                TrackCollections.Add(trackCollection);
            }
        }

        public Task DeletePlaylistTrack(TrackItem track, PlaybackService trackCollection)
        {
            return tracklistItemRepository.Remove(track.Id, trackCollection.Id);
        }

        public async Task DeletePlaylist(PlaybackService trackCollection)
        {
            await trackCollectionRepository.Remove(trackCollection);
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
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
                ToastHelper.Basic(string.Format(Strings.TrackAddedToYourPlaylist, trackItem.Name));
        }

        public async Task AddToPlaylist(AlbumItem albumItem)
        {
            if (Locator.MusicLibraryVM.CurrentTrackCollection == null) return;
            var playlistId = Locator.MusicLibraryVM.CurrentTrackCollection.Id;
            foreach (TrackItem trackItem in albumItem.Tracks)
            {
                Locator.MusicLibraryVM.CurrentTrackCollection.Playlist.Add(trackItem);
                await tracklistItemRepository.Add(new TracklistItem()
                {
                    TrackId = trackItem.Id,
                    TrackCollectionId = playlistId,
                });
            }
            ToastHelper.Basic(string.Format(Strings.TrackAddedToYourPlaylist, albumItem.Name));
        }

        public async Task UpdateTrackCollection(PlaybackService trackCollection)
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

        public async Task RemoveTrackFromCollectionAndDatabase(TrackItem trackItem)
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                try
                {
                    trackDatabase.Remove(Tracks.FirstOrDefault(x => x.Path == trackItem.Path));
                    Tracks.Remove(Tracks.FirstOrDefault(x => x.Path == trackItem.Path));
                    var album = Albums.FirstOrDefault(x => x.Id == trackItem.AlbumId);
                    album?.Tracks.Remove(album.Tracks.FirstOrDefault(x => x.Path == trackItem.Path));

                    var artist = Artists.FirstOrDefault(x => x.Id == trackItem.ArtistId);
                    var artistalbum = artist?.Albums.FirstOrDefault(x => x.Id == trackItem.AlbumId);
                    artistalbum?.Tracks.Remove(artistalbum.Tracks.FirstOrDefault(x => x.Path == trackItem.Path));
                    if (album.Tracks.Count == 0)
                    {
                        // We should remove the album as a whole
                        Albums.Remove(album);
                        albumDatabase.Remove(album);
                        artist.Albums.Remove(artistalbum);
                    }
                    var playingTrack = Locator.MediaPlaybackViewModel.PlaybackService.Playlist.FirstOrDefault(x => x.Id == trackItem.Id);
                    if (playingTrack != null) Locator.MediaPlaybackViewModel.PlaybackService.Playlist.Remove(playingTrack);
                }
                catch
                {
                }
            });
        }

        public bool AddAlbumToPlaylist(object args)
        {
            if (Locator.MusicLibraryVM.CurrentTrackCollection == null)
            {
#if WINDOWS_PHONE_APP
                if (args is ContentDialogButtonClickEventArgs)
                    ((ContentDialogButtonClickEventArgs)args).Cancel = true;
#endif
                ToastHelper.Basic(Strings.HaveToSelectPlaylist, false, "selectplaylist");
                return false;
            }
#if WINDOWS_APP
            var flyout = ((AddAlbumToPlaylistBase)args).GetFirstDescendantOfType<SettingsFlyout>();
            flyout?.Hide();
#endif
            Locator.MusicLibraryVM.AddToPlaylistCommand.Execute(Locator.MusicLibraryVM.CurrentAlbum);
            return true;
        }

        public async Task<TrackItem> GetTrackItemFromFile(StorageFile track, string token = null)
        {
            //TODO: Warning, is it safe to consider this a good idea?
            var trackItem = await trackDatabase.LoadTrackByPath(track.Path);
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
                var tracks = await trackDatabase.LoadTracksByAlbumId(album.Id);
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
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
                var albums = await albumDatabase.LoadAlbumsFromId(artist.Id).ToObservableAsync();
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
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
                var albums = await albumDatabase.LoadAlbumsFromIdWithTracks(artist.Id).ToObservableAsync();
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

                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
                {
                    artist.Albums = albums;
                    artist.AlbumsGrouped = groupedAlbums;
                });
            }
            catch { }
        }

        public Task RemoveStreamFromCollectionAndDatabase(StreamMedia stream)
        {
            var collectionStream = Streams?.FirstOrDefault(x => x.Path == stream.Path);
            if (collectionStream != null)
            {
                Streams?.Remove(collectionStream);
            }
            return streamsDatabase.Delete(stream);
        }
        #endregion
        #region database operations
        #region audio
        public Task<List<TracklistItem>> LoadTracks(PlaybackService trackCollection)
        {
            return tracklistItemRepository.LoadTracks(trackCollection);
        }

        public Task<TrackItem> LoadTrackById(int id)
        {
            return trackDatabase.LoadTrack(id);
        }

        public Task<List<TrackItem>> LoadTracksByArtistId(int id)
        {
            return trackDatabase.LoadTracksByArtistId(id);
        }

        public async Task<List<TrackItem>> LoadTracksByAlbumId(int id)
        {
            return await trackDatabase.LoadTracksByAlbumId(id);
        }

        public async Task<ArtistItem> LoadArtist(int id)
        {
            return await artistDatabase.LoadArtist(id);
        }

        public async Task<ArtistItem> LoadViaArtistName(string name)
        {
            return await artistDatabase.LoadViaArtistName(name);
        }

        public Task<AlbumItem> LoadAlbum(int id)
        {
            return albumDatabase.LoadAlbum(id);
        }

        public Task<List<AlbumItem>> LoadAlbums(int artistId)
        {
            return albumDatabase.LoadAlbumsFromId(artistId);
        }

        public Task<int> LoadAlbumsCount(int artistId)
        {
            return albumDatabase.LoadAlbumsCountFromId(artistId);
        }

        public Task Update(ArtistItem artist)
        {
            return artistDatabase.Update(artist);
        }

        public Task Update(AlbumItem album)
        {
            return albumDatabase.Update(album);
        }

        public Task Update(TrackItem track)
        {
            return trackDatabase.Update(track);
        }

        public Task Remove(TracklistItem tracklist)
        {
            return tracklistItemRepository.Remove(tracklist);
        }

        public Task RemoveTrackInPlaylist(int trackid, int playlistid)
        {
            return tracklistItemRepository.Remove(trackid, playlistid);
        }

        public Task<int> ArtistCount()
        {
            return artistDatabase.Count();
        }

        public Task<ArtistItem> ArtistAt(int index)
        {
            return artistDatabase.At(index);
        }

        public Task<List<ArtistItem>> LoadArtists(Expression<Func<ArtistItem, bool>> predicate)
        {
            return artistDatabase.Load(predicate);
        }

        public Task<List<AlbumItem>> LoadAlbums(Expression<Func<AlbumItem, bool>> predicate)
        {
            return albumDatabase.Load(predicate);
        }

        public Task<List<TrackItem>> LoadTracks()
        {
            return trackDatabase.LoadTracks();
        }
        #endregion
        #region video
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
