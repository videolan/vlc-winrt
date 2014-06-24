/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using SQLite;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Commands;
using VLC_WINRT_APP.Commands.MusicPlayer;
using VLC_WINRT_APP.DataRepository;
using VLC_WINRT_APP.Utility.Helpers;
using VLC_WINRT_APP.Utility.Helpers.MusicLibrary;
using VLC_WINRT_APP.Utility.Helpers.MusicLibrary.MusicEntities;
using VLC_WINRT_APP.Utility.Helpers.MusicLibrary.xboxmusic.Models;
using VLC_WINRT.ViewModels;
using VLC_WINRT.Views.Controls.MainPage;
using VLC_WINRT_APP.Commands;
using XboxMusicLibrary;
using Panel = VLC_WINRT_APP.Model.Panel;
using VLC_WINRT_APP.ViewModels.Settings;
#if WINDOWS_PHONE_APP
using VLC_WINPRT;
#endif

namespace VLC_WINRT_APP.ViewModels.MusicVM
{
    public class MusicLibraryVM : BindableBase
    {
        private ObservableCollection<Panel> _panels = new ObservableCollection<Panel>();
        private ObservableCollection<ArtistItem> _artists = new ObservableCollection<ArtistItem>();
        private ObservableCollection<string> _albumsCover = new ObservableCollection<string>();
        private ObservableCollection<TrackItem> _tracks = new ObservableCollection<TrackItem>();
        private ObservableCollection<AlbumItem> _favoriteAlbums = new ObservableCollection<AlbumItem>();
        private ObservableCollection<AlbumItem> _randomAlbums = new ObservableCollection<AlbumItem>();
        private static ArtistDataRepository _artistDataRepository = new ArtistDataRepository();
        private static TrackDataRepository _trackDataRepository = new TrackDataRepository();
        private static AlbumDataRepository _albumDataRepository = new AlbumDataRepository();

        private StopVideoCommand _goBackCommand;
        private bool _isLoaded = false;
        private bool _isBusy = false;
        private bool _isMusicLibraryEmpty = true;

        // XBOX Music Stuff
        // REMOVE: Do we need this stuff anymore?
        public MusicHelper XboxMusicHelper = new MusicHelper();
        public Authenication XboxMusicAuthenication;
        ObservableCollection<string> _imgCollection = new ObservableCollection<string>();
        public MusicLibraryVM()
        {
            var resourceLoader = new ResourceLoader();
            _goBackCommand = new StopVideoCommand();
            Panels.Add(new Panel(resourceLoader.GetString("Artist").ToUpper(), 0, 1));
            Panels.Add(new Panel(resourceLoader.GetString("Tracks").ToUpper(), 1, 0.4));
            Panels.Add(new Panel(resourceLoader.GetString("FavoriteAlbums").ToUpper(), 2, 0.4));
        }

        public async Task Initialize()
        {
            await GetMusicFromLibrary();
        }

