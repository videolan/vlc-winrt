using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Commands;
using VLC_WINRT.Utility.Commands.MusicPlayer;
using VLC_WINRT.Utility.Helpers;
using VLC_WINRT.Views.Controls.MainPage;
using Panel = VLC_WINRT.Model.Panel;
using VLC_WINRT.Utility.Helpers.MusicLibrary;

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

        int _numberOfTracks;
        ThreadPoolTimer _periodicTimer;

        // XBOX Music Stuff; Take 2
        ObservableCollection<string> _imgCollection = new ObservableCollection<string>();
        public MusicLibraryViewModel()
        {
            _goBackCommand = new StopVideoCommand();
            GetMusicFromLibrary();
            Panels.Add(new Panel("ARTISTS", 0, 1));
            Panels.Add(new Panel("TRACKS", 1, 0.4));
            Panels.Add(new Panel("FAVORITE ALBUMS", 2, 0.4));

        }

        public bool IsLoaded
        {
            get { return _isLoaded; }
            set { SetProperty(ref _isLoaded, value); }
        }

        public bool IsMusicLibraryEmpty
        {
            get { return Track.Any(); }
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

        public async Task GetMusicFromLibrary()
        {
            var musicFolder = await
                KnownVLCLocation.MusicLibrary.GetFoldersAsync(CommonFolderQuery.GroupByArtist);

            nbOfFiles = (await KnownVLCLocation.MusicLibrary.GetItemsAsync()).Count;
            bool isMusicLibraryChanged = await IsMusicLibraryChanged();
            if (isMusicLibraryChanged)
            {
                TimeSpan period = TimeSpan.FromSeconds(30);
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
                        foreach (AlbumItem album in Artist.SelectMany(artist => artist.Albums))
                        {
                            await album.GetCover();
                        }
                        DispatchHelper.Invoke(() => IsLoaded = true);
                        SerializeArtistsDataBase();
                        _periodicTimer.Cancel();
                    }

                }, period);

                foreach (var artistItem in musicFolder)
                {
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
                        OnPropertyChanged("Track");
                        OnPropertyChanged("Artist");
                        Artist.Add(artist);
                    }
                }
                ExecuteSemanticZoom();
                OnPropertyChanged("Artist");
            }
            else
            {
                IsLoaded = true;
                Artist = await SerializationHelper.LoadFromJsonFile<ObservableCollection<ArtistItemViewModel>>("MusicDB.json");
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
                ExecuteSemanticZoom();
                OnPropertyChanged("Artist");
                OnPropertyChanged("Albums");
                OnPropertyChanged("Tracks");
            }
        }

        void ExecuteSemanticZoom()
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

        public void SerializeArtistsDataBase()
        {
            SerializationHelper.SerializeAsJson(Artist, "MusicDB.json",
                null,
                CreationCollisionOption.ReplaceExisting);
        }

        public class ArtistItemViewModel : BindableBase
        {
            private string _name;
            private string _picture = "/Assets/GreyPylon/280x156.jpg";
            private ObservableCollection<AlbumItem> _albumItems = new ObservableCollection<AlbumItem>();
            private int _currentAlbumIndex = 0;

            // more informations
            private string _biography;
            private List<OnlineAlbumItem> _onlinePopularAlbumItems = new List<OnlineAlbumItem>();
            private List<ArtistItemViewModel> _onlineRelatedArtists = new List<ArtistItemViewModel>();
            private bool _isFavorite;
            private ArtistInformationsHelper informationHelper;

            public string Name
            {
                get { return _name; }
                set { SetProperty(ref _name, value); }
            }

            public string Picture
            {
                get { return _picture; }
                set { SetProperty(ref _picture, value); }
            }

            public ObservableCollection<AlbumItem> Albums
            {
                get { return _albumItems; }
                set { SetProperty(ref _albumItems, value); }
            }

            public int CurrentAlbumIndex
            {
                set { SetProperty(ref _currentAlbumIndex, value); }
            }

            public AlbumItem CurrentAlbumItem
            {
                get { return _albumItems[_currentAlbumIndex]; }
            }

            public string Biography
            {
                get
                {
                    if (_biography == null) GetBiography();
                    return _biography;
                }
                set { SetProperty(ref _biography, value); }
            }

            public List<OnlineAlbumItem> OnlinePopularAlbumItems
            {
                get { return _onlinePopularAlbumItems; }
                set { SetProperty(ref _onlinePopularAlbumItems, value); }
            }

            public List<ArtistItemViewModel> OnlineRelatedArtists
            {
                get { return _onlineRelatedArtists; }
                set { SetProperty(ref _onlineRelatedArtists, value); }
            }

            public bool IsFavorite
            {
                get { return _isFavorite; }
                set { SetProperty(ref _isFavorite, value); }
            }

            public ArtistItemViewModel(StorageFolderQueryResult albumQueryResult, string artistName)
            {
                Name = artistName;
                ArtistInformations();
                LoadAlbums(albumQueryResult);
            }

            public ArtistItemViewModel()
            {
            }

            private async Task ArtistInformations()
            {
                if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                {
                    informationHelper = new ArtistInformationsHelper();
                    await informationHelper.GetArtistFromXboxMusic(Name);
                    Picture = await informationHelper.GetArtistPicture();
                    OnlineRelatedArtists = await informationHelper.GetArtistSimilarsArtist();
                    OnPropertyChanged("OnlineRelatedArtists");
                    OnlinePopularAlbumItems = await informationHelper.GetArtistTopAlbums();
                    OnPropertyChanged("OnlinePopularAlbumsItems");
                }
            }

            private async Task GetBiography()
            {
                if(System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                    Biography = await informationHelper.GetArtistBiography();
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
                    new MessageDialog(e.ToString()).ShowAsync();
                }
                if (albumFolders != null)
                {
                    foreach (var item in albumFolders)
                    {
                        var musicAttr = await item.Properties.GetMusicPropertiesAsync();
                        var files = await item.GetFilesAsync(CommonFileQuery.OrderByMusicProperties);
                        var thumbnail = await item.GetThumbnailAsync(ThumbnailMode.MusicView, 250);
                        var albumItem = new AlbumItem(thumbnail, files, musicAttr.Album, albumQueryResult.Folder.DisplayName);

                        Albums.Add(albumItem);
                        if (Locator.MusicLibraryVM.RandomAlbums.Count < 12)
                        {
                            Locator.MusicLibraryVM.RandomAlbums.Add(albumItem);
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

            [XmlIgnore()]
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

            [XmlIgnore()]
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
                Name = (name.Length == 0) ? "Album without title" : name;
                Artist = artist;
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
                        }
                    }
                }
                if (hasFoundCover)
                {
                    App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        DispatchHelper.Invoke(() => Picture = "ms-appdata:///local/" + Artist + "_" + Name + ".jpg");
                        OnPropertyChanged("Picture");
                    });
                }
            }

            public async Task LoadTracks(IReadOnlyList<StorageFile> tracks)
            {
                int i = 0;
                foreach (var track in tracks)
                {
                    i++;
                    var trackInfos = await track.Properties.GetMusicPropertiesAsync();
                    TrackItem trackItem = new TrackItem();
                    trackItem.ArtistName = Artist;
                    trackItem.AlbumName = Name;
                    trackItem.Name = trackInfos.Title;
                    trackItem.Path = track.Path;
                    trackItem.Duration = trackInfos.Duration;
                    trackItem.Index = i;
                    Tracks.Add(trackItem);
                    Locator.MusicLibraryVM.Track.Add(trackItem);
                    OnPropertyChanged("Track");
                }
            }

            [XmlIgnore()]
            public PlayAlbumCommand PlayAlbum
            {
                get { return _playAlbumCommand; }
                set { SetProperty(ref _playAlbumCommand, value); }
            }

            [XmlIgnore()]
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

            [XmlIgnore()]
            public int CurrentPosition
            {
                get { return _currentPosition; }
                set { SetProperty(ref _currentPosition, value); }
            }

            [XmlIgnore()]
            public PlayTrackCommand PlayTrack
            {
                get { return _playTrackCommand; }
                set { SetProperty(ref _playTrackCommand, value); }
            }

            [XmlIgnore()]
            public FavoriteTrackCommand FavoriteTrack
            {
                get { return _favoriteTrackCommand; }
                set { SetProperty(ref _favoriteTrackCommand, value); }
            }
        }
    }
}