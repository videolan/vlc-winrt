using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using VLC_WINRT_APP.Commands.Music;
using VLC_WINRT_APP.Commands.MusicPlayer;
using VLC_WINRT_APP.Common;
using VLC_WINRT_APP.Helpers.MusicLibrary;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Model.Music
{
    public class TrackItem : BindableBase
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
                Task.Run(() => ArtistInformationsHelper.GetAlbumPicture(this));
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

        public bool IsFromSandbox { get; set; }
    }
}
