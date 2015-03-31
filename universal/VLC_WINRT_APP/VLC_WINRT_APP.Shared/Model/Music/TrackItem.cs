using System;
using System.Threading.Tasks;
using Windows.Storage;
using SQLite;
using VLC_WinRT.Commands.Music;
using VLC_WinRT.Commands.MusicPlayer;
using VLC_WinRT.Common;
using VLC_WinRT.Helpers.MusicLibrary;

namespace VLC_WinRT.Model.Music
{
    public class TrackItem : BindableBase, IVLCMedia
    {
        private string _artistName;
        private string _albumName;
        private string _name;
        private string _path;
        private uint _index;
        private TimeSpan _duration;
        private bool _favorite;
        private int _currentPosition;
        private bool _isCurrentPlaying;
        private string _thumbnail;
        private TrackClickedCommand _trackClickedCommand = new TrackClickedCommand();
        private FavoriteTrackCommand _favoriteTrackCommand = new FavoriteTrackCommand();
        private ArtistClickedCommand _viewArtist = new ArtistClickedCommand();
        private StorageFile _file;

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

        public TimeSpan Duration
        {
            get { return _duration; }
            set { SetProperty(ref _duration, value); }
        }
        public bool Favorite { get { return _favorite; } set { SetProperty(ref _favorite, value); } }

        [Ignore]
        public string Thumbnail
        {
            get
            {
                if(string.IsNullOrEmpty(_thumbnail))
                    _thumbnail = "ms-appdata:///local/albumPic/" + AlbumId + ".jpg";
                return _thumbnail;
            }
            set { SetProperty(ref _thumbnail, value); }
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
        public TrackClickedCommand TrackClicked
        {
            get { return _trackClickedCommand; }
            set { SetProperty(ref _trackClickedCommand, value); }
        }

        [Ignore]
        public ArtistClickedCommand ViewArtist
        {
            get { return _viewArtist; }
            set { SetProperty(ref _viewArtist, value); }
        }

        [Ignore]
        public FavoriteTrackCommand FavoriteTrack
        {
            get { return _favoriteTrackCommand; }
            set { SetProperty(ref _favoriteTrackCommand, value); }
        }

        [Ignore]
        public StorageFile File
        {
            get { return _file; }
            set { SetProperty(ref _file, value); }
        }
    }
}
