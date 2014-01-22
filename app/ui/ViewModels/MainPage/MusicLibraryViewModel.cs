using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using VLC_WINRT.Common;
using VLC_WINRT.Model;
using VLC_WINRT.Utility.Commands;
using VLC_WINRT.Utility.Helpers;

namespace VLC_WINRT.ViewModels.MainPage
{
    public class MusicLibraryViewModel : BindableBase
    {
        private ObservableCollection<Panel> _panels = new ObservableCollection<Panel>();
        public int nbOfFiles = 0;
        private ObservableCollection<ArtistItemViewModel> _artists;
        private ObservableCollection<string> _albumsCover = new ObservableCollection<string>();
        private ObservableCollection<TrackItem> _tracks = new ObservableCollection<TrackItem>();

        private StopVideoCommand _goBackCommand;

        int _numberOfTracks;
        ThreadPoolTimer _periodicTimer;
        public MusicLibraryViewModel()
        {
            _goBackCommand = new StopVideoCommand();
            GetMusicFromLibrary();
            Panels.Add(new Panel("ARTISTS", 0, 1));
            Panels.Add(new Panel("TRACKS", 1, 0.4));
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
            bool isMusicLibraryChanged = await IsMusicLibraryChanged(musicFolder);
            if (isMusicLibraryChanged)
            {
                TimeSpan period = TimeSpan.FromSeconds(60);
                _periodicTimer = ThreadPoolTimer.CreatePeriodicTimer((source) =>
                {
                    App.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                        () =>
                        {
                            if (Locator.MusicLibraryVM.Track.Count > _numberOfTracks)
                            {
                                SerializationHelper.SerializeAsJson(Locator.MusicLibraryVM.Artist, "MusicDB.json",
                                    null,
                                    CreationCollisionOption.ReplaceExisting);
                                Locator.MusicLibraryVM._numberOfTracks = Track.Count;
                            }
                            else
                            {
                                _periodicTimer.Cancel();
                            }
                        });

                }, period);

                _artists = new ObservableCollection<ArtistItemViewModel>();
                foreach (var artistItem in musicFolder)
                {
                    MusicProperties artistProperties = null;
                    try
                    {
                        artistProperties = await artistItem.Properties.GetMusicPropertiesAsync();
                    }
                    catch (Exception e)
                    {
                    }
                    if (artistProperties != null && artistProperties.Artist != "")
                    {
                        StorageFolderQueryResult albumQuery =
                            artistItem.CreateFolderQuery(CommonFolderQuery.GroupByAlbum);
                        var artist = new ArtistItemViewModel(albumQuery);
                        artist.Name = artistProperties.Artist;
                        _artists.Add(artist);
                        OnPropertyChanged("Artist");
                    }
                }
            }
            else
            {
                Artist = await SerializationHelper.LoadFromJsonFile<ObservableCollection<ArtistItemViewModel>>("MusicDB.json");

                foreach (ArtistItemViewModel artist in Artist)
                {
                    foreach (AlbumItem album in artist.Albums)
                    {
                        AlbumCover.Add(album.Picture);
                        foreach (TrackItem trackItem in album.Tracks)
                        {
                            trackItem.IsFavorite = true;
                            Track.Add(trackItem);
                        }
                    }
                }
                OnPropertyChanged("Artist");
                OnPropertyChanged("Albums");
                OnPropertyChanged("Tracks");
            }
        }

        async Task<bool> IsMusicLibraryChanged(IReadOnlyList<StorageFolder> musicFolder)
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

        public class ArtistItemViewModel : BindableBase
        {
            private string _name;
            private string _picture;
            private ObservableCollection<AlbumItem> _albumItems = new ObservableCollection<AlbumItem>();
            private int _currentAlbumIndex;

            // more informations
            private string _biography;
            private List<OnlineAlbumItem> _onlinePopularAlbumItems = new List<OnlineAlbumItem>();
            private bool _isFavorite;

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
                get { return _biography; }
                set { SetProperty(ref _biography, value); }
            }

            public List<OnlineAlbumItem> OnlinePopularAlbumItems
            {
                get
                {
                    return _onlinePopularAlbumItems;
                }
                set { SetProperty(ref _onlinePopularAlbumItems, value); }
            }
            public bool IsFavorite { get { return _isFavorite; } set { SetProperty(ref _isFavorite, value); } }

            public ArtistItemViewModel(StorageFolderQueryResult albumQueryResult)
            {
                LoadAlbums(albumQueryResult);
                GetArtistBiography(albumQueryResult.Folder.DisplayName);
                GetPopularAlbums(albumQueryResult.Folder.DisplayName);
            }

            public ArtistItemViewModel()
            {
            }

