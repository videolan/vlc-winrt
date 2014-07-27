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
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using SQLite;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Commands.MusicPlayer;
using VLC_WINRT_APP.Common;
using VLC_WINRT_APP.DataRepository;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Helpers.MusicLibrary;
using VLC_WINRT_APP.Helpers.MusicLibrary.MusicEntities;
using VLC_WINRT_APP.Helpers.MusicLibrary.xboxmusic.Models;
using VLC_WINRT_APP.Model;
using XboxMusicLibrary;
using VLC_WINRT_APP.Commands.Music;
using Panel = VLC_WINRT_APP.Model.Panel;

namespace VLC_WINRT_APP.ViewModels.MusicVM
{
    public class MusicLibraryVM : BindableBase
    {
        public static ArtistDataRepository _artistDataRepository = new ArtistDataRepository();
        public static TrackDataRepository _trackDataRepository = new TrackDataRepository();
        public static AlbumDataRepository _albumDataRepository = new AlbumDataRepository();

        #region private fields
#if WINDOWS_APP
        private ObservableCollection<Panel> _panels = new ObservableCollection<Panel>();
#endif
        private ObservableCollection<ArtistItem> _artistses = new ObservableCollection<ArtistItem>();
        private ObservableCollection<string> _albumsCover = new ObservableCollection<string>();
        private ObservableCollection<TrackItem> _trackses = new ObservableCollection<TrackItem>();
        private ObservableCollection<AlbumItem> _favoriteAlbums = new ObservableCollection<AlbumItem>();
        private ObservableCollection<AlbumItem> _randomAlbums = new ObservableCollection<AlbumItem>();
        #endregion
        #region private props
        private LoadingState _loadingState;
        private AlbumClickedCommand _albumClickedCommand;
        private ArtistClickedCommand _artistClickedCommand;
        private TrackClickedCommand _trackClickedCommand;
        private ArtistItem _currentArtist;
        private bool _isLoaded = false;
        private bool _isBusy = false;
        private bool _isMusicLibraryEmpty = true;
        private bool _isAlbumPageShown = false;
        private string _currentIndexingStatus = "Loading music";
        #endregion
        #region public fields
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

#if WINDOWS_APP
        public ObservableCollection<Panel> Panels
        {
            get { return _panels; }
            set
            {
                SetProperty(ref _panels, value);
            }
        }
#endif

        public ObservableCollection<ArtistItem> Artists
        {
            get { return _artistses; }
            set { SetProperty(ref _artistses, value); }
        }

        public ObservableCollection<TrackItem> Tracks
        {
            get { return _trackses; }
            set { SetProperty(ref _trackses, value); }
        }

        #endregion
        #region public props
        public LoadingState LoadingState
        {
            get { return _loadingState; }
            set { SetProperty(ref _loadingState, value); }
        }

        public string CurrentIndexingStatus
        {
            get { return _currentIndexingStatus; }
            set { SetProperty(ref _currentIndexingStatus, value); }
        }