        public bool IsLoaded
        {
            get { return _isLoaded; }
            set { SetProperty(ref _isLoaded, value); }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        public bool IsMusicLibraryEmpty
        {
            get { return _isMusicLibraryEmpty; }
            set { SetProperty(ref _isMusicLibraryEmpty, value); }
        }

        public ObservableCollection<string> ImgCollection
        {
            get { return _imgCollection; }
            set
            {
                SetProperty(ref _imgCollection, value);
            }
        }

        public ObservableCollection<AlbumItem> FavoriteAlbums
        {
            get { return _favoriteAlbums; }
            set { SetProperty(ref _favoriteAlbums, value); }
        }

        public ObservableCollection<AlbumItem> RandomAlbums
        {
            get { return _randomAlbums; }
            set { SetProperty(ref _randomAlbums, value); }
        }

        public ObservableCollection<Panel> Panels
        {
            get { return _panels; }
            set
            {
                SetProperty(ref _panels, value);
            }
        }

        public StopVideoCommand GoBack
        {
            get { return _goBackCommand; }
            set { SetProperty(ref _goBackCommand, value); }
        }
        public ObservableCollection<ArtistItem> Artist
        {
            get { return _artists; }
            set { SetProperty(ref _artists, value); }
        }
        public ObservableCollection<string> AlbumCover
        {
            get { return _albumsCover; }
            set { SetProperty(ref _albumsCover, value); }
        }

        public ObservableCollection<TrackItem> Track
        {
            get { return _tracks; }
            set { SetProperty(ref _tracks, value); }
        }

        public async Task GetMusicFromLibrary()
        {
            await LoadFromDatabase();
            IsMusicLibraryEmpty = false;
            if (Artist.Any())
            {
                LoadFavoritesRandomAlbums();
            }
            else
            {
                await StartIndexing();
                return;
            }

            if(!await VerifyAllFilesAreHere())
            {
                await StartIndexing();
                return;
            }

            IsLoaded = true;
            IsBusy = false;
        }

        private void LoadFavoritesRandomAlbums()
        {
            try
            {
                foreach (AlbumItem album in Artist.SelectMany(artist => artist.Albums))
                {
                    if (album.Favorite)
                    {
                        RandomAlbums.Add(album);
                        FavoriteAlbums.Add(album);
                        OnPropertyChanged("FavoriteAlbums");
                    }

                    if (RandomAlbums.Count < 12)
                    {
                        if (!album.Favorite)
                            RandomAlbums.Add(album);
                    }
                    foreach (TrackItem trackItem in album.Tracks)
                    {
                        Track.Add(trackItem);
                    }
                }

                OnPropertyChanged("Artist");
                OnPropertyChanged("Albums");
                OnPropertyChanged("Tracks");
            }
            catch (Exception)
            {
                Debug.WriteLine("Error selecting random albums.");
            }
        }

        public async Task StartIndexing()
        {
            if (await DoesMusicDatabaseExist())
            {
                StorageFile dB = await ApplicationData.Current.LocalFolder.GetFileAsync("mediavlc.sqlite");
                dB.DeleteAsync();
            }

            DispatchHelper.InvokeAsync(() =>
            {
                IsBusy = true;
                IsLoaded = false;
                OnPropertyChanged("IsBusy");
                OnPropertyChanged("IsLoaded");
            });

            _artistDataRepository = new ArtistDataRepository();
            _artistDataRepository.Initialize();

            await GetAllMusicFolders();

            await LoadFromDatabase();

            await DispatchHelper.InvokeAsync(() =>
            {
                IsBusy = false;
                IsLoaded = true;
                IsMusicLibraryEmpty = false;
                OnPropertyChanged("IsBusy");
                OnPropertyChanged("IsMusicLibraryEmpty");
                OnPropertyChanged("IsLoaded");
            });
            LoadFavoritesRandomAlbums();
        }

        private async Task GetAllMusicFolders()
        {
            foreach (CustomFolder folder in Locator.SettingsVM.MusicFolders)
            {
                StorageFolder customMusicFolder;
                if (folder.Mru == "Music Library")
                {
                    customMusicFolder = KnownVLCLocation.MusicLibrary;
                }
                else
                {
                    customMusicFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(
                        folder.Mru);
                }
                await CreateDatabaseFromMusicFolder(customMusicFolder);
            }
        }

        private async Task CreateDatabaseFromMusicFolder(StorageFolder musicFolder)
        {
            IReadOnlyList<IStorageItem> items = await musicFolder.GetItemsAsync();
            foreach (IStorageItem storageItem in items)
            {
                if (storageItem.IsOfType(StorageItemTypes.File))
                {
                    await CreateDatabaseFromMusicFile((StorageFile) storageItem);
                }
                else
                {
                    await CreateDatabaseFromMusicFolder((StorageFolder) storageItem);
                }
            }
        }

        private async Task CreateDatabaseFromMusicFile(StorageFile item)
        {
            MusicProperties properties = await item.Properties.GetMusicPropertiesAsync();
            if (properties != null)
            {
                ArtistItem artist = await _artistDataRepository.LoadViaArtistName(properties.Artist);
                if (artist == null)
                {
                    artist = new ArtistItem();
                    artist.Name = string.IsNullOrEmpty(properties.Artist) ? "Unknown artist" : properties.Artist;
                    await _artistDataRepository.Add(artist);
                }

                AlbumItem album = await _albumDataRepository.LoadAlbumViaName(artist.Id, properties.Album);
                if (album == null)
                {

                    album = new AlbumItem
                    {
                        Name = string.IsNullOrEmpty(properties.Album) ? "Unknown album" : properties.Album,
                        Artist = string.IsNullOrEmpty(properties.Artist) ? "Unknown artist" : properties.Artist,
                        ArtistId = artist.Id,
                        Favorite = false,
                    };
                    await _albumDataRepository.Add(album);
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
                await _trackDataRepository.Add(track);
            }
        }

        private async Task LoadFromDatabase()
        {
            try
            {
                var artists = await _artistDataRepository.Load();
                foreach (var artistItem in artists)
                {
                    var albums = await _albumDataRepository.LoadAlbumsFromId(artistItem.Id);
                    foreach (var album in albums)
                    {
                        var tracks = await _trackDataRepository.LoadTracksByAlbumId(album.Id);
                        var orderedTracks = tracks.OrderBy(x => x.Index);
                        foreach (var track in orderedTracks)
                        {
                            album.Tracks.Add(track);
                        }
                    }
                    var orderedAlbums = albums.OrderBy(x => x.Name);
                    foreach (var album in orderedAlbums)
                    {
                        artistItem.Albums.Add(album);
                    }
                }
                var orderedArtists = artists.OrderBy(x => x.Name);
                foreach (var artist in orderedArtists)
                {
                    Artist.Add(artist);
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("Error getting database.");
            }

            DispatchHelper.InvokeAsync(() =>
            {
                IsMusicLibraryEmpty = !Artist.Any();
                OnPropertyChanged("IsMusicLibraryEmpty");
            });
        }

        public void ExecuteSemanticZoom(SemanticZoom sZ, CollectionViewSource cvs)
        {
            (sZ.ZoomedOutView as ListViewBase).ItemsSource = cvs.View.CollectionGroups;
        }

        private async Task<bool> DoesMusicDatabaseExist()
        {
            return await DoesFileExistHelper.DoesFileExistAsync("mediavlc.sqlite");
        }

        private async Task<bool> VerifyAllFilesAreHere()
        {
            var artists = await _artistDataRepository.Load();
            foreach (var artistItem in artists)
            {
                var albums = await _albumDataRepository.LoadAlbumsFromId(artistItem.Id);
                foreach (var album in albums)
                {
                    var tracks = await _trackDataRepository.LoadTracksByAlbumId(album.Id);
                    foreach (var trackItem in tracks)
                    {
                        try
                        {
                            StorageFile file = await StorageFile.GetFileFromPathAsync(trackItem.Path);
                        }
                        catch (FileNotFoundException exception)
                        {
                            Debug.WriteLine(trackItem.Path + "has been renamed, moved or deleted.");
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public class ArtistItem : BindableBase
        {
            private string _name;
            private string _picture;
            private bool _isPictureLoaded;
            private ObservableCollection<AlbumItem> _albumItems = new ObservableCollection<AlbumItem>();
            private int _currentAlbumIndex = 0;

            // more informations
            private bool _isFavorite;
            private bool _isOnlinePopularAlbumItemsLoaded;
            private List<Album> _onlinePopularAlbumItems;
            private bool _isOnlineRelatedArtistsLoaded;
            private List<Artist> _onlineRelatedArtists;
            private bool _isOnlineMusicVideosLoaded;
            private string _biography;

            [PrimaryKey, AutoIncrement, Column("_id")]
            public int Id { get; set; }

            [Ignore]
            public bool IsOnlinePopularAlbumItemsLoaded
            {
                get { return _isOnlinePopularAlbumItemsLoaded; }
                set { SetProperty(ref _isOnlinePopularAlbumItemsLoaded, value); }
            }

            [Ignore]
            public bool IsOnlineRelatedArtistsLoaded
            {
                get { return _isOnlineRelatedArtistsLoaded; }
                set { SetProperty(ref _isOnlineRelatedArtistsLoaded, value); }
            }

            [Ignore]
            public bool IsOnlineMusicVideosLoaded
            {
                get { return _isOnlineMusicVideosLoaded; }
                set { SetProperty(ref _isOnlineMusicVideosLoaded, value); }
            }

            public string Name
            {
                get { return _name; }
                set { SetProperty(ref _name, value); }
            }

            public string Picture
            {
                get
                {
                    if (!_isPictureLoaded)
                    {
                        Task.Run(() => LoadPicture());
                    }
                    return _picture;
                }
                set
                {
                    SetProperty(ref _picture, value);
                }
            }

            private void LoadPicture()
            {
                if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()) return;
                ArtistInformationsHelper.GetArtistPicture(this);
                _isPictureLoaded = true;
            }

            [Ignore]
            public ObservableCollection<AlbumItem> Albums
            {
                get { return _albumItems; }
                set { SetProperty(ref _albumItems, value); }
            }

            [Ignore]
            public int CurrentAlbumIndex
            {
                set { SetProperty(ref _currentAlbumIndex, value); }
            }

            [Ignore]
            public AlbumItem CurrentAlbumItem
            {
                get { return _albumItems[_currentAlbumIndex]; }
            }

            [Ignore]
            public string Biography
            {
                get
                {
                    if (_biography != null)
                    {
                        return _biography;
                    }
                    if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                        return "Please verify your internet connection";
                    ArtistInformationsHelper.GetArtistBiography(this);
                    return "Loading";
                }
                set { SetProperty(ref _biography, value); }
            }

            [Ignore]
            public List<Album> OnlinePopularAlbumItems
            {
                get
                {
                    if (_onlinePopularAlbumItems != null)
                        return _onlinePopularAlbumItems;
                    if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                        ArtistInformationsHelper.GetArtistTopAlbums(this);
                    return null;
                }
                set { SetProperty(ref _onlinePopularAlbumItems, value); }
            }

            [Ignore]
            public List<Artist> OnlineRelatedArtists
            {
                get
                {
                    if (_onlineRelatedArtists != null)
                        return _onlineRelatedArtists;
                    if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                        ArtistInformationsHelper.GetArtistSimilarsArtist(this);
                    return null;
                }
                set { SetProperty(ref _onlineRelatedArtists, value); }
            }

            public bool IsFavorite
            {
                get { return _isFavorite; }
                set { SetProperty(ref _isFavorite, value); }
            }
        }

        public class AlbumItem : BindableBase
        {
            private string _name;
            private string _artist;
            private int _currentTrackPosition;
            private string _picture = "/Assets/GreyPylon/280x156.jpg";
            private uint _year;
            private bool _favorite;
            private ObservableCollection<TrackItem> _trackItems = new ObservableCollection<TrackItem>();
            private PlayAlbumCommand _playAlbumCommand = new PlayAlbumCommand();
            private FavoriteAlbumCommand _favoriteAlbumCommand = new FavoriteAlbumCommand();
            private bool _isPictureLoaded;

            [PrimaryKey, AutoIncrement, Column("_id")]
            public int Id { get; set; }

            public int ArtistId { get; set; }

            public string Name
            {
                get { return _name; }
                set { SetProperty(ref _name, value); }
            }

            public string Artist
            {
                get { return _artist; }
                set
                {
                    SetProperty(ref _artist, value);
                }
            }

            [Ignore]
            public int CurrentTrackPosition
            {
                get { return _currentTrackPosition; }
                set { SetProperty(ref _currentTrackPosition, value); }
            }

            public bool Favorite
            {
                get { return _favorite; }
                set
                {
                    SetProperty(ref _favorite, value);
                }
            }

            [Ignore]
            public ObservableCollection<TrackItem> Tracks
            {
                get { return _trackItems; }
                set { SetProperty(ref _trackItems, value); }
            }

            public string Picture
            {
                get
                {
                    if (!_isPictureLoaded)
                    {
                        Task.Run(() => LoadPicture());
                    }
                    return _picture;
                }
                set { SetProperty(ref _picture, value); }
            }

            public uint Year
            {
                get { return _year; }
                set { SetProperty(ref _year, value); }
            }

            [Ignore]
            public TrackItem CurrentTrack
            {
                get { return _trackItems[CurrentTrackPosition]; }
                set
                {
                    CurrentTrackPosition = (value == null) ? 0 : _trackItems.IndexOf(value);
                }
            }

            public void NextTrack()
            {
                if (CurrentTrackPosition < _trackItems.Count)
                    CurrentTrackPosition++;
            }

            public void PreviousTrack()
            {
                if (CurrentTrackPosition > 0)
                    CurrentTrackPosition--;
            }

            private void LoadPicture()
            {
                if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()) return;
                ArtistInformationsHelper.GetAlbumPicture(this);
                _isPictureLoaded = true;
            }

            [Ignore]
            public PlayAlbumCommand PlayAlbum
            {
                get { return _playAlbumCommand; }
                set { SetProperty(ref _playAlbumCommand, value); }
            }

            [Ignore]
            public FavoriteAlbumCommand FavoriteAlbum
            {
                get { return _favoriteAlbumCommand; }
                set { SetProperty(ref _favoriteAlbumCommand, value); }
            }
        }

        public class TrackItem : BindableBase
        {
            private string _artistName;
            private string _albumName;
            private string _name;
            private string _path;
            private int _index;
            private TimeSpan _duration;
            private bool _favorite;
            private int _currentPosition;
            private PlayTrackCommand _playTrackCommand = new PlayTrackCommand();
            private FavoriteTrackCommand _favoriteTrackCommand = new FavoriteTrackCommand();

            [PrimaryKey, AutoIncrement, Column("_id")]
            public int Id { get; set; }
            public int AlbumId { get; set; }
            public int ArtistId { get; set; }
            public string ArtistName
            {
                get { return _artistName; }
                set { SetProperty(ref _artistName, value); }
            }
            public string AlbumName
            {
                get { return _albumName; }
                set { SetProperty(ref _albumName, value); }
            }

            public string Name
            {
                get { return _name; }
                set { SetProperty(ref _name, value); }
            }

            public string Path
            {
                get { return _path; }
                set { SetProperty(ref _path, value); }
            }

            public int Index
            {
                get { return _index; }
                set { SetProperty(ref _index, value); }
            }

            public TimeSpan Duration
            {
                get { return _duration; }
                set { SetProperty(ref _duration, value); }
            }
            public bool Favorite { get { return _favorite; } set { SetProperty(ref _favorite, value); } }

            [Ignore]
            public int CurrentPosition
            {
                get { return _currentPosition; }
                set { SetProperty(ref _currentPosition, value); }
            }

            [Ignore]
            public PlayTrackCommand PlayTrack
            {
                get { return _playTrackCommand; }
                set { SetProperty(ref _playTrackCommand, value); }
            }

            [Ignore]
            public FavoriteTrackCommand FavoriteTrack
            {
                get { return _favoriteTrackCommand; }
                set { SetProperty(ref _favoriteTrackCommand, value); }
            }
        }
    }
}