            public async Task LoadAlbums(StorageFolderQueryResult albumQueryResult)
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
                        var albumItem = new AlbumItem(files, musicAttr.Album, albumQueryResult.Folder.DisplayName);
                        albumItem.Name = musicAttr.Album;
                        albumItem.Artist = musicAttr.Artist;
                        Albums.Add(albumItem);
                        Locator.MusicLibraryVM.AlbumCover.Add(albumItem.Picture);
                        OnPropertyChanged("Albums");
                    }
                }
            }

            public async Task GetArtistBiography(string artist)
            {
                try
                {
                    HttpClient Fond = new HttpClient();
                    var reponse = await Fond.GetStringAsync("http://ws.audioscrobbler.com/2.0/?method=artist.getinfo&api_key=" + App.ApiKeyLastFm + "&artist=" + artist);
                    {
                        var xml1 = XDocument.Parse(reponse);
                        var img = xml1.Root.Descendants("image").ElementAt(3);
                        if (img != null)
                            Picture = img.Value;

                        var bio = xml1.Root.Descendants("bio").Elements("summary").FirstOrDefault();
                        if (bio != null)
                            Biography = bio.Value;
                    }
                }
                catch
                {

                }
            }
            public async Task GetPopularAlbums(string artist)
            {
                try
                {
                    var Client1 = new HttpClient();
                    var response1 = await Client1.GetStringAsync("http://ws.audioscrobbler.com/2.0/?method=artist.gettopalbums&limit=6&api_key=" + App.ApiKeyLastFm + "&artist=" + artist);
                    {
                        var xml = XDocument.Parse(response1);
                        var topalbums = from results in xml.Descendants("album")
                                        select new OnlineAlbumItem
                                        {
                                            Name = results.Element("name").Value.ToString(),
                                            Picture = results.Elements("image").ElementAt(3).Value,
                                        };

                        foreach (var item in topalbums.ToList())
                            OnlinePopularAlbumItems.Add(item);
                        OnPropertyChanged("OnlinePopularAlbumItems");
                    }
                }
                catch
                { }
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
            private string _picture;
            private uint _year;
            private ObservableCollection<TrackItem> _trackItems = new ObservableCollection<TrackItem>();
            private PlayAlbumCommand _playAlbumCommand = new PlayAlbumCommand();

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

            public AlbumItem(IReadOnlyList<StorageFile> tracks, string name, string artist)
            {
                if (tracks == null) return;
                LoadTracks(tracks, artist);
                ChargementAlbumBio(name, artist);
            }
            public AlbumItem() { }

            public async Task LoadTracks(IReadOnlyList<StorageFile> tracks, string artist)
            {
                int i = 0;
                foreach (var track in tracks)
                {
                    i++;
                    var trackInfos = await track.Properties.GetMusicPropertiesAsync();
                    TrackItem trackItem = new TrackItem();
                    trackItem.ArtistName = artist;
                    trackItem.Name = trackInfos.Title;
                    trackItem.Path = track.Path;
                    trackItem.Duration = trackInfos.Duration;
                    trackItem.Index = i;
                    Tracks.Add(trackItem);
                    OnPropertyChanged("Tracks");
                    OnPropertyChanged("Albums");
                    Locator.MusicLibraryVM.Track.Add(trackItem);
                }
            }

            [XmlIgnore()]
            public PlayAlbumCommand PlayAlbum
            {
                get { return _playAlbumCommand; }
                set { SetProperty(ref _playAlbumCommand, value); }
            }
            public async Task ChargementAlbumBio(string name, string artist)
            {
                try
                {
                    HttpClient Fond = new HttpClient();
                    var reponse =
                        await
                            Fond.GetStringAsync("http://ws.audioscrobbler.com/2.0/?method=album.getinfo&api_key=a8eba7d40559e6f3d15e7cca1bfeaa1c&artist=" + artist + "&album=" + name);
                    {
                        var xml1 = XDocument.Parse(reponse);
                        var firstImage = xml1.Root.Descendants("image").ElementAt(3);
                        if (firstImage != null)
                        {
                            Picture = firstImage.Value;
                        }
                    }
                }
                catch (Exception e)
                {
                }
            }
        }

        public class TrackItem : BindableBase
        {
            private string _artistName;
            private string _name;
            private string _path;
            private int _index;
            private TimeSpan _duration;
            private bool _isFavorite;
            private int _currentPosition;
            private PlayTrackCommand _playTrackCommand = new PlayTrackCommand();

            public string ArtistName
            {
                get { return _artistName; }
                set { SetProperty(ref _artistName, value); }
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
            public bool IsFavorite { get { return _isFavorite; } set { SetProperty(ref _isFavorite, value); } }

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
        }
    }
}