        public bool IsAlbumPageShown
        {
            get { return _isAlbumPageShown; }
            set { SetProperty(ref _isAlbumPageShown, value); }
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
        public AlbumClickedCommand AlbumClickedCommand
        {
            get
            {
                return _albumClickedCommand;
            }
            set { SetProperty(ref _albumClickedCommand, value); }
        }
        public ArtistClickedCommand ArtistClickedCommand
        {
            get
            {
                return _artistClickedCommand;
            }
            set { SetProperty(ref _artistClickedCommand, value); }
        }

        public TrackClickedCommand TrackClickedCommand
        {
            get { return _trackClickedCommand; }
            set { SetProperty(ref _trackClickedCommand, value); }
        }

        public ArtistItem CurrentArtist
        {
            get { return _currentArtist; }
            set { SetProperty(ref _currentArtist, value); }
        }
        #endregion
        #region XBOX Music Stuff
        public MusicHelper XboxMusicHelper = new MusicHelper();
        public Authenication XboxMusicAuthenication;
        #endregion
        public MusicLibraryVM()
        {
            LoadingState = LoadingState.NotLoaded;
            var resourceLoader = new ResourceLoader();
#if WINDOWS_APP
            Panels.Add(new Panel(resourceLoader.GetString("Albums").ToLower(), 0, 1, App.Current.Resources["HomePath"].ToString()));
            Panels.Add(new Panel(resourceLoader.GetString("Artists").ToLower(), 1, 0.4, App.Current.Resources["HomePath"].ToString()));
            Panels.Add(new Panel(resourceLoader.GetString("Songs").ToLower(), 2, 0.4, App.Current.Resources["HomePath"].ToString()));
            //Panels.Add(new Panel(resourceLoader.GetString("Pinned").ToLower(), 2, 0.4, App.Current.Resources["HomePath"].ToString()));
            //Panels.Add(new Panel(resourceLoader.GetString("Playlists").ToLower(), 2, 0.4, App.Current.Resources["HomePath"].ToString()));
#endif
            }

        public void Initialize()
        {
            LoadingState = LoadingState.Loading;
            _albumClickedCommand = new AlbumClickedCommand();
            _artistClickedCommand = new ArtistClickedCommand();
            _trackClickedCommand = new TrackClickedCommand();
            Task.Run(() => GetMusicFromLibrary());
        }

        #region methods

        public async Task GetMusicFromLibrary()
        {
            await LoadFromDatabase();
            App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => IsMusicLibraryEmpty = false);
            if (Artists.Any())
            {
                LoadFavoritesRandomAlbums();
            }
            else
            {
                await StartIndexing();
                return;
            }

            if (!await VerifyAllFilesAreHere())
            {
                await StartIndexing();
                return;
            }

            App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                LoadingState = LoadingState.Loaded;
                IsLoaded = true;
                IsBusy = false;
            });
        }

        private void LoadFavoritesRandomAlbums()
        {
            try
            {
                foreach (AlbumItem album in Artists.SelectMany(artist => artist.Albums))
                {
                    if (album.Favorite)
                    {
                        App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                        {
                            RandomAlbums.Add(album);
                            FavoriteAlbums.Add(album);
                            OnPropertyChanged("FavoriteAlbums");
                        });
                    }

                    if (RandomAlbums.Count < 12)
                    {
                        if (!album.Favorite)

                            App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                            RandomAlbums.Add(album));
                    }
                    foreach (TrackItem trackItem in album.Tracks)
                    {
                        App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                            Tracks.Add(trackItem));
                    }
                }

                App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    OnPropertyChanged("Artist");
                    OnPropertyChanged("Albums");
                    OnPropertyChanged("Tracks");
                });
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
                OnPropertyChanged("Artists");
                OnPropertyChanged("IsBusy");
                OnPropertyChanged("IsMusicLibraryEmpty");
                OnPropertyChanged("IsLoaded");
                LoadingState = LoadingState.Loaded;
            });
            LoadFavoritesRandomAlbums();
        }

        private async Task GetAllMusicFolders()
        {
#if WINDOWS_APP
            StorageLibrary musicLibrary = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);
            foreach (StorageFolder storageFolder in musicLibrary.Folders)
            {
                await CreateDatabaseFromMusicFolder(storageFolder);
            }
#else
            StorageFolder musicLibrary = KnownFolders.MusicLibrary;
            CreateDatabaseFromMusicFolder(musicLibrary);
#endif

        }

        private async Task CreateDatabaseFromMusicFolder(StorageFolder musicFolder)
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
                App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    foreach (var artist in orderedArtists)
                    {
                        Artists.Add(artist);
                    }
                });
            }
            catch (Exception)
            {
                Debug.WriteLine("Error getting database.");
            }

            DispatchHelper.InvokeAsync(() =>
            {
                IsMusicLibraryEmpty = !Artists.Any();
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
            // TODO: Improve the algorithm
            return true;
        }
        #endregion

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
                get { return _currentAlbumIndex; }
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
                //get
                //{
                //    if (_biography != null)
                //    {
                //        return _biography;
                //    }
                //    if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                //        return "Please verify your internet connection";
                //    Task.Run(() => ArtistInformationsHelper.GetArtistBiography(this));
                //    return "Loading";
                //}
                //set
                //{
                //    SetProperty(ref _biography, value);
                //}
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
            private bool _isPictureLoaded;
            private ObservableCollection<TrackItem> _trackItems = new ObservableCollection<TrackItem>();
            private PlayAlbumCommand _playAlbumCommand = new PlayAlbumCommand();
            private FavoriteAlbumCommand _favoriteAlbumCommand = new FavoriteAlbumCommand();
            private TrackClickedCommand _trackClickedCommand = new TrackClickedCommand();

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
                set
                {
                    SetProperty(ref _currentTrackPosition, value); 
                    OnPropertyChanged("CurrentTrackPosition");
                }
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
            [Ignore]
            public TrackClickedCommand TrackClicked
            {
                get { return _trackClickedCommand; }
                set { SetProperty(ref _trackClickedCommand, value); }
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
            private TrackClickedCommand _trackClickedCommand = new TrackClickedCommand();
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
            public TrackClickedCommand TrackClicked
            {
                get { return _trackClickedCommand; }
                set { SetProperty(ref _trackClickedCommand, value); }
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
