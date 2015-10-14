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

namespace VLC_WinRT.Helpers.MusicLibrary
{
    public static class MusicLibraryManagement
    {
        static readonly SemaphoreSlim AlbumCoverFetcherSemaphoreSlim = new SemaphoreSlim(2);
        static readonly SemaphoreSlim ArtistPicFetcherSemaphoreSlim = new SemaphoreSlim(2);
        static readonly SemaphoreSlim TrackItemDiscovererSemaphoreSlim = new SemaphoreSlim(2);

        public static async Task FetchAlbumCoverOrWaitAsync(AlbumItem albumItem)
        {
            await AlbumCoverFetcherSemaphoreSlim.WaitAsync();
            try
            {
                await albumItem.LoadPicture();
            }
            finally
            {
                AlbumCoverFetcherSemaphoreSlim.Release();
            }
        }

        public static async Task FetchArtistPicOrWaitAsync(ArtistItem artistItem)
        {
            await ArtistPicFetcherSemaphoreSlim.WaitAsync();
            try
            {
                await artistItem.LoadPicture();
            }
            finally
            {
                ArtistPicFetcherSemaphoreSlim.Release();
            }
        }

        public static async Task DiscoverTrackItemOrWaitAsync(StorageFile storageItem)
        {
            await TrackItemDiscovererSemaphoreSlim.WaitAsync();
            try
            {
                if (!await Locator.MusicLibraryVM._trackDatabase.DoesTrackExist(storageItem.Path))
                {
                    await CreateDatabaseFromMusicFile(storageItem);
                }
            }
            catch(Exception e)
            {
                LogHelper.Log(StringsHelper.ExceptionToString(e));
                TrackItemDiscovererSemaphoreSlim.Release();
            }
            finally
            {
                TrackItemDiscovererSemaphoreSlim.Release();
            }
        }

        public static async Task LoadFromSQL()
        {
            try
            {
                LogHelper.Log("Loading artists from MusicDB ...");
                var artists = await Locator.MusicLibraryVM._artistDatabase.Load();
                LogHelper.Log("Found " + artists.Count + " artists from MusicDB");
                await OrderArtists(artists);            
                
                var albums = await Locator.MusicLibraryVM._albumDatabase.LoadAlbums(x => x.ArtistId != 0).ToObservableAsync();
                var orderedAlbums = albums.OrderBy(x => x.Artist).ThenBy(x => x.Name);

                var tracks = await Locator.MusicLibraryVM._trackDatabase.LoadTracks().ToObservableAsync();
                var groupedTracks = tracks.GroupBy(x => string.IsNullOrEmpty(x.Name) ? Strings.UnknownChar : (char.IsLetter(x.Name.ElementAt(0)) ? x.Name.ToUpper().ElementAt(0) : Strings.UnknownChar));

                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Locator.MusicLibraryVM.TopArtists = new ObservableCollection<ArtistItem>(artists.FindAll(x => x.PlayCount > 10).Take(20));
                    Locator.MusicLibraryVM.Tracks = tracks;
                    Locator.MusicLibraryVM.Albums = new ObservableCollection<AlbumItem>(orderedAlbums);

                    if (Locator.MainVM.CurrentPage == VLCPage.MainPageMusic && Locator.MusicLibraryVM.MusicView == MusicView.Albums && (Locator.MusicLibraryVM.GroupedAlbums == null || !Locator.MusicLibraryVM.GroupedAlbums.Any()))
                    {
                        OrderAlbums();
                    }
                    Locator.MusicLibraryVM.GroupedTracks = groupedTracks;
                });

