using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using XboxMusicLibrary.Models;
#if WINDOWS_PHONE_APP
using Windows.Phone.ApplicationModel;
#endif
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using VLC_WINRT_APP.Helpers.MusicLibrary.Deezer;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MusicVM;

namespace VLC_WINRT_APP.Helpers.MusicLibrary
{
    public static class MusicLibraryManagement
    {
        public static async Task LoadFromSQL()
        {
            try
            {
                LogHelper.Log("Loading artists from MusicDB ...");
                var artists = await MusicLibraryVM._artistDataRepository.Load();
                LogHelper.Log("Found " + artists.Count + " artists from MusicDB");
                var orderedArtists = artists.OrderBy(x => x.Name);
                var tracks = await MusicLibraryVM._trackDataRepository.LoadTracks();
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    foreach (var artist in orderedArtists)
                    {
                        Locator.MusicLibraryVM.Artists.Add(artist);
                    }
                    Locator.MusicLibraryVM.Tracks = tracks;
                });

                var trackColl = await MusicLibraryVM.TrackCollectionRepository.LoadTrackCollections();
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Locator.MusicLibraryVM.TrackCollections = trackColl;
                });

                foreach (TrackCollection trackCollection in Locator.MusicLibraryVM.TrackCollections)
                {
                    ObservableCollection<TracklistItem> observableCollection =
                        await MusicLibraryVM.TracklistItemRepository.LoadTracks(trackCollection);
                    foreach (TracklistItem tracklistItem in observableCollection)
                    {
                        TrackItem item = await MusicLibraryVM._trackDataRepository.LoadTrack(tracklistItem.TrackId);
                        trackCollection.Playlist.Add(item);
                    }
                }
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Locator.MusicLibraryVM.AlphaGroupedTracks =
                        Locator.MusicLibraryVM.Tracks.OrderBy(
                            x =>
                                x.Name != null
                                    ? x.Name
                                        .ElementAt(0)
                                    : '\0')
                            .GroupBy(
                                x =>
                                    x.Name != null
                                        ? (char
                                            .IsLetter
                                            (x.Name
                                                .ToLower
                                                ()
                                                .ElementAt
                                                (0))
                                            ? x.Name
                                                .ToLower
                                                ()
                                                .ElementAt
                                                (0)
                                            : '#')
                                        : '\0');
                });
            }
            catch (Exception)
            {
                LogHelper.Log("Error getting database.");
            }
        }

        public static async Task GetAllMusicFolders(bool routineCheck = false)
        {
            try
            {
#if WINDOWS_APP
                StorageLibrary musicLibrary = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);
                foreach (StorageFolder storageFolder in musicLibrary.Folders)
                {
                    await CreateDatabaseFromMusicFolder(files, storageFolder, routineCheck);
                }
#else
                StorageFolder musicLibrary = KnownFolders.MusicLibrary;
                LogHelper.Log("Searching for music from Phone MusicLibrary ...");
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    StatusBarHelper.UpdateTitle("Searching for music"));
                await CreateDatabaseFromMusicFolder(musicLibrary, routineCheck);
