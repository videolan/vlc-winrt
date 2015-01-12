using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Playback;
using XboxMusicLibrary.Models;
#if WINDOWS_PHONE_APP
using Windows.Phone.ApplicationModel;
#endif
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Autofac;
using VLC_WINRT_APP.Helpers.MusicLibrary.Deezer;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.Services.Interface;
using VLC_WINRT_APP.Services.RunTime;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MusicVM;
using WinRTXamlToolkit.IO.Extensions;

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
                    await CreateDatabaseFromMusicFolder(storageFolder, routineCheck);
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

        private static async Task CreateDatabaseFromMusicFolder(StorageFolder musicFolder, bool routineCheck = false)
        {
            try
            {
                if (Locator.VideoVm.IsPlaying || (BackgroundMediaPlayer.Current != null 
                    && BackgroundMediaPlayer.Current.CurrentState != MediaPlayerState.Stopped 
                    && BackgroundMediaPlayer.Current.CurrentState != MediaPlayerState.Closed))
                {
                    await Locator.MusicLibraryVM.ContinueIndexing.Task;
                }
                var folders = await musicFolder.GetFoldersAsync();
                if (folders.Any())
                {
                    foreach (var storageFolder in folders)
                    {
                        await CreateDatabaseFromMusicFolder(storageFolder, routineCheck);
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
        }

        private static async Task CreateDatabaseFromMusicFile(StorageFile item)
        {
            try
            {
                if (!VLCFileExtensions.AudioExtensions.Contains(item.FileType.ToLower())) return;
                LogHelper.Log("Music indexation: found music file " + item.Path);
                MusicProperties properties = await item.Properties.GetMusicPropertiesAsync();
                Dictionary<string, object> propDictionary = null;
                MediaService mediaService = App.Container.Resolve<IMediaService>() as MediaService;
                if (properties != null)
                {
                    propDictionary = new Dictionary<string, object>();
                    propDictionary.Add("artist", properties.Artist);
                    propDictionary.Add("album", properties.Album);
                    propDictionary.Add("title", properties.Title);
                    propDictionary.Add("year", properties.Year);
                    propDictionary.Add("duration", properties.Duration);
                    propDictionary.Add("tracknumber", properties.TrackNumber);
                }
                else
                {
                    propDictionary = mediaService.GetMusicProperties(item.Path);
                }
                if (propDictionary["duration"] == null || (TimeSpan)propDictionary["duration"] == TimeSpan.Zero)
                {
                    propDictionary["duration"] = mediaService.GetDuration(item.Path);
                }
                if (propDictionary != null)
                {
                    var artistName = propDictionary["artist"].ToString();
                    ArtistItem artist = await MusicLibraryVM._artistDataRepository.LoadViaArtistName(artistName);
                    if (artist == null)
                    {
                        artist = new ArtistItem();
                        artist.Name = string.IsNullOrEmpty(artistName) ? "Unknown artist" : artistName;
                        await MusicLibraryVM._artistDataRepository.Add(artist);
                        await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                        {
                            Locator.MusicLibraryVM.Artists.Add(artist);
                        });
                    }

                    var albumName = propDictionary["album"].ToString();
                    var albumYear = (uint)propDictionary["year"];
                    AlbumItem album =
                        await MusicLibraryVM._albumDataRepository.LoadAlbumViaName(artist.Id, albumName);
                    if (album == null)
                    {
                        album = new AlbumItem
                        {
                            Name = string.IsNullOrEmpty(albumName) ? "Unknown album" : albumName,
                            Artist = string.IsNullOrEmpty(artistName) ? "Unknown artist" : artistName,
                            ArtistId = artist.Id,
                            Favorite = false,
                            Year = albumYear
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

                    var duration = (TimeSpan)propDictionary["duration"];
                    var title = propDictionary["title"].ToString();
                    var trackNb = (uint)propDictionary["tracknumber"];
                    TrackItem track = new TrackItem()
                    {
                        AlbumId = album.Id,
                        AlbumName = album.Name,
                        ArtistId = artist.Id,
                        ArtistName = artist.Name,
                        CurrentPosition = 0,
                        Duration = duration,
                        Favorite = false,
                        Name = string.IsNullOrEmpty(title) ? item.DisplayName : title,
                        Path = item.Path,
                        Index = trackNb,
                        IsFromSandbox = true
                    };

                    if (album.LoadingState == LoadingState.NotLoaded && !album.IsPictureLoaded)
                    {
                        album.LoadingState = LoadingState.Loading;
                        await SetAlbumCover(album, track.Path, false, mediaService);
                    }
                    await MusicLibraryVM._trackDataRepository.Add(track);
                }
            }
            catch (Exception e)
            {
                ExceptionHelper.CreateMemorizedException("MusicLibraryManagement.CreateDatabaseFromMusicFile", e);
            }
        }

        public static async Task SetAlbumCover(AlbumItem album, string filePath, bool useLibVLc, MediaService mediaService = null)
        {
            if (useLibVLc)
            {
                if (mediaService == null)
                    mediaService = App.Container.Resolve<IMediaService>() as MediaService;
                var albumUrl = mediaService.GetAlbumUrl(filePath);
                if (!string.IsNullOrEmpty(albumUrl))
                {
                    await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        album.IsPictureLoaded = true;
                        album.Picture = System.Net.WebUtility.UrlDecode(albumUrl.Replace("file:///", ""));
                    });
                    await MusicLibraryVM._albumDataRepository.Update(album);
                    Debug.WriteLine("VLC found embedded cover " + albumUrl);
                }
            }
            else
            {
                try
                {
                    var folderPath = Path.GetDirectoryName(filePath);
                    var folder = await StorageFolder.GetFolderFromPathAsync(folderPath + "\\");
                    bool thumbnail = await folder.ContainsFileAsync("Folder.jpg");
                    if (thumbnail)
                    {
                        await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            album.IsPictureLoaded = true;
                            album.Picture = folder.Path + "\\Folder.jpg";
                        });
                        await MusicLibraryVM._albumDataRepository.Update(album);
                        Debug.WriteLine("WinRT found embedded cover " + album.Picture);
                    }
                }
                catch(Exception exception)
                {
                    ExceptionHelper.CreateMemorizedException("MusicLibraryManagement.SetAlbumCover : get WinRT cover", exception);
                }
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
