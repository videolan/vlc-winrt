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
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Newtonsoft.Json;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Commands;
using VLC_WINRT.Utility.Commands.MusicPlayer;
using VLC_WINRT.Utility.Helpers;
using VLC_WINRT.Views.Controls.MainPage;
using XboxMusicLibrary;
using Panel = VLC_WINRT.Model.Panel;
using VLC_WINRT.Utility.Helpers.MusicLibrary;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace VLC_WINRT.ViewModels.MainPage
{
    public class MusicLibraryViewModel : BindableBase
    {
        public int nbOfFiles = 0;
        private ObservableCollection<Panel> _panels = new ObservableCollection<Panel>();
        private ObservableCollection<ArtistItemViewModel> _artists = new ObservableCollection<ArtistItemViewModel>();
        private ObservableCollection<string> _albumsCover = new ObservableCollection<string>();
        private ObservableCollection<TrackItem> _tracks = new ObservableCollection<TrackItem>();
        private ObservableCollection<AlbumItem> _favoriteAlbums = new ObservableCollection<AlbumItem>();
        private ObservableCollection<AlbumItem> _randomAlbums = new ObservableCollection<AlbumItem>();

        private StopVideoCommand _goBackCommand;
        private bool _isLoaded;
        private bool _isBusy;
        private bool _isMusicLibraryEmpty = true;

        int _numberOfTracks;
        ThreadPoolTimer _periodicTimer;

        // XBOX Music Stuff
        public MusicHelper XboxMusicHelper = new MusicHelper();
        public string XBOXMusicToken;
        ObservableCollection<string> _imgCollection = new ObservableCollection<string>();
        public MusicLibraryViewModel()
        {
            _goBackCommand = new StopVideoCommand();
            ThreadPool.RunAsync(GetMusicFromLibrary);
            Panels.Add(new Panel("ARTISTS", 0, 1));
            Panels.Add(new Panel("TRACKS", 1, 0.4));
            Panels.Add(new Panel("FAVORITE ALBUMS", 2, 0.4));
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
        public ObservableCollection<ArtistItemViewModel> Artist
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

        public async void GetMusicFromLibrary(IAsyncAction operation)
        {
            nbOfFiles = (await KnownVLCLocation.MusicLibrary.GetItemsAsync()).Count;
            bool isMusicLibraryChanged = await IsMusicLibraryChanged();
            if (isMusicLibraryChanged)
            {
                StartIndexing();
            }
            else
            {
                DeserializeAndLoad();
            }
        }

        async void StartIndexing()
        {
            var musicFolder = await
                KnownVLCLocation.MusicLibrary.GetFoldersAsync(CommonFolderQuery.GroupByArtist);
            TimeSpan period = TimeSpan.FromSeconds(10);

            _periodicTimer = ThreadPoolTimer.CreatePeriodicTimer(async (source) =>
            {
                if (Locator.MusicLibraryVM.Track.Count > _numberOfTracks)
                {
                    SerializeArtistsDataBase();
                    SerializationHelper.SerializeAsJson(ImgCollection, "Artist_Img_Collection.json", null,
                        CreationCollisionOption.ReplaceExisting);
                    Locator.MusicLibraryVM._numberOfTracks = Track.Count;
                }
                else
                {
                    Task.Run(() => SerializeArtistsDataBase());
                    _periodicTimer.Cancel();
                    DispatchHelper.Invoke(() => IsLoaded = true);
                    DispatchHelper.Invoke(() => IsBusy = false);
                }

            }, period);

            foreach (var artistItem in musicFolder)
            {
                DispatchHelper.Invoke(() => IsMusicLibraryEmpty = false);
                MusicProperties artistProperties = null;
                try
                {
                    artistProperties = await artistItem.Properties.GetMusicPropertiesAsync();
                }
                catch
                {
                }
                if (artistProperties != null && artistProperties.Artist != "")
                {
                    StorageFolderQueryResult albumQuery =
                        artistItem.CreateFolderQuery(CommonFolderQuery.GroupByAlbum);
                    var artist = new ArtistItemViewModel(albumQuery, artistProperties.Artist);
                    DispatchHelper.Invoke(() => OnPropertyChanged("Track"));
                    DispatchHelper.Invoke(() => OnPropertyChanged("Artist"));
                    DispatchHelper.Invoke(() => Artist.Add(artist));
                }
            }
            DispatchHelper.Invoke(() => OnPropertyChanged("Artist"));
        }

        async Task DeserializeAndLoad()
        {
            IsLoaded = true;
            Artist = await SerializationHelper.LoadFromJsonFile<ObservableCollection<ArtistItemViewModel>>("MusicDB.json");
            try
            {
                Artist = await SerializationHelper.LoadFromJsonFile<ObservableCollection<ArtistItemViewModel>>("MusicDB.json");
            }
            catch (SerializationException exception)
            {
                StartIndexing();
                return;
            }

            if (Artist.Count == 0)
            {
                StartIndexing();
                return;
            }
            IsMusicLibraryEmpty = false;
            ImgCollection =
                await
                    SerializationHelper.LoadFromJsonFile<ObservableCollection<string>>("Artist_Img_Collection.json");

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

            IsBusy = false;
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
            var doesDBExists = await DoesFileExistHelper.DoesFileExistAsync("MusicDB.json");
            if (doesDBExists)
            {
                if (App.LocalSettings.ContainsKey("nbOfFiles"))
                {
                    if ((int)App.LocalSettings["nbOfFiles"] == nbOfFiles)
                    {
                        return false;
                    }
                    App.LocalSettings.Remove("nbOfFiles");
                }
                App.LocalSettings.Add("nbOfFiles", nbOfFiles);
            }
            return true;
        }

        public async Task SerializeArtistsDataBase()
        {
            IsBusy = true;
            await SerializationHelper.SerializeAsJson(Artist, "MusicDB.json",
                null,
                CreationCollisionOption.ReplaceExisting);

            IsBusy = false;
        }

        public class ArtistItemViewModel : BindableBase
        {
            private string _name;
            private string _picture;
            private bool _isPictureLoaded = false;
            private ObservableCollection<AlbumItem> _albumItems = new ObservableCollection<AlbumItem>();
            private int _currentAlbumIndex = 0;

            // more informations
            private bool _isFavorite;
            private bool _isOnlinePopularAlbumItemsLoaded = false;
            private List<OnlineAlbumItem> _onlinePopularAlbumItems;
            private bool _isOnlineRelatedArtistsLoaded = false;
            private List<ArtistItemViewModel> _onlineRelatedArtists;
            private string _biography;

            [JsonIgnore()]
            public bool IsOnlinePopularAlbumItemsLoaded
            {
                get { return _isOnlinePopularAlbumItemsLoaded; }
                set { SetProperty(ref _isOnlinePopularAlbumItemsLoaded, value); }
            }

            [JsonIgnore()]
            public bool IsOnlineRelatedArtistsLoaded
            {
                get { return _isOnlineRelatedArtistsLoaded; }
                set { SetProperty(ref _isOnlineRelatedArtistsLoaded, value); }
            }

            public string Name
            {
                get { return _name; }
                set { SetProperty(ref _name, value); }
            }

            [JsonIgnore()]
            public string Picture
            {
                get
                {
                    if (!_isPictureLoaded)
                    {
                        if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                        {
                            // Get Artist Picture via XBOX Music
                            ArtistInformationsHelper.GetArtistPicture(this);
                            _isPictureLoaded = true;
                        }
                    }
                    return _picture;
                }
                set
                {
                    OnPropertyChanged("Picture");
                    SetProperty(ref _picture, value);
                }
            }

            public ObservableCollection<AlbumItem> Albums
            {
                get { return _albumItems; }
                set { SetProperty(ref _albumItems, value); }
            }

            [JsonIgnore()]
            public int CurrentAlbumIndex
            {
                set { SetProperty(ref _currentAlbumIndex, value); }
            }

            [JsonIgnore()]
            public AlbumItem CurrentAlbumItem
            {
                get { return _albumItems[_currentAlbumIndex]; }
            }

            [JsonIgnore()]
            public string Biography
            {
                get
                {
                    if (_biography != null)
                    {
                        return _biography;
                    }
                    else if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                    {
                        ArtistInformationsHelper.GetArtistBiography(this);
                        return "Loading";
                    }
                    else
                    {
                        return "Please verify your internet connection";
                    }
                }
                set { SetProperty(ref _biography, value); }
            }

            [JsonIgnore()]
            public List<OnlineAlbumItem> OnlinePopularAlbumItems
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

            [JsonIgnore()]
            public List<ArtistItemViewModel> OnlineRelatedArtists
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

            public ArtistItemViewModel(StorageFolderQueryResult albumQueryResult, string artistName)
            {
                DispatchHelper.Invoke(() => Name = artistName);
                LoadAlbums(albumQueryResult);
            }

            public ArtistItemViewModel()
            {
            }

            private async Task LoadAlbums(StorageFolderQueryResult albumQueryResult)
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
                        AlbumItem albumItem = await GetInformationsFromMusicFile.GetAlbumItemFromFolder(item, albumQueryResult);
                        albumItem.GetCover();
                        DispatchHelper.Invoke(() => Albums.Add(albumItem));
                        if (Locator.MusicLibraryVM.RandomAlbums.Count < 12)
                        {
                            DispatchHelper.Invoke(() => Locator.MusicLibraryVM.RandomAlbums.Add(albumItem));
                        }
                        Locator.MusicLibraryVM.AlbumCover.Add(albumItem.Picture);
                    }
                }
            }
        }

        public class OnlineAlbumItem : BindableBase
        {
            private string _name;
            private string _artist;
            private string _picture;
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
            public string Picture
            {
                get { return _picture; }
                set { SetProperty(ref _picture, value); }
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
            private StorageItemThumbnail _storageItemThumbnail;

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

            [JsonIgnore()]
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

            [JsonIgnore()]
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

            public AlbumItem(StorageItemThumbnail thumbnail, IReadOnlyList<StorageFile> tracks, string name, string artist)
            {
                if (tracks == null) return;
                //DispatchHelper.Invoke(() =>
                //{
                Name = (name.Length == 0) ? "Album without title" : name;
                Artist = artist;
                //});
                _storageItemThumbnail = thumbnail;
                LoadTracks(tracks);
            }

            public AlbumItem()
            {
            }

            public async Task GetCover()
            {
                string fileName = Artist + "_" + Name;

                bool hasFoundCover = false;
                if (_storageItemThumbnail != null)
                {
                    var file =
                        await
                            ApplicationData.Current.LocalFolder.CreateFileAsync(
                                Artist + "_" + Name + ".jpg",
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
                            HttpClient Fond = new HttpClient();
                            var reponse =
                                await
                                    Fond.GetStringAsync(
                                        "http://ws.audioscrobbler.com/2.0/?method=album.getinfo&api_key=a8eba7d40559e6f3d15e7cca1bfeaa1c&artist=" +
                                        Artist + "&album=" + Name);
                            {
                                var xml1 = XDocument.Parse(reponse);
                                var firstImage = xml1.Root.Descendants("image").ElementAt(3);
                                if (firstImage != null)
                                {
                                    hasFoundCover = true;
                                    DownloadAndSaveHelper.SaveAsync(
                                        new Uri(firstImage.Value, UriKind.RelativeOrAbsolute),
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
                    App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        DispatchHelper.Invoke(() => Picture = "ms-appdata:///local/" + Artist + "_" + Name + ".jpg");
                        DispatchHelper.Invoke(() => OnPropertyChanged("Picture"));
                    });
                }
            }

            public async Task LoadTracks(IReadOnlyList<StorageFile> tracks)
            {
                int i = 0;
                foreach (var track in tracks)
                {
                    i++;
                    var trackItem = await GetInformationsFromMusicFile.GetTrackItemFromFile(track, Artist, Name, i);
                    Tracks.Add(trackItem);
                    DispatchHelper.Invoke(() => Locator.MusicLibraryVM.Track.Add(trackItem));
                    DispatchHelper.Invoke(() => OnPropertyChanged("Track"));
                }
            }

            [JsonIgnore()]
            public PlayAlbumCommand PlayAlbum
            {
                get { return _playAlbumCommand; }
                set { SetProperty(ref _playAlbumCommand, value); }
            }

            [JsonIgnore()]
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

            [JsonIgnore()]
            public int CurrentPosition
            {
                get { return _currentPosition; }
                set { SetProperty(ref _currentPosition, value); }
            }

            [JsonIgnore()]
            public PlayTrackCommand PlayTrack
            {
                get { return _playTrackCommand; }
                set { SetProperty(ref _playTrackCommand, value); }
            }

            [JsonIgnore()]
            public FavoriteTrackCommand FavoriteTrack
            {
                get { return _favoriteTrackCommand; }
                set { SetProperty(ref _favoriteTrackCommand, value); }
            }
        }
    }
}
