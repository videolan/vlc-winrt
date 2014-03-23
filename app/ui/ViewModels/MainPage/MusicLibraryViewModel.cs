/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Commands;
using VLC_WINRT.Utility.Commands.MusicPlayer;
using VLC_WINRT.Utility.DataRepository;
using VLC_WINRT.Utility.Helpers;
using VLC_WINRT.Utility.Helpers.MusicLibrary;
using VLC_WINRT.Utility.Helpers.MusicLibrary.LastFm;
using VLC_WINRT.Utility.Helpers.MusicLibrary.xboxmusic.Models;
using VLC_WINRT.Views.Controls.MainPage;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.System.Threading;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using XboxMusicLibrary;
using Panel = VLC_WINRT.Model.Panel;

namespace VLC_WINRT.ViewModels.MainPage
{
    public class MusicLibraryViewModel : BindableBase
    {
        public int NbOfFiles = 0;
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
        private bool _isLoaded;
        private bool _isBusy;
        private bool _isMusicLibraryEmpty = true;

        int _numberOfTracks;
        ThreadPoolTimer _periodicTimer;
        readonly AsyncLock _artistLock = new AsyncLock();

        // XBOX Music Stuff
        // REMOVE: Do we need this stuff anymore?
        public MusicHelper XboxMusicHelper = new MusicHelper();
        public Authenication XboxMusicAuthenication;
        ObservableCollection<string> _imgCollection = new ObservableCollection<string>();
        public MusicLibraryViewModel()
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
            if (Artist.Any())
            {
                IsMusicLibraryEmpty = false;
                LoadFavoritesRandomAlbums();
            }

            // TODO: Find a better way to check for new items in the library.
            // This checks for new folders. If there are more or less then what was there before, it will index again.
            NbOfFiles = (await KnownVLCLocation.MusicLibrary.GetItemsAsync()).Count;
            bool isMusicLibraryChanged = await IsMusicLibraryChanged();
            if (isMusicLibraryChanged)
            {
                await StartIndexing();
                return;
            }
            IsLoaded = true;
            IsBusy = false;
        }

        private void LoadFavoritesRandomAlbums()
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

