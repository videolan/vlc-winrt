using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using SQLite;
using VLC.Commands.MusicLibrary;
using VLC.Commands.MusicPlayer;
using VLC.Utils;
using Windows.Storage.AccessCache;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
using libVLCX;
using VLC.Helpers;
using VLC.ViewModels;

namespace VLC.Model.Music
{
    public class TrackItem : BindableBase, IMediaItem
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
        private string _genre;
        private StorageFile _file;
        private BitmapImage _albumImage;
        private LoadingState _albumImageLoadingState = LoadingState.NotLoaded;

        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public bool IsAvailable { get; set; }
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
        public String Token
        {
            get { return _token; }
            set { SetProperty(ref _token, value); }
        }

        [Ignore]
        public TrackClickedCommand TrackClickedCommand { get; } = new TrackClickedCommand();

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

        [Ignore]
        public Media VlcMedia { get; set; }

        public Tuple<FromType, string> GetMrlAndFromType(bool preferToken = false)
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
        
        public bool IsCurrentPlaying()
        {
            if (Locator.MediaPlaybackViewModel.PlaybackService.CurrentPlaylistIndex == -1)
                return false;
            if (!Locator.MediaPlaybackViewModel.PlaybackService.Playlist.Any())
                return false;
            return Id == Locator.MediaPlaybackViewModel.PlaybackService.Playlist[Locator.MediaPlaybackViewModel.PlaybackService.CurrentPlaylistIndex].Id;
        }
    }
}
