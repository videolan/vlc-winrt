using Windows.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
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
using VLC_WINRT_APP.Views.MusicPages.PlaylistControls;
using WinRTXamlToolkit.Controls.Extensions;
using WinRTXamlToolkit.IO.Extensions;
#if WINDOWS_PHONE_APP
using Windows.Media.Playback;
using Windows.Phone.ApplicationModel;
#else
#endif

namespace VLC_WINRT_APP.Helpers.MusicLibrary
{
    public static class MusicLibraryManagement
    {
        static readonly SemaphoreSlim AlbumCoverFetcherSemaphoreSlim = new SemaphoreSlim(2);
        static readonly SemaphoreSlim ArtistPicFetcherSemaphoreSlim = new SemaphoreSlim(2);
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


        public static async Task LoadFromSQL()
        {
            try
            {
                LogHelper.Log("Loading artists from MusicDB ...");
                var artists = await MusicLibraryVM._artistDataRepository.Load();
                LogHelper.Log("Found " + artists.Count + " artists from MusicDB");
                var orderedArtists = artists.OrderBy(x => x.Name);
                var tracks = await MusicLibraryVM._trackDataRepository.LoadTracks().ToObservableAsync();
                var albums = await MusicLibraryVM._albumDataRepository.LoadAlbums(x => x.ArtistId != 0).ToObservableAsync();

                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    foreach (var artist in orderedArtists)
                    {
                        Locator.MusicLibraryVM.Artists.Add(artist);
                    }
                    Locator.MusicLibraryVM.Tracks = tracks;
                    Locator.MusicLibraryVM.Albums = albums;
                });