#endif
            }
            catch (Exception e)
            {
                ExceptionHelper.CreateMemorizedException("MusicLibraryManagement.GetAllMusicFolders", e);
            }
        }

        private static async Task<List<StorageFile>> CreateDatabaseFromMusicFolder(StorageFolder musicFolder, bool routineCheck = false)
        {
            try
            {
                var folders = await musicFolder.GetFoldersAsync();
                if (folders.Any())
                {
                    foreach (var storageFolder in folders)
                    {
                        await CreateDatabaseFromMusicFolder( storageFolder, routineCheck);
                    }
                }
                var folderFiles = await musicFolder.GetFilesAsync();
                if (folderFiles != null && folderFiles.Any())
                {
#if WINDOWS_PHONE_APP
                    if (!routineCheck)
                    {
                        await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            StatusBarHelper.UpdateTitle("Found " + folderFiles.Count + " files"));
                    }
#endif
                    foreach (var storageFile in folderFiles)
                    {
                        if (!routineCheck)
                        {
                            await CreateDatabaseFromMusicFile(storageFile);
                        }
                        else
                        {
                            if (!await MusicLibraryVM._trackDataRepository.DoesTrackExist(storageFile.Path))
                            {
                                await CreateDatabaseFromMusicFile(storageFile);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionHelper.CreateMemorizedException("MusicLibraryManagement.CreateDatabaseFromMusicFolder", e);
            }
            return null;
        }

        private static async Task CreateDatabaseFromMusicFile(StorageFile item)
        {
            try
            {
                if (!VLCFileExtensions.AudioExtensions.Contains(item.FileType.ToLower())) return;
                LogHelper.Log("Music indexation: found music file " + item.Path);
                MusicProperties properties = await item.Properties.GetMusicPropertiesAsync();
                if (properties != null)
                {
                    ArtistItem artist = await MusicLibraryVM._artistDataRepository.LoadViaArtistName(properties.Artist);
                    if (artist == null)
                    {
                        artist = new ArtistItem();
                        artist.Name = string.IsNullOrEmpty(properties.Artist) ? "Unknown artist" : properties.Artist;
                        await MusicLibraryVM._artistDataRepository.Add(artist);
                        await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                        {
                            Locator.MusicLibraryVM.Artists.Add(artist);
                        });
                    }

                    AlbumItem album =
                        await MusicLibraryVM._albumDataRepository.LoadAlbumViaName(artist.Id, properties.Album);
                    if (album == null)
                    {
                        album = new AlbumItem
                        {
                            Name = string.IsNullOrEmpty(properties.Album) ? "Unknown album" : properties.Album,
                            Artist = string.IsNullOrEmpty(properties.Artist) ? "Unknown artist" : properties.Artist,
                            ArtistId = artist.Id,
                            Favorite = false,
                            Year = properties.Year
                        };
                        await MusicLibraryVM._albumDataRepository.Add(album);
                        await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                        {
                            var artistFromCollection =
                                Locator.MusicLibraryVM.Artists.FirstOrDefault(x => x.Id == album.ArtistId);
                            if (artistFromCollection != null) artistFromCollection.Albums.Add(album);
                            Locator.MusicLibraryVM.CurrentIndexingStatus = "Found album " + album.Name;
#if WINDOWS_PHONE_APP
                            StatusBarHelper.UpdateTitle("Found " + album.Name);
#endif
                            Locator.MusicLibraryVM.Albums.Add(album);
                        });
                    }

                    TrackItem track = new TrackItem()
                    {
                        AlbumId = album.Id,
                        AlbumName = album.Name,
                        ArtistId = artist.Id,
                        ArtistName = artist.Name,
                        CurrentPosition = 0,
                        Duration = properties.Duration,
                        Favorite = false,
                        Name = string.IsNullOrEmpty(properties.Title) ? item.DisplayName : properties.Title,
                        Path = item.Path,
                        Index = (int) properties.TrackNumber,
                        IsFromSandbox = true
                    };
                    await MusicLibraryVM._trackDataRepository.Add(track);
                }
            }
            catch (Exception e)
            {
                ExceptionHelper.CreateMemorizedException("MusicLibraryManagement.CreateDatabaseFromMusicFile", e);
            }
        }

        public static async Task GetTracks(this AlbumItem album)
        {
            var tracks = await MusicLibraryVM._trackDataRepository.LoadTracksByAlbumId(album.Id);
            var orderedTracks = tracks.OrderBy(x => x.Index);
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                album.Tracks = new ObservableCollection<TrackItem>();
                foreach (var track in orderedTracks)
                {
                    album.Tracks.Add(track);
                }
            });
        }

        public static async Task GetAlbums(this ArtistItem artist)
        {
            var albums = await MusicLibraryVM._albumDataRepository.LoadAlbumsFromId(artist.Id);
            var orderedAlbums = albums.OrderBy(x => x.Name);
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                artist.Albums = new ObservableCollection<AlbumItem>();
                foreach (AlbumItem album in orderedAlbums)
                {
                    artist.Albums.Add(album);
                }
            });
        }

        public static async Task LoadFavoriteRandomAlbums()
        {
            try
            {
                if (Locator.MusicLibraryVM.RandomAlbums != null && Locator.MusicLibraryVM.RandomAlbums.Any()) return;
                ObservableCollection<AlbumItem> favAlbums = await MusicLibraryVM._albumDataRepository.LoadAlbums(x => x.Favorite);
                if (favAlbums != null && favAlbums.Any())
                {
                    await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        Locator.MusicLibraryVM.FavoriteAlbums = favAlbums;
                        Locator.MusicLibraryVM.RandomAlbums = new ObservableCollection<AlbumItem>(favAlbums.Take(3));
                    });
                }
                ObservableCollection<AlbumItem> nonfavAlbums = await MusicLibraryVM._albumDataRepository.LoadAlbums(x => x.Favorite == false);
                if (nonfavAlbums != null && nonfavAlbums.Any())
                {
                    if (Locator.MusicLibraryVM.RandomAlbums != null && Locator.MusicLibraryVM.RandomAlbums.Count > 6)
                        return;
                    await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        int howManyAlbums = HowManyAlbumsToDisplayWithTwoRows();
                        int total = ((howManyAlbums < nonfavAlbums.Count) ? howManyAlbums : nonfavAlbums.Count - 1);
                        for (int i = 0; i < total; i++)
                        {
                            Locator.MusicLibraryVM
                                .RandomAlbums.Add(
                                    nonfavAlbums[i]);
                        }
                    });
                }
            }
            catch (Exception)
            {
                LogHelper.Log("Error selecting random albums.");
            }
        }

        public static int HowManyAlbumsToDisplayWithTwoRows()
        {
            var width = Window.Current.Bounds.Width;
            // an album is 220 pixels wide
            width -= (int)Locator.MusicLibraryVM.SidebarState;
            var nbAlbumsPerRow = width / 220;
            return (int)nbAlbumsPerRow * 2;
        }

        public static void OrderAlbums()
        {
            if (Locator.SettingsVM.AlbumsOrderType == OrderType.ByArtist)
            {
                if (Locator.SettingsVM.AlbumsOrderListing == OrderListing.Ascending)
                {
                    Locator.MusicLibraryVM.Albums =
                        new ObservableCollection<AlbumItem>(Locator.MusicLibraryVM.Albums.OrderBy(x => x.Artist));
                }
                else if (Locator.SettingsVM.AlbumsOrderListing == OrderListing.Descending)
                {
                    Locator.MusicLibraryVM.Albums = new ObservableCollection<AlbumItem>(Locator.MusicLibraryVM.Albums.OrderByDescending(x => x.Artist));
                }
            }
            else if (Locator.SettingsVM.AlbumsOrderType == OrderType.ByDate)
            {
                if (Locator.SettingsVM.AlbumsOrderListing == OrderListing.Ascending)
                {
                    Locator.MusicLibraryVM.Albums =
                        new ObservableCollection<AlbumItem>(Locator.MusicLibraryVM.Albums.OrderBy(x => x.Year));
                }
                else if (Locator.SettingsVM.AlbumsOrderListing == OrderListing.Descending)
                {
                    Locator.MusicLibraryVM.Albums = new ObservableCollection<AlbumItem>(Locator.MusicLibraryVM.Albums.OrderByDescending(x => x.Year));
                }
            }
        }

        public static async Task AddNewPlaylist(string trackCollectionName)
        {
            TrackCollection trackCollection = null;
            trackCollection = await MusicLibraryVM.TrackCollectionRepository.LoadFromName(trackCollectionName);
            if (trackCollection != null)
            {
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await
                        new MessageDialog(
                            "A playlist with this name already exists", "Sorry ...")
                            .ShowAsync();
                });
            }
            else
            {
                trackCollection = new TrackCollection();
                trackCollection.Name = trackCollectionName;
                await MusicLibraryVM.TrackCollectionRepository.Add(trackCollection);
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                Locator.MusicLibraryVM.TrackCollections.Add(trackCollection));
            }
        }

        public static async Task DeletePlaylist(TrackCollection trackCollection)
        {
            await MusicLibraryVM.TrackCollectionRepository.Remove(trackCollection);
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Locator.MusicLibraryVM.TrackCollections.Remove(trackCollection));
        }

        public static async Task AddToPlaylist(TrackItem trackItem, bool displayToastNotif = true)
        {
            Locator.MusicLibraryVM.CurrentTrackCollection.Playlist.Add(trackItem);
            await MusicLibraryVM.TracklistItemRepository.Add(new TracklistItem()
            {
                TrackId = trackItem.Id,
                TrackCollectionId = Locator.MusicLibraryVM.CurrentTrackCollection.Id,
            });
            if (displayToastNotif)
                ToastHelper.Basic(trackItem.Name + " added to your playlist");
        }

        public static async Task AddToPlaylist(AlbumItem albumItem)
        {
            var playlistId = Locator.MusicLibraryVM.CurrentTrackCollection.Id;
            foreach (TrackItem trackItem in albumItem.Tracks)
            {
                Locator.MusicLibraryVM.CurrentTrackCollection.Playlist.Add(trackItem);
                await MusicLibraryVM.TracklistItemRepository.Add(new TracklistItem()
                {
                    TrackId = trackItem.Id,
                    TrackCollectionId = playlistId,
                });
            }
            ToastHelper.Basic(albumItem.Name + " added to your playlist");
        }

        public static async Task UpdateTrackCollection(TrackCollection trackCollection)
        {
            var loadTracks = await MusicLibraryVM.TracklistItemRepository.LoadTracks(trackCollection);
            foreach (TracklistItem tracklistItem in loadTracks)
            {
                await MusicLibraryVM.TracklistItemRepository.Remove(tracklistItem);
            }
            foreach (TrackItem trackItem in trackCollection.Playlist)
            {
                var trackListItem = new TracklistItem { TrackId = trackItem.Id, TrackCollectionId = trackCollection.Id };
                await MusicLibraryVM.TracklistItemRepository.Add(trackListItem);
            }
        }

        public static async Task RemoveTrackFromCollectionAndDatabase(TrackItem trackItem)
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                MusicLibraryVM._trackDataRepository.Remove(Locator.MusicLibraryVM.Tracks.FirstOrDefault(x => x.Path == trackItem.Path));
                Locator.MusicLibraryVM.Tracks.Remove(Locator.MusicLibraryVM.Tracks.FirstOrDefault(x => x.Path == trackItem.Path));
                var album = Locator.MusicLibraryVM.Albums.FirstOrDefault(x => x.Id == trackItem.AlbumId);
                if (album != null)
                    album.Tracks.Remove(album.Tracks.FirstOrDefault(x => x.Path == trackItem.Path));
                var artist = Locator.MusicLibraryVM.Artists.FirstOrDefault(x => x.Id == trackItem.ArtistId);
                if (artist != null)
                {
                    var artistalbum = artist.Albums.FirstOrDefault(x => x.Id == trackItem.AlbumId);
                    if (artistalbum != null) artistalbum.Tracks.Remove(artistalbum.Tracks.FirstOrDefault(x => x.Path == trackItem.Path));
                }

                var playingTrack =
                    Locator.MusicPlayerVM.TrackCollection.Playlist.FirstOrDefault(x => x.Id == trackItem.Id);
                if (playingTrack != null) Locator.MusicPlayerVM.TrackCollection.Playlist.Remove(playingTrack);
            });
        }
    }
}
