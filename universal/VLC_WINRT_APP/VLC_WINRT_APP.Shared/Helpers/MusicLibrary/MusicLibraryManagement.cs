using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Core;
using Windows.UI.Xaml;
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
                var artists = await MusicLibraryVM._artistDataRepository.Load();
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

            }
            catch (Exception)
            {
                Debug.WriteLine("Error getting database.");
            }
        }

        public static async Task GetAllMusicFolders()
        {
#if WINDOWS_APP
            StorageLibrary musicLibrary = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);
            foreach (StorageFolder storageFolder in musicLibrary.Folders)
            {
                await CreateDatabaseFromMusicFolder(storageFolder);
            }
#else
            StorageFolder musicLibrary = KnownFolders.MusicLibrary;
            await CreateDatabaseFromMusicFolder(musicLibrary);
#endif

        }

        private static async Task CreateDatabaseFromMusicFolder(StorageFolder musicFolder)
        {
            IReadOnlyList<IStorageItem> items = await musicFolder.GetItemsAsync();
            foreach (IStorageItem storageItem in items)
            {
                if (storageItem.IsOfType(StorageItemTypes.File))
                {
                    await CreateDatabaseFromMusicFile((StorageFile)storageItem);
                }
                else
                {
                    await CreateDatabaseFromMusicFolder((StorageFolder)storageItem);
                }
            }
        }

        private static async Task CreateDatabaseFromMusicFile(StorageFile item)
        {
            MusicProperties properties = await item.Properties.GetMusicPropertiesAsync();
            if (properties != null && !string.IsNullOrEmpty(properties.Album) && !string.IsNullOrEmpty(properties.Artist) && !string.IsNullOrEmpty(properties.Title))
            {
                ArtistItem artist = await MusicLibraryVM._artistDataRepository.LoadViaArtistName(properties.Artist);
                if (artist == null)
                {
                    artist = new ArtistItem();
                    artist.Name = string.IsNullOrEmpty(properties.Artist) ? "Unknown artist" : properties.Artist;
                    await MusicLibraryVM._artistDataRepository.Add(artist);
                    App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                    {
                        Locator.MusicLibraryVM.Artists.Add(artist);
                    });
                }

                AlbumItem album = await MusicLibraryVM._albumDataRepository.LoadAlbumViaName(artist.Id, properties.Album);
                if (album == null)
                {
                    album = new AlbumItem
                    {
                        Name = string.IsNullOrEmpty(properties.Album) ? "Unknown album" : properties.Album,
                        Artist = string.IsNullOrEmpty(properties.Artist) ? "Unknown artist" : properties.Artist,
                        ArtistId = artist.Id,
                        Favorite = false,
                    };
                    await MusicLibraryVM._albumDataRepository.Add(album);
                    App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                    {
                        Locator.MusicLibraryVM.Artists.FirstOrDefault(x => x.Id == album.ArtistId).Albums.Add(album);
                        Locator.MusicLibraryVM.CurrentIndexingStatus = "Found album " + album.Name;
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
                    Name = string.IsNullOrEmpty(properties.Title) ? "Unknown track" : properties.Title,
                    Path = item.Path,
                    Index = (int)properties.TrackNumber,
                };
                await MusicLibraryVM._trackDataRepository.Add(track);
            }
        }

        public static async Task GetTracks(this AlbumItem album)
        {
            album.Tracks = new ObservableCollection<TrackItem>();
            var tracks = await MusicLibraryVM._trackDataRepository.LoadTracksByAlbumId(album.Id);
            var orderedTracks = tracks.OrderBy(x => x.Index);
            foreach (var track in orderedTracks)
            {
                album.Tracks.Add(track);
            }
        }

        public static async Task GetAlbums(this ArtistItem artist)
        {
            artist.Albums = new ObservableCollection<AlbumItem>();
            var albums = await MusicLibraryVM._albumDataRepository.LoadAlbumsFromId(artist.Id);
            var orderedAlbums = albums.OrderBy(x => x.Name);
            foreach (AlbumItem album in orderedAlbums)
            {
                artist.Albums.Add(album);
            }
        }

        public static async Task LoadFavoriteRandomAlbums()
        {
            try
            {
                if (Locator.MusicLibraryVM.RandomAlbums != null && Locator.MusicLibraryVM.RandomAlbums.Any()) return;
                ObservableCollection<AlbumItem> favAlbums = await MusicLibraryVM._albumDataRepository.LoadAlbums(x => x.Favorite);
                if (favAlbums != null && favAlbums.Any())
                {
                    App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
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
                    App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
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
                Debug.WriteLine("Error selecting random albums.");
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
    }
}