                var trackColl = await MusicLibraryVM.TrackCollectionRepository.LoadTrackCollections().ToObservableAsync();
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Locator.MusicLibraryVM.TrackCollections = trackColl;
                });

                foreach (TrackCollection trackCollection in Locator.MusicLibraryVM.TrackCollections)
                {
                    var observableCollection = await MusicLibraryVM.TracklistItemRepository.LoadTracks(trackCollection);
                    foreach (TracklistItem tracklistItem in observableCollection)
                    {
                        TrackItem item = await MusicLibraryVM._trackDataRepository.LoadTrack(tracklistItem.TrackId);
                        trackCollection.Playlist.Add(item);
                    }
                }
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Locator.MusicLibraryVM.AlphaGroupedTracks =
                        Locator.MusicLibraryVM.Tracks
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
                if (App.IMediaService.ContinueIndexing != null)
                {
                    await App.IMediaService.ContinueIndexing.Task;
                    App.IMediaService.ContinueIndexing = null;
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
                MusicProperties properties = await item.Properties.GetMusicPropertiesAsync();
                MediaProperties mP = null;
                if (properties != null)
                {
                    mP = new MediaProperties();
                    mP.Artist = properties.Artist;
                    mP.Album = properties.Album;
                    mP.Title = properties.Title;
                    mP.Year = properties.Year;
                    mP.Duration = properties.Duration;
                    mP.Tracknumber = properties.TrackNumber;
                    mP.Genre = (properties.Genre.Any()) ? properties.Genre[0] : null;
                }
                if (mP != null)
                {
                    var artistName = mP.Artist;
                    ArtistItem artist = MusicLibraryVM._artistDataRepository.LoadViaArtistName(artistName);
                    if (artist == null)
                    {
                        artist = new ArtistItem();
                        artist.Name = string.IsNullOrEmpty(artistName) ? string.Empty : artistName;
                        await MusicLibraryVM._artistDataRepository.Add(artist);
                        await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                        {
                            Locator.MusicLibraryVM.Artists.Add(artist);
                        });
                    }

                    var albumName = mP.Album;
                    var albumYear = mP.Year;
                    AlbumItem album =
                        await MusicLibraryVM._albumDataRepository.LoadAlbumViaName(artist.Id, albumName);
                    if (album == null)
                    {
                        album = new AlbumItem
                        {
                            Name = string.IsNullOrEmpty(albumName) ? string.Empty : albumName,
                            Artist = string.IsNullOrEmpty(artistName) ? string.Empty : artistName,
                            ArtistId = artist.Id,
                            Favorite = false,
                            Year = albumYear
                        };
                        await MusicLibraryVM._albumDataRepository.Add(album);
                        await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                        {
                            var artistFromCollection = Locator.MusicLibraryVM.Artists.FirstOrDefault(x => x.Id == album.ArtistId);
                            if (artistFromCollection != null) artistFromCollection.Albums.Add(album);
                            Locator.MusicLibraryVM.CurrentIndexingStatus = "Found " + Locator.MusicLibraryVM.Albums.Count + " albums";
#if WINDOWS_PHONE_APP
                            StatusBarHelper.UpdateTitle("Found " + album.Name);
#endif
                            Locator.MusicLibraryVM.Albums.Add(album);
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
                    await MusicLibraryVM._trackDataRepository.Add(track);
                }
            }
            catch (Exception e)
            {
                ExceptionHelper.CreateMemorizedException("MusicLibraryManagement.CreateDatabaseFromMusicFile", e);
            }
        }

        public static async Task<bool> SetAlbumCover(AlbumItem album, string filePath, bool useLibVLc, MediaService mediaService = null)
        {
            if (useLibVLc)
            {
                if (mediaService == null)
                    mediaService = App.IMediaService as MediaService;
                var albumUrl = mediaService.GetAlbumUrl(filePath);
                if (!string.IsNullOrEmpty(albumUrl))
                {
                    await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        album.IsPictureLoaded = true;
                        album.IsCoverInLocalFolder = false;
                        album.Picture = System.Net.WebUtility.UrlDecode(albumUrl.Replace("file:///", ""));
                    });
                    Debug.WriteLine("VLC found embedded cover " + album.Id + " " + albumUrl);
                    return true;
                }
            }
            else
            {
                try
                {
                    if (filePath == null) return false;
                    var destinationFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("albumPic", CreationCollisionOption.OpenIfExists);
                    var thumbnail = false;
#if WINDOWS_APP
                    var file = await StorageFile.GetFileFromPathAsync(filePath);
                    var mP = await file.GetThumbnailAsync(ThumbnailMode.MusicView, 200, ThumbnailOptions.ReturnOnlyIfCached);
                    if (mP != null)
                    {
                        Debug.WriteLine("WinRT found embedded cover " + album.Id + " " + album.Name);
                        thumbnail = true;
                        var buffer = new Windows.Storage.Streams.Buffer(Convert.ToUInt32(mP.Size));
                        var iBuf = await mP.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.None);
                        var destFile = await destinationFolder.CreateFileAsync(album.Id + ".jpg", CreationCollisionOption.ReplaceExisting);
                        using (var strm = await destFile.OpenAsync(FileAccessMode.ReadWrite))
                        {
                            await strm.WriteAsync(iBuf);
                        }
                    }

                    if (thumbnail == false)
#endif
                    {
                        var folderPath = Path.GetDirectoryName(filePath);
                        var folder = await StorageFolder.GetFolderFromPathAsync(folderPath + "\\");
                        var coverName = "Folder.jpg";
                        thumbnail = await folder.ContainsFileAsync(coverName);
                        if (!thumbnail)
                        {
                            coverName = "cover.jpg";
                            thumbnail = await folder.ContainsFileAsync(coverName);
                        }
                        if (thumbnail)
                        {
                            var folderPicFile = await folder.GetFileAsync(coverName);
                            Debug.WriteLine("Writing file " + "albumPic" + " " + album.Id);
                            await folderPicFile.CopyAsync(destinationFolder, String.Format("{0}.jpg", album.Id), NameCollisionOption.FailIfExists);
                        }
                        else return false;
                    }

                    Debug.WriteLine("VLC_WINRT found embedded cover " + album.Id + " " + album.Name);
                    await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        album.IsPictureLoaded = true;
                        album.IsCoverInLocalFolder = true;
                        Debug.WriteLine("WinRT found embedded cover " + album.AlbumCoverUri);
                    });
                    await MusicLibraryVM._albumDataRepository.Update(album);
                    return true;
                }
                catch (Exception exception)
                {
                    ExceptionHelper.CreateMemorizedException("MusicLibraryManagement.SetAlbumCover : get WinRT cover", exception);
                }
            }
            return false;
        }

        public static async Task PopulateTracks(this AlbumItem album)
        {
            var tracks = await MusicLibraryVM._trackDataRepository.LoadTracksByAlbumId(album.Id);
            var orderedTracks = tracks.OrderBy(x => x.Index).ToObservable();
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                album.Tracks = orderedTracks;
            });
        }

        public static async Task PopulateAlbums(this ArtistItem artist)
        {
            var albums = await MusicLibraryVM._albumDataRepository.LoadAlbumsFromId(artist.Id);
            var orderedAlbums = albums.OrderBy(x => x.Name).ToObservable();
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                artist.Albums = orderedAlbums;
            });
        }

        public static async Task LoadFavoriteRandomAlbums()
        {
            try
            {
                int howManyAlbumsToFill = await HowManyAlbumsToDisplayWithTwoRows();
                if (Locator.MusicLibraryVM.RandomAlbums != null && Locator.MusicLibraryVM.RandomAlbums.Any()) return;
                ObservableCollection<AlbumItem> favAlbums = await MusicLibraryVM._albumDataRepository.LoadAlbums(x => x.Favorite).ToObservableAsync();
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
                ObservableCollection<AlbumItem> nonfavAlbums = await MusicLibraryVM._albumDataRepository.LoadAlbums(x => x.Favorite == false).ToObservableAsync();
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

        public static void OrderAlbums()
        {
            if (Locator.SettingsVM.AlbumsOrderType == OrderType.ByArtist)
            {
                if (Locator.SettingsVM.AlbumsOrderListing == OrderListing.Ascending)
                {
                    Locator.MusicLibraryVM.Albums = Locator.MusicLibraryVM.Albums.OrderBy(x => x.Artist).ToObservable();
                }
                else if (Locator.SettingsVM.AlbumsOrderListing == OrderListing.Descending)
                {
                    Locator.MusicLibraryVM.Albums = Locator.MusicLibraryVM.Albums.OrderByDescending(x => x.Artist).ToObservable();
                }
            }
            else if (Locator.SettingsVM.AlbumsOrderType == OrderType.ByDate)
            {
                if (Locator.SettingsVM.AlbumsOrderListing == OrderListing.Ascending)
                {
                    Locator.MusicLibraryVM.Albums = Locator.MusicLibraryVM.Albums.OrderBy(x => x.Year).ToObservable();
                }
                else if (Locator.SettingsVM.AlbumsOrderListing == OrderListing.Descending)
                {
                    Locator.MusicLibraryVM.Albums = Locator.MusicLibraryVM.Albums.OrderByDescending(x => x.Year).ToObservable();
                }
            }
        }

        public static async Task AddNewPlaylist(string trackCollectionName)
        {
            if (string.IsNullOrEmpty(trackCollectionName)) return;
            TrackCollection trackCollection = null;
            trackCollection = await MusicLibraryVM.TrackCollectionRepository.LoadFromName(trackCollectionName);
            if (trackCollection != null)
            {
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    ToastHelper.Basic("A playlist with this name already exists");
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
            if (Locator.MusicLibraryVM.CurrentTrackCollection == null) return;
            if (Locator.MusicLibraryVM.CurrentTrackCollection.Playlist.Contains(trackItem))
            {
                ToastHelper.Basic("This track is already in your playlist!");
                return;
            }
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
            if (Locator.MusicLibraryVM.CurrentTrackCollection == null) return;
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
                    Locator.MediaPlaybackViewModel.TrackCollection.Playlist.FirstOrDefault(x => x.Id == trackItem.Id);
                if (playingTrack != null) Locator.MediaPlaybackViewModel.TrackCollection.Playlist.Remove(playingTrack);
            });
        }

        public static void AddAlbumToPlaylist(object args)
        {
            if (Locator.MusicLibraryVM.CurrentTrackCollection == null)
            {
#if WINDOWS_PHONE_APP
                ((ContentDialogButtonClickEventArgs) args).Cancel = true;
#endif
                ToastHelper.Basic("You have to select a playlist", false, "selectplaylist");
                return;
            }
#if WINDOWS_APP
            var flyout = ((AddAlbumToPlaylistBase)args).GetFirstDescendantOfType<SettingsFlyout>();
            if (flyout != null) flyout.Hide();
#endif
            Locator.MusicLibraryVM.AddToPlaylistCommand.Execute(Locator.MusicLibraryVM.CurrentAlbum);
        }
    }
}