                var trackColl = await Locator.MusicLibraryVM.TrackCollectionRepository.LoadTrackCollections().ToObservableAsync();
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Locator.MusicLibraryVM.TrackCollections = trackColl;
                });

                foreach (TrackCollection trackCollection in Locator.MusicLibraryVM.TrackCollections)
                {
                    var observableCollection = await Locator.MusicLibraryVM.TracklistItemRepository.LoadTracks(trackCollection);
                    foreach (TracklistItem tracklistItem in observableCollection)
                    {
                        TrackItem item = await Locator.MusicLibraryVM._trackDatabase.LoadTrack(tracklistItem.TrackId);
                        trackCollection.Playlist.Add(item);
                    }
                }
            }
            catch (Exception)
            {
                LogHelper.Log("Error getting database.");
            }
        }

        public static async Task LoadMusicFlow()
        {
            // We use user top artists to generated background images
            foreach (var artistItem in Locator.MusicLibraryVM.TopArtists)
            {
                if (artistItem.IsPictureLoaded)
                {
                    Locator.Slideshow.AddImg(artistItem.Picture);
                }
            }

            // Choosing 5 pics randomly
            var r = new Random();
            if (Locator.MusicLibraryVM.Artists.Any())
            {
                var count = Locator.MusicLibraryVM.Artists.Count;
                var tries = (count < 5) ? count : 5;
                for (int search = 0; search < tries; search++)
                {
                    var i = r.Next(0, count - 1);
                    if (Locator.MusicLibraryVM.Artists[i].IsPictureLoaded)
                    {
                        Locator.Slideshow.AddImg(Locator.MusicLibraryVM.Artists[i].Picture);
                    }
                }
            }


            // We use user top artists to search for similar artists in its collection, to recommend them
            if (Locator.MusicLibraryVM.TopArtists.Any())
            {
                var random = new Random().Next(0, Locator.MusicLibraryVM.TopArtists.Count - 1);
                var suggestedArtists = await MusicFlow.GetFollowingArtistViaSimilarity(Locator.MusicLibraryVM.TopArtists[random]);
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => Locator.MusicLibraryVM.RecommendedArtists = new ObservableCollection<ArtistItem>(suggestedArtists));
            }

            // We use the user top artists and top albums to search for "popular Music" with the Same genre, that are in its collection, to recommend them
            // Example. TopArtist = [ Muse, Coldplay ]. Genre is Rock, Pop. We'll search on the Internet popular rock/pop artists. If some of them are in the local user's collection,
            // We add them to the list
            // The code below is not fully implemented
            return;
            if (Locator.MusicLibraryVM.FavoriteAlbums.Any())
            {
                var random = new Random().Next(0, Locator.MusicLibraryVM.FavoriteAlbums.Count - 1);
                var trackItem = await Locator.MusicLibraryVM._trackDatabase.GetFirstTrackOfAlbumId(Locator.MusicLibraryVM.FavoriteAlbums[random].Id);
                if (trackItem != null)
                {
                    var popularArtists = await MusicFlow.GetPopularArtistFromGenre(trackItem.Genre);
                    if (popularArtists != null && popularArtists.Any())
                    {
                        random = new Random().Next(0, popularArtists.Count - 1);
                        Locator.MusicLibraryVM.FocusOnAnArtist = popularArtists[random];
                    }
                }
            }
        }

        public static Task DoRoutineMusicLibraryCheck()
        {
            return PerformMusicLibraryIndexing();
        }
        
        /// <summary>
        /// This method is Windows-only since the crappy WP OS throws a 
        /// NotImplementedexception when calling QueryOptions, CreateFileQueryWithOptions
        /// </summary>
        /// <returns></returns>
        public static async Task PerformMusicLibraryIndexing()
        {
            try
            {
                IReadOnlyList<StorageFile> files = null;
#if WINDOWS_PHONE_APP
                if (OSHelper.IsWindows10) // On Windows 10 Mobile devices, the API is present, but we need to use Reflection to use it
                {
                    var queryOptions = new QueryOptions { FolderDepth = FolderDepth.Deep };
                    foreach (var type in VLCFileExtensions.Supported)
                        queryOptions.FileTypeFilter.Add(type);
                    var fileQueryResult = KnownFolders.MusicLibrary.CreateFileQueryWithOptions(queryOptions);
                    var getFilesMethod = fileQueryResult.GetType()
                                        .GetRuntimeMethods()?
                                        .Where(x => x.Name == nameof(fileQueryResult.GetFilesAsync))?
                                        .FirstOrDefault(x => !x.GetParameters().Any());
                    if (getFilesMethod != null)
                    {
                        files = await ((IAsyncOperation<IReadOnlyList<StorageFile>>) getFilesMethod.Invoke(fileQueryResult, null));
                    }
                    else
                    {
                        await GetAllMusicFolders();
                        return;
                    }
                }
                else
                {
                    await GetAllMusicFolders();
                    return;
                }
#else
                var queryOptions = new QueryOptions {FolderDepth = FolderDepth.Deep};
                foreach (var type in VLCFileExtensions.Supported)
                    queryOptions.FileTypeFilter.Add(type);
                var fileQueryResult = KnownFolders.MusicLibrary.CreateFileQueryWithOptions(queryOptions);
                files = await fileQueryResult.GetFilesAsync();
#endif
                foreach (var item in files)
                {
                    if (Locator.MediaPlaybackViewModel.ContinueIndexing != null)
                        // We prevent indexing this file and upcoming files when a video is playing
                    {
                        await Locator.MediaPlaybackViewModel.ContinueIndexing.Task;
                        Locator.MediaPlaybackViewModel.ContinueIndexing = null;
                    }
                    await DiscoverTrackItemOrWaitAsync(item);
                }
            }
            catch(Exception e)
            {
                LogHelper.Log(StringsHelper.ExceptionToString(e));
            }
        }

        public static async Task GetAllMusicFolders()
        {
            try
            {
#if WINDOWS_APP
                StorageLibrary musicLibrary = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);
                foreach (StorageFolder storageFolder in musicLibrary.Folders)
                {
                    await CreateDatabaseFromMusicFolder(storageFolder);
                }
#else
                StorageFolder musicLibrary = KnownFolders.MusicLibrary;
                LogHelper.Log("Searching for music from Phone MusicLibrary ...");
                await CreateDatabaseFromMusicFolder(musicLibrary);
#endif
            }
            catch (Exception e)
            {
                LogHelper.Log(StringsHelper.ExceptionToString(e));
            }
        }

        private static async Task CreateDatabaseFromMusicFolder(StorageFolder musicFolder)
        {
            try
            {
                if (Locator.MediaPlaybackViewModel.ContinueIndexing != null) // We prevent indexing new folder and files recursively when a Video is playing
                {
                    await Locator.MediaPlaybackViewModel.ContinueIndexing.Task;
                    Locator.MediaPlaybackViewModel.ContinueIndexing = null;
                }
                if (musicFolder.Name != Strings.PodcastFolderName)
                {
                    var folders = await musicFolder.GetFoldersAsync();
                    if (folders.Any())
                    {
                        foreach (var storageFolder in folders)
                        {
                            await CreateDatabaseFromMusicFolder(storageFolder);
                        }
                    }
                    var folderFiles = await musicFolder.GetFilesAsync();
                    if (folderFiles != null && folderFiles.Any())
                    {
                        foreach (var storageFile in folderFiles)
                        {
                            await DiscoverTrackItemOrWaitAsync(storageFile);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Log(StringsHelper.ExceptionToString(e));
            }
        }

        private static async Task CreateDatabaseFromMusicFile(StorageFile item)
        {
            try
            {
                if (!VLCFileExtensions.AudioExtensions.Contains(item.FileType.ToLower())) return;
                var media = Locator.VLCService.GetMediaFromPath(item.Path);
                MediaProperties mP = Locator.VLCService.GetMusicProperties(media);
                if (mP == null)
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
                    ArtistItem artist = Locator.MusicLibraryVM._artistDatabase.LoadViaArtistName(string.IsNullOrEmpty(albumArtistName) ? artistName : albumArtistName);
                    if (artist == null)
                    {
                        artist = new ArtistItem();
                        artist.Name = string.IsNullOrEmpty(albumArtistName) ? artistName : albumArtistName;
                        artist.PlayCount = 0;
                        await Locator.MusicLibraryVM._artistDatabase.Add(artist);
                        await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                        {
                            AddArtist(artist);
                        });
                    }

                    var albumName = mP.Album?.Trim();
                    var albumYear = mP.Year;
                    AlbumItem album = await Locator.MusicLibraryVM._albumDatabase.LoadAlbumViaName(artist.Id, albumName);
                    if (album == null)
                    {
                        var albumUrl = Locator.VLCService.GetAlbumUrl(media);
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
                        await Locator.MusicLibraryVM._albumDatabase.Add(album);
                        await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                        {
                            var artistFromCollection = Locator.MusicLibraryVM.Artists.FirstOrDefault(x => x.Id == album.ArtistId);
                            AddAlbum(album, artistFromCollection);
                            Locator.MainVM.InformationText = string.Format(Strings.AlbumsFound, Locator.MusicLibraryVM.Albums.Count);
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
                    await Locator.MusicLibraryVM._trackDatabase.Add(track);
                    await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => AddTrack(track));
                }
            }
            catch (Exception e)
            {
                LogHelper.Log(StringsHelper.ExceptionToString(e));
            }
        }

        public static async void AddArtist(ArtistItem artist)
        {
            Locator.MusicLibraryVM.Artists.Add(artist);
            if (Locator.MusicLibraryVM.Artists.Count < 3)
            {
                await artist.LoadPicture();
                if (artist.IsPictureLoaded)
                {
                    Locator.Slideshow.AddImg(artist.Picture);
                }
            }
        }

        public static void AddAlbum(AlbumItem album, ArtistItem artist)
        {
            artist?.Albums.Add(album);
            if (Locator.NavigationService.CurrentPage == VLCPage.MainPageMusic && Locator.MusicLibraryVM.MusicView == MusicView.Albums)
            {
                Task.Run(() => InsertIntoGroupAlbum(album));
            }
            Locator.MusicLibraryVM.Albums.Add(album);
        }

        public static void AddTrack(TrackItem track)
        {
            Locator.MusicLibraryVM.Tracks.Add(track);
        }

        public static async Task PopulateTracks(this AlbumItem album)
        {
            try
            {
                var tracks = Locator.MusicLibraryVM._trackDatabase.LoadTracksByAlbumId(album.Id).ToObservable();
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    album.Tracks = tracks;
                });
            }
            catch (Exception e)
            {
                LogHelper.Log(StringsHelper.ExceptionToString(e));
            }
        }

        public static async Task PopulateAlbums(this ArtistItem artist)
        {
            try
            {
                var albums = await Locator.MusicLibraryVM._albumDatabase.LoadAlbumsFromId(artist.Id).ToObservableAsync();
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    artist.Albums = albums;
                });
            }
            catch (Exception e)
            {
                LogHelper.Log(StringsHelper.ExceptionToString(e));
            }
        }

        public static async Task PopulateTracksByAlbum(this ArtistItem artist)
        {
            try
            {
                var tracks = await Locator.MusicLibraryVM._trackDatabase.LoadTracksByArtistId(artist.Id);
                var groupedTracks = tracks.GroupBy(x => new Tuple<string, string, int>(x.AlbumName, x.Thumbnail, x.AlbumId));
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    artist.TracksGroupedByAlbum = groupedTracks;
                });
            }
            catch (Exception e)
            {
                LogHelper.Log(StringsHelper.ExceptionToString(e));
            }
        }

        public static async Task LoadFavoriteRandomAlbums()
        {
            try
            {
                var howManyAlbumsToFill = 3;
                if (Locator.MusicLibraryVM.RandomAlbums != null && Locator.MusicLibraryVM.RandomAlbums.Any()) return;
                ObservableCollection<AlbumItem> favAlbums = await Locator.MusicLibraryVM._albumDatabase.LoadAlbums(x => x.Favorite).ToObservableAsync();
                if (favAlbums != null && favAlbums.Any())
                {
                    await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        Locator.MusicLibraryVM.FavoriteAlbums = favAlbums;
                        Locator.MusicLibraryVM.RandomAlbums = favAlbums.Count > howManyAlbumsToFill ? favAlbums.Take(howManyAlbumsToFill).ToObservable() : favAlbums;
                        howManyAlbumsToFill -= Locator.MusicLibraryVM.RandomAlbums.Count;
                    });
                }
                if (howManyAlbumsToFill == 0) return;
                ObservableCollection<AlbumItem> nonfavAlbums = await Locator.MusicLibraryVM._albumDatabase.LoadAlbums(x => x.Favorite == false).ToObservableAsync();
                if (nonfavAlbums != null && nonfavAlbums.Any())
                {
                    await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        int total = (nonfavAlbums.Count > howManyAlbumsToFill) ? howManyAlbumsToFill : nonfavAlbums.Count - 1;
                        for (int i = 0; i < total; i++)
                        {
                            Locator.MusicLibraryVM.RandomAlbums.Add(nonfavAlbums[i]);
                        }
                    });
                }
            }
            catch (Exception)
            {
                LogHelper.Log("Error selecting random albums.");
            }
        }

        public static async Task OrderArtists(IEnumerable<ArtistItem> artists)
        {
            var orderedArtists = artists.OrderBy(x => x.Name);
            var groupedArtists = orderedArtists.GroupBy(x => string.IsNullOrEmpty(x.Name) ? Strings.UnknownString : (char.IsLetter(x.Name.ElementAt(0)) ? x.Name.ToUpper().ElementAt(0).ToString() : Strings.UnknownString));

            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Locator.MusicLibraryVM.Artists = new ObservableCollection<ArtistItem>(orderedArtists);
                Locator.MusicLibraryVM.GroupedArtists = groupedArtists;
            });
        }

        public static void OrderAlbums()
        {
            Task.Run(async () =>
            {
                var groupedAlbums = new ObservableCollection<GroupItemList<AlbumItem>>();
                if (Locator.SettingsVM.AlbumsOrderType == OrderType.ByArtist)
                {
                    if (Locator.SettingsVM.AlbumsOrderListing == OrderListing.Ascending)
                    {
                        var groupQuery = from album in Locator.MusicLibraryVM.Albums
                                         orderby album.Artist
                                         group album by Strings.HumanizedArtistName(album.Artist) into a
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
                    else if (Locator.SettingsVM.AlbumsOrderListing == OrderListing.Descending)
                    {
                        var groupQuery = from album in Locator.MusicLibraryVM.Albums
                                         orderby album.Artist descending
                                         group album by Strings.HumanizedArtistName(album.Artist) into a
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
                else if (Locator.SettingsVM.AlbumsOrderType == OrderType.ByDate)
                {
                    if (Locator.SettingsVM.AlbumsOrderListing == OrderListing.Ascending)
                    {
                        var groupQuery = from album in Locator.MusicLibraryVM.Albums
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
                    else if (Locator.SettingsVM.AlbumsOrderListing == OrderListing.Descending)
                    {
                        var groupQuery = from album in Locator.MusicLibraryVM.Albums
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
                else if (Locator.SettingsVM.AlbumsOrderType == OrderType.ByAlbum)
                {
                    if (Locator.SettingsVM.AlbumsOrderListing == OrderListing.Ascending)
                    {
                        var groupQuery = from album in Locator.MusicLibraryVM.Albums
                                         orderby album.Name
                                         group album by Strings.HumanizedAlbumFirstLetter(album.Name) into a
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
                    else if (Locator.SettingsVM.AlbumsOrderListing == OrderListing.Descending)
                    {
                        var groupQuery = from album in Locator.MusicLibraryVM.Albums
                                         orderby album.Name descending
                                         group album by Strings.HumanizedAlbumFirstLetter(album.Name) into a
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

                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Locator.MusicLibraryVM.GroupedAlbums = groupedAlbums);
            });
        }

        static async Task InsertIntoGroupAlbum(AlbumItem album)
        {
            if (Locator.SettingsVM.AlbumsOrderType == OrderType.ByArtist)
            {
                var artist = Locator.MusicLibraryVM.GroupedAlbums.FirstOrDefault(x => x.Key == Strings.HumanizedArtistName(album.Artist));
                if (artist == null)
                {
                    artist = new GroupItemList<AlbumItem>(album) { Key = Strings.HumanizedArtistName(album.Artist) };
                    int i = Locator.MusicLibraryVM.GroupedAlbums.IndexOf(Locator.MusicLibraryVM.GroupedAlbums.LastOrDefault(x => string.Compare(x.Key, artist.Key) < 0));
                    i++;
                    await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => Locator.MusicLibraryVM.GroupedAlbums.Insert(i, artist));
                }
                else await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => artist.Add(album));
            }
            else if (Locator.SettingsVM.AlbumsOrderType == OrderType.ByDate)
            {
                var year = Locator.MusicLibraryVM.GroupedAlbums.FirstOrDefault(x => x.Key == Strings.HumanizedYear(album.Year));
                if (year == null)
                {
                    var newyear = new GroupItemList<AlbumItem>(album) { Key = Strings.HumanizedYear(album.Year) };
                    int i = Locator.MusicLibraryVM.GroupedAlbums.IndexOf(Locator.MusicLibraryVM.GroupedAlbums.LastOrDefault(x => string.Compare(x.Key, newyear.Key) < 0));
                    i++;
                    await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => Locator.MusicLibraryVM.GroupedAlbums.Insert(i, newyear));
                }
                else await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => year.Add(album));
            }
            else if (Locator.SettingsVM.AlbumsOrderType == OrderType.ByAlbum)
            {
                var firstChar = Locator.MusicLibraryVM.GroupedAlbums.FirstOrDefault(x => x.Key == Strings.HumanizedAlbumFirstLetter(album.Name));
                if (firstChar == null)
                {
                    var newChar = new GroupItemList<AlbumItem>(album) { Key = Strings.HumanizedAlbumFirstLetter(album.Name) };
                    int i = Locator.MusicLibraryVM.GroupedAlbums.IndexOf(Locator.MusicLibraryVM.GroupedAlbums.LastOrDefault(x => string.Compare(x.Key, newChar.Key) < 0));
                    i++;
                    await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => Locator.MusicLibraryVM.GroupedAlbums.Insert(i, newChar));
                }
                else await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => firstChar.Add(album));
            }
        }

        public static async Task AddNewPlaylist(string trackCollectionName)
        {
            if (string.IsNullOrEmpty(trackCollectionName)) return;
            TrackCollection trackCollection = null;
            trackCollection = await Locator.MusicLibraryVM.TrackCollectionRepository.LoadFromName(trackCollectionName);
            if (trackCollection != null)
            {
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ToastHelper.Basic(Strings.PlaylistAlreadyExists));
            }
            else
            {
                trackCollection = new TrackCollection();
                trackCollection.Name = trackCollectionName;
                await Locator.MusicLibraryVM.TrackCollectionRepository.Add(trackCollection);
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Locator.MusicLibraryVM.TrackCollections.Add(trackCollection));
            }
        }

        public static Task DeletePlaylistTrack(TrackItem track, TrackCollection trackCollection)
        {
            return Locator.MusicLibraryVM.TracklistItemRepository.Remove(track.Id, trackCollection.Id);
        }

        public static async Task DeletePlaylist(TrackCollection trackCollection)
        {
            await Locator.MusicLibraryVM.TrackCollectionRepository.Remove(trackCollection);
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Locator.MusicLibraryVM.TrackCollections.Remove(trackCollection);
                Locator.MusicLibraryVM.CurrentTrackCollection = null;
            });
        }

        public static async Task AddToPlaylist(TrackItem trackItem, bool displayToastNotif = true)
        {
            if (Locator.MusicLibraryVM.CurrentTrackCollection == null) return;
            if (Locator.MusicLibraryVM.CurrentTrackCollection.Playlist.Contains(trackItem))
            {
                ToastHelper.Basic(Strings.TrackAlreadyExistsInPlaylist);
                return;
            }
            Locator.MusicLibraryVM.CurrentTrackCollection.Playlist.Add(trackItem);
            await Locator.MusicLibraryVM.TracklistItemRepository.Add(new TracklistItem()
            {
                TrackId = trackItem.Id,
                TrackCollectionId = Locator.MusicLibraryVM.CurrentTrackCollection.Id,
            });
            if (displayToastNotif)
                ToastHelper.Basic(string.Format(Strings.TrackAddedToYourPlaylist, trackItem.Name));
        }

        public static async Task AddToPlaylist(AlbumItem albumItem)
        {
            if (Locator.MusicLibraryVM.CurrentTrackCollection == null) return;
            var playlistId = Locator.MusicLibraryVM.CurrentTrackCollection.Id;
            foreach (TrackItem trackItem in albumItem.Tracks)
            {
                Locator.MusicLibraryVM.CurrentTrackCollection.Playlist.Add(trackItem);
                await Locator.MusicLibraryVM.TracklistItemRepository.Add(new TracklistItem()
                {
                    TrackId = trackItem.Id,
                    TrackCollectionId = playlistId,
                });
            }
            ToastHelper.Basic(string.Format(Strings.TrackAddedToYourPlaylist, albumItem.Name));
        }

        public static async Task UpdateTrackCollection(TrackCollection trackCollection)
        {
            var loadTracks = await Locator.MusicLibraryVM.TracklistItemRepository.LoadTracks(trackCollection);
            foreach (TracklistItem tracklistItem in loadTracks)
            {
                await Locator.MusicLibraryVM.TracklistItemRepository.Remove(tracklistItem);
            }
            foreach (TrackItem trackItem in trackCollection.Playlist)
            {
                var trackListItem = new TracklistItem { TrackId = trackItem.Id, TrackCollectionId = trackCollection.Id };
                await Locator.MusicLibraryVM.TracklistItemRepository.Add(trackListItem);
            }
        }

        public static async Task RemoveTrackFromCollectionAndDatabase(TrackItem trackItem)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                try
                {
                    Locator.MusicLibraryVM._trackDatabase.Remove(Locator.MusicLibraryVM.Tracks.FirstOrDefault(x => x.Path == trackItem.Path));
                    Locator.MusicLibraryVM.Tracks.Remove(Locator.MusicLibraryVM.Tracks.FirstOrDefault(x => x.Path == trackItem.Path));
                    var album = Locator.MusicLibraryVM.Albums.FirstOrDefault(x => x.Id == trackItem.AlbumId);
                    album?.Tracks.Remove(album.Tracks.FirstOrDefault(x => x.Path == trackItem.Path));

                    var artist = Locator.MusicLibraryVM.Artists.FirstOrDefault(x => x.Id == trackItem.ArtistId);
                    var artistalbum = artist?.Albums.FirstOrDefault(x => x.Id == trackItem.AlbumId);
                    artistalbum?.Tracks.Remove(artistalbum.Tracks.FirstOrDefault(x => x.Path == trackItem.Path));
                    if (album.Tracks.Count == 0)
                    {
                        // We should remove the album as a whole
                        Locator.MusicLibraryVM.Albums.Remove(album);
                        Locator.MusicLibraryVM._albumDatabase.Remove(album);
                        artist.Albums.Remove(artistalbum);
                    }
                    var playingTrack = Locator.MediaPlaybackViewModel.TrackCollection.Playlist.FirstOrDefault(x => x.Id == trackItem.Id);
                    if (playingTrack != null) Locator.MediaPlaybackViewModel.TrackCollection.Playlist.Remove(playingTrack);
                }
                catch
                {
                }
            });
        }

        public static bool AddAlbumToPlaylist(object args)
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

        public async static Task<TrackItem> GetTrackItemFromFile(StorageFile track, string token = null)
        {
            //TODO: Warning, is it safe to consider this a good idea?
            var trackItem = await Locator.MusicLibraryVM._trackDatabase.LoadTrackByPath(track.Path);
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
    }
}