        private async Task StartIndexing()
        {
            // TODO: Rewrite function.
            _artistDataRepository = new ArtistDataRepository();
            var musicFolder = await
                KnownVLCLocation.MusicLibrary.GetFoldersAsync(CommonFolderQuery.GroupByArtist);
            TimeSpan period = TimeSpan.FromSeconds(10);

            _periodicTimer = ThreadPoolTimer.CreatePeriodicTimer(async (source) =>
            {
                if (Locator.MusicLibraryVM.Track.Count > _numberOfTracks)
                {
                    await DispatchHelper.InvokeAsync(() => Locator.MusicLibraryVM._numberOfTracks = Track.Count);
                }
                else
                {
                    _periodicTimer.Cancel();
                    await DispatchHelper.InvokeAsync(() =>
                    {
                        IsLoaded = true;
                        IsBusy = false;
                    });
                }
            }, period);

            using (await _artistLock.LockAsync())
                foreach (var artistItem in musicFolder)
                {
                    IsMusicLibraryEmpty = false;
                    MusicProperties artistProperties = null;
                    try
                    {
                        artistProperties = await artistItem.Properties.GetMusicPropertiesAsync();
                    }
                    catch
                    {
                        Debug.WriteLine("Could not get artist item properties.");
                    }

                    // If we could not get the artist information, skip it and continue.
                    if (artistProperties == null || artistProperties.Artist == string.Empty)
                    {
                        continue;
                    }

                    StorageFolderQueryResult albumQuery =
                        artistItem.CreateFolderQuery(CommonFolderQuery.GroupByAlbum);

                    // Check if artist is in the database. If so, use it.
                    ArtistItem artist = await _artistDataRepository.LoadViaArtistName(artistProperties.Artist);
                    if (artist == null)
                    {
                        artist = new ArtistItem { Name = artistProperties.Artist };
                        Artist.Add(artist);
                        await _artistDataRepository.Add(artist);
                    }
                    await artist.Initialize(albumQuery, artist);
                    OnPropertyChanged("Track");
                    OnPropertyChanged("Artist");
                }
            OnPropertyChanged("Artist");
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
                        album.Tracks = await _trackDataRepository.LoadTracksByAlbumId(album.Id);
                    }
                    artistItem.Albums = albums;
                }
                Artist = artists;
            }
            catch (Exception)
            {
                Debug.WriteLine("Error getting database.");
            }
        }

        public void ExecuteSemanticZoom()
        {
            var page = App.ApplicationFrame.Content as Views.MainPage;
            if (page != null)
            {
                var musicColumn = page.GetFirstDescendantOfType<MusicColumn>() as MusicColumn;
                var albumsByArtistSemanticZoom = musicColumn.GetDescendantsOfType<SemanticZoom>();
                var albumsCollection = musicColumn.Resources["albumsCollection"] as CollectionViewSource;
                if (albumsByArtistSemanticZoom != null)
                {
                    var firstlistview = albumsByArtistSemanticZoom.ElementAt(0).ZoomedOutView as ListViewBase;
                    var secondlistview = albumsByArtistSemanticZoom.ElementAt(1).ZoomedOutView as ListViewBase;
                    if (albumsCollection != null)
                    {
                        firstlistview.ItemsSource = albumsCollection.View.CollectionGroups;
                        secondlistview.ItemsSource = albumsCollection.View.CollectionGroups;
                    }
                }
            }
        }

        async Task<bool> IsMusicLibraryChanged()
        {
            var doesDbExists = await DoesFileExistHelper.DoesFileExistAsync("vlc.sqlite");
            if (doesDbExists)
            {
                if (App.LocalSettings.ContainsKey("nbOfFiles"))
                {
                    if ((int)App.LocalSettings["nbOfFiles"] == NbOfFiles)
                    {
                        return false;
                    }
                    App.LocalSettings.Remove("nbOfFiles");
                }
                App.LocalSettings.Add("nbOfFiles", NbOfFiles);
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
            private List<Utility.Helpers.MusicLibrary.MusicEntities.Album> _onlinePopularAlbumItems;
            private bool _isOnlineRelatedArtistsLoaded;
            private List<Utility.Helpers.MusicLibrary.MusicEntities.Artist> _onlineRelatedArtists;
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
            public List<Utility.Helpers.MusicLibrary.MusicEntities.Album> OnlinePopularAlbumItems
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
            public List<Utility.Helpers.MusicLibrary.MusicEntities.Artist> OnlineRelatedArtists
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

            public async Task Initialize(StorageFolderQueryResult albumQueryResult, ArtistItem artist)
            {
                await LoadAlbums(albumQueryResult, artist.Id);
            }

            private async Task LoadAlbums(StorageFolderQueryResult albumQueryResult, int artistId)
            {
                IReadOnlyList<StorageFolder> albumFolders = null;

                try
                {
                    albumFolders = await albumQueryResult.GetFoldersAsync();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                }
                if (albumFolders != null)
                {
                    foreach (var item in albumFolders)
                    {
                        AlbumItem albumItem = await GetInformationsFromMusicFile.GetAlbumItemFromFolder(item, albumQueryResult, artistId);
                        await albumItem.GetCover();

                        // Album is in database, update with cover.
                        await _albumDataRepository.Update(albumItem);
                        await DispatchHelper.InvokeAsync(() =>
                        {
                            Albums.Add(albumItem);
                            if (Locator.MusicLibraryVM.RandomAlbums.Count < 12)
                            {
                                Locator.MusicLibraryVM.RandomAlbums.Add(albumItem);
                            }
                        });
                        Locator.MusicLibraryVM.AlbumCover.Add(albumItem.Picture);
                    }
                }
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
            private readonly StorageItemThumbnail _storageItemThumbnail;

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
                get { return _picture; }
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

            public AlbumItem(StorageItemThumbnail thumbnail, string name, string artist)
            {
                //DispatchHelper.Invoke(() =>
                //{
                Name = (name.Length == 0) ? "Album without title" : name;
                Artist = artist;
                //});
                _storageItemThumbnail = thumbnail;
            }

            public AlbumItem()
            {
            }

            public async Task GetCover()
            {
                string fileName = Artist + "_" + Name;

                // fileName needs to be scrubbed of some punctuation.
                // For example, Windows does not like question marks in file names.
                fileName = System.IO.Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c, '_'));
                bool hasFoundCover = false;
                if (_storageItemThumbnail != null)
                {
                    var file =
                        await
                            ApplicationData.Current.LocalFolder.CreateFileAsync(
                                fileName + ".jpg",
                                CreationCollisionOption.ReplaceExisting);
                    var raStream = await file.OpenAsync(FileAccessMode.ReadWrite);

                    using (var thumbnailStream = _storageItemThumbnail.GetInputStreamAt(0))
                    {
                        using (var stream = raStream.GetOutputStreamAt(0))
                        {
                            await RandomAccessStream.CopyAsync(thumbnailStream, stream);
                            hasFoundCover = true;
                        }
                    }
                }
                else
                {
                    if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                    {
                        try
                        {
                            HttpClient lastFmClient = new HttpClient();
                            var reponse =
                                await
                                    lastFmClient.GetStringAsync(
                                        string.Format("http://ws.audioscrobbler.com/2.0/?method=album.getinfo&format=json&api_key=a8eba7d40559e6f3d15e7cca1bfeaa1c&artist={0}&album={1}", Artist, Name));
                            {
                                var albumInfo = JsonConvert.DeserializeObject<AlbumInformation>(reponse);
                                if (albumInfo.Album == null)
                                {
                                    return;
                                }
                                if (albumInfo.Album.Image == null)
                                {
                                    return;
                                }
                                // Last.FM returns images from small to 'mega',
                                // So try and get the largest image possible.
                                // If we don't get any album art, or can't find the album, return.
                                var largestImage = albumInfo.Album.Image.LastOrDefault(url => !string.IsNullOrEmpty(url.Text));
                                if (largestImage != null)
                                {
                                    hasFoundCover = true;
                                    await DownloadAndSaveHelper.SaveAsync(
                                        new Uri(largestImage.Text, UriKind.RelativeOrAbsolute),
                                        ApplicationData.Current.LocalFolder,
                                        fileName + ".jpg");
                                }
                            }
                        }
                        catch
                        {
                            Debug.WriteLine("Unable to get album Cover from LastFM API");
                        }
                    }
                }
                if (hasFoundCover)
                {
                    await DispatchHelper.InvokeAsync(() =>
                    {
                        Picture = "ms-appdata:///local/" + Artist + "_" + Name + ".jpg";
                        OnPropertyChanged("Picture");
                    });
                }
            }

            public async Task LoadTracks(IReadOnlyList<StorageFile> tracks)
            {
                if (tracks == null)
                    return;
                int i = 0;
                foreach (var track in tracks)
                {
                    i++;
                    var trackItem = await GetInformationsFromMusicFile.GetTrackItemFromFile(track, Artist, Name, i, ArtistId, Id);
                    var databaseTrack = await _trackDataRepository.LoadTrack(ArtistId, Id, trackItem.Name);
                    if (databaseTrack == null)
                    {
                        await _trackDataRepository.Add(trackItem);
                        Tracks.Add(trackItem);
                    }
                    await DispatchHelper.InvokeAsync(() =>
                    {
                        Locator.MusicLibraryVM.Track.Add(trackItem);
                        OnPropertyChanged("Track");
                    });
                }
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
