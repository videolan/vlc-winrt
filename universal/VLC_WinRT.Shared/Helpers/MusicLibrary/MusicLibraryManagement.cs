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

namespace VLC_WinRT.Helpers.MusicLibrary
{
    public static class MusicLibraryManagement
    {
        static readonly SemaphoreSlim AlbumCoverFetcherSemaphoreSlim = new SemaphoreSlim(2);
        static readonly SemaphoreSlim ArtistPicFetcherSemaphoreSlim = new SemaphoreSlim(2);
        static readonly SemaphoreSlim TrackItemDiscovererSemaphoreSlim = new SemaphoreSlim(1);

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
                var groupedTracks = tracks.GroupBy(x => string.IsNullOrEmpty(x.Name) ? Strings.UnknownChar : (char.IsLetter(x.Name.ElementAt(0)) ? x.Name.ToLower().ElementAt(0) : Strings.UnknownChar));

                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Locator.MusicLibraryVM.Tracks = tracks;
                    Locator.MusicLibraryVM.Albums = new ObservableCollection<AlbumItem>(orderedAlbums);
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

        public static Task DoRoutineMusicLibraryCheck()
        {
#if WINDOWS_PHONE_APP
            return GetAllMusicFolders();
#else
            return PerformMusicLibraryIndexing();
#endif
        }


#if WINDOWS_PHONE_APP
#else
        /// <summary>
        /// This method is Windows-only since the crappy WP OS throws a 
        /// NotImplementedexception when calling QueryOptions, CreateFileQueryWithOptions
        /// </summary>
        /// <returns></returns>
        public static async Task PerformMusicLibraryIndexing()
        {
            var queryOptions = new QueryOptions { FolderDepth = FolderDepth.Deep };
            foreach (var type in VLCFileExtensions.Supported)
                queryOptions.FileTypeFilter.Add(type);
            var fileQueryResult = KnownFolders.MusicLibrary.CreateFileQueryWithOptions(queryOptions);
            var files = await fileQueryResult.GetFilesAsync();
            foreach (var item in files)
            {
                await DiscoverTrackItemOrWaitAsync(item);
            }
        }
#endif

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
                ExceptionHelper.CreateMemorizedException("MusicLibraryManagement.GetAllMusicFolders", e);
            }
        }

        private static async Task CreateDatabaseFromMusicFolder(StorageFolder musicFolder)
        {
            try
            {
                if (Locator.MediaPlaybackViewModel.ContinueIndexing != null)
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
                ExceptionHelper.CreateMemorizedException("MusicLibraryManagement.CreateDatabaseFromMusicFolder", e);
            }
        }

        private static async Task CreateDatabaseFromMusicFile(StorageFile item)
        {
            try
            {
                if (!VLCFileExtensions.AudioExtensions.Contains(item.FileType.ToLower())) return;
                var media = Locator.VLCService.GetMediaFromPath(item.Path);
                MediaProperties mP = Locator.VLCService.GetMusicProperties(media);
                if (mP != null)
                {
                    var artistName = mP.Artist;
                    ArtistItem artist = Locator.MusicLibraryVM._artistDatabase.LoadViaArtistName(artistName);
                    if (artist == null)
                    {
                        artist = new ArtistItem();
                        artist.Name = string.IsNullOrEmpty(artistName) ? string.Empty : artistName;
                        await Locator.MusicLibraryVM._artistDatabase.Add(artist);
                        await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                        {
                            AddArtist(artist);
                        });
                    }

                    var albumName = mP.Album;
                    var albumYear = mP.Year;
                    AlbumItem album = await Locator.MusicLibraryVM._albumDatabase.LoadAlbumViaName(artist.Id, albumName);
                    if (album == null)
                    {
                        var albumUrl = Locator.VLCService.GetAlbumUrl(media);
                        var hasVLCFoundCover = false;
                        if (!string.IsNullOrEmpty(albumUrl) && albumUrl.StartsWith("file://"))
                        {
                            // The Uri will be like
                            // ms-appdata:///local/vlc/art/artistalbum/30 Seconds To Mars/B-sides & Rarities/art.jpg
                            hasVLCFoundCover = true;
                        }

                        album = new AlbumItem
                        {
                            Name = string.IsNullOrEmpty(albumName) ? string.Empty : albumName,
                            Artist = string.IsNullOrEmpty(artistName) ? string.Empty : artistName,
                            ArtistId = artist.Id,
                            Favorite = false,
                            Year = albumYear,
                            IsPictureLoaded = hasVLCFoundCover,
                            IsVLCCover = hasVLCFoundCover,
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
                        ArtistName = artist.Name,
                        CurrentPosition = 0,
                        Duration = mP.Duration,
                        Favorite = false,
                        Name = string.IsNullOrEmpty(mP.Title) ? item.DisplayName : mP.Title,
                        Path = item.Path,
                        Index = mP.Tracknumber,
                    };
                    await Locator.MusicLibraryVM._trackDatabase.Add(track);
                    await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => AddTrack(track));
                }
            }
            catch (Exception e)
            {
                ExceptionHelper.CreateMemorizedException("MusicLibraryManagement.CreateDatabaseFromMusicFile", e);
            }
        }

        public static void AddArtist(ArtistItem artist)
        {
            Locator.MusicLibraryVM.Artists.Add(artist);
        }

        public static void AddAlbum(AlbumItem album, ArtistItem artist)
        {
            artist?.Albums.Add(album);
            InsertIntoGroupAlbum(album);
            Locator.MusicLibraryVM.Albums.Add(album);
        }

        public static void AddTrack(TrackItem track)
        {
            Locator.MusicLibraryVM.Tracks.Add(track);
        }

        public static async Task PopulateTracks(this AlbumItem album)
        {
            var tracks = await Locator.MusicLibraryVM._trackDatabase.LoadTracksByAlbumId(album.Id);
            var orderedTracks = tracks.OrderBy(x => x.Index).ToObservable();
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                album.Tracks = orderedTracks;
            });
        }

        public static async Task PopulateAlbums(this ArtistItem artist)
        {
            var albums = await Locator.MusicLibraryVM._albumDatabase.LoadAlbumsFromId(artist.Id);
            var orderedAlbums = albums.OrderBy(x => x.Name).ToObservable();
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                artist.Albums = orderedAlbums;
            });
        }

        public static async Task PopulateTracksByAlbum(this ArtistItem artist)
        {
            var t = new Tuple<string, string>("", "");
            var tracks = await Locator.MusicLibraryVM._trackDatabase.LoadTracksByArtistId(artist.Id);
            var groupedTracks = tracks.GroupBy(x => new Tuple<string, string>(x.AlbumName, x.Thumbnail));
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                artist.TracksGroupedByAlbum = groupedTracks;
            });
        }

        public static async Task LoadFavoriteRandomAlbums()
        {
            try
            {
                int howManyAlbumsToFill = await HowManyAlbumsToDisplayWithTwoRows();
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

        public static async Task<int> HowManyAlbumsToDisplayWithTwoRows()
        {
#if WINDOWS_APP
            var width = 0.0;
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                width = Window.Current.Bounds.Width;
            });
            // an album is 220 pixels wide
            width -= (int)Locator.MusicLibraryVM.SidebarState;
            var nbAlbumsPerRow = width / 220;
            return (int)nbAlbumsPerRow * 2;
#else
            return 6;
#endif
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
                
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, ()=> Locator.MusicLibraryVM.GroupedAlbums = groupedAlbums);
            });
        }

        static void InsertIntoGroupAlbum(AlbumItem album)
        {
            if (Locator.NavigationService.CurrentPage != VLCPage.MainPageMusic || Locator.MusicLibraryVM.MusicView != MusicView.Albums) return;
            Task.Run(async () =>
            {
                if (Locator.SettingsVM.AlbumsOrderType == OrderType.ByArtist)
                {
                    var artist = Locator.MusicLibraryVM.GroupedAlbums.FirstOrDefault(x => x.Key == Strings.HumanizedArtistName(album.Artist));
                    if (artist == null)
                    {
                        var newArtist = new GroupItemList<AlbumItem>(album) { Key = Strings.HumanizedArtistName(album.Artist) };
                        int i = Locator.MusicLibraryVM.GroupedAlbums.IndexOf(Locator.MusicLibraryVM.GroupedAlbums.LastOrDefault(x => string.Compare(x.Key, newArtist.Key) < 0));
                        i++;
                        await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => Locator.MusicLibraryVM.GroupedAlbums.Insert(i, newArtist));
                    }
                    else
                    {
                        artist.Add(album);
                    }
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
                    else
                    {
                        year.Add(album);
                    }
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
                    else
                    {
                        firstChar.Add(album);
                    }
                }
            });
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

        public static void AddAlbumToPlaylist(object args)
        {
            if (Locator.MusicLibraryVM.CurrentTrackCollection == null)
            {
#if WINDOWS_PHONE_APP
                if(args is ContentDialogButtonClickEventArgs)
                    ((ContentDialogButtonClickEventArgs)args).Cancel = true;
#endif
                ToastHelper.Basic(Strings.HaveToSelectPlaylist, false, "selectplaylist");
                return;
            }
#if WINDOWS_APP
            var flyout = ((AddAlbumToPlaylistBase)args).GetFirstDescendantOfType<SettingsFlyout>();
            flyout?.Hide();
#endif
            Locator.MusicLibraryVM.AddToPlaylistCommand.Execute(Locator.MusicLibraryVM.CurrentAlbum);
        }

        public async static Task<TrackItem> GetTrackItemFromFile(StorageFile track)
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
                File = track
            };
            return trackItem;
        }
    }
}
