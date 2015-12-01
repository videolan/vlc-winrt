using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using SQLite;
using VLC_WinRT.Commands.MusicLibrary;
using VLC_WinRT.Commands.MusicPlayer;
using VLC_WinRT.Utils;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml.Media.Imaging;
using libVLCX;
using VLC_WinRT.Helpers;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Model.Music
{
    public class TrackItem : BindableBase, IVLCMedia
    {
        private string _artistName;
        private string _albumName;
        private string _name;
        private string _path;
        private string _token;
        private uint _index;
        private int _disc;
        private TimeSpan _duration;
        private bool _favorite;
        private int _currentPosition;
        private bool _isCurrentPlaying;
        private string _thumbnail;
        private string _genre;
        private StorageFile _file;
        private BitmapImage _albumImage;
        private LoadingState _albumImageLoadingState = LoadingState.NotLoaded;

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

        [Indexed]
        public string Path
        {
            get { return _path; }
            set { SetProperty(ref _path, value); }
        }

        public uint Index
        {
            get { return _index; }
            set { SetProperty(ref _index, value); }
        }

        public string IndexString
        {
            get
            {
                if (_index < 10)
                {
                    return "0" + _index;
                }
                return _index.ToString();
            }
        }

        public int DiscNumber
        {
            get { return _disc; }
            set { SetProperty(ref _disc, value); }
        }

        public TimeSpan Duration
        {
            get { return _duration; }
            set { SetProperty(ref _duration, value); }
        }

        public bool Favorite { get { return _favorite; } set { SetProperty(ref _favorite, value); } }

        public string Genre
        {
            get { return _genre; }
            set { SetProperty(ref _genre, value); }
        }

        [Ignore]
        public string Thumbnail
        {
            get
            {
                if (string.IsNullOrEmpty(_thumbnail))
                    _thumbnail = Locator.MusicLibraryVM.MusicLibrary.LoadAlbum(this.AlbumId)?.AlbumCoverFullUri;
                return _thumbnail;
            }
            set { SetProperty(ref _thumbnail, value); }
        }
        
        [Ignore]
        public BitmapImage AlbumImage
        {
            get
            {
                if (_albumImage == null && _albumImageLoadingState == LoadingState.NotLoaded)
                {
                    _albumImageLoadingState = LoadingState.Loading;
                    Task.Run(() => ResetAlbumArt());
                }
                return _albumImage;
            }
            set { SetProperty(ref _albumImage, value); }
        }

        public Task ResetAlbumArt()
        {
            return LoadImageToMemoryHelper.LoadImageToMemory(this);
        }

        [Ignore]
        public int CurrentPosition
        {
            get { return _currentPosition; }
            set { SetProperty(ref _currentPosition, value); }
        }

        [Ignore]
        public bool IsCurrentPlaying
        {
            get
            {
                return _isCurrentPlaying;
            }
            set { SetProperty(ref _isCurrentPlaying, value); }
        }

        [Ignore]
        public String Token
        {
            get { return _token; }
            set { SetProperty(ref _token, value); }
        }

        [Ignore]
        public TrackClickedCommand TrackClicked { get; } = new TrackClickedCommand();

        [Ignore]
        public ArtistClickedCommand ViewArtist { get; } = new ArtistClickedCommand();

        [Ignore]
        public FavoriteTrackCommand FavoriteTrack { get; } = new FavoriteTrackCommand();

        [Ignore]
        public StorageFile File
        {
            get { return _file; }
            set { SetProperty(ref _file, value); }
        }

        public Tuple<FromType, string> GetMrlAndFromType()
        {
            if (!string.IsNullOrEmpty(_token))
            {
                // Using an already created token
                return new Tuple<FromType, string>(FromType.FromLocation, "winrt://" + _token);
            }
            if (File != null && string.IsNullOrEmpty(Path))
            {
                // Using a Token
                // FromLocation : 1
                return new Tuple<FromType, string>(FromType.FromLocation, "winrt://" + StorageApplicationPermissions.FutureAccessList.Add(File));
            }
            if (!string.IsNullOrEmpty(Path))
                return new Tuple<FromType, string>(FromType.FromPath, Path);
            return null;
        }
    }
}
