using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using VLC_WINRT_APP.Commands.Music;
using VLC_WINRT_APP.Commands.MusicPlayer;
using VLC_WINRT_APP.Common;
using VLC_WINRT_APP.Helpers.MusicLibrary;
using VLC_WINRT_APP.ViewModels.MusicVM;

namespace VLC_WINRT_APP.Model.Music
{

    public class AlbumItem : BindableBase
    {
        private string _name;
        private string _artist;
        private string _picture = "/Assets/GreyPylon/280x156.jpg";
        private uint _year;
        private bool _favorite;
        private bool _isPictureLoaded;
        private bool _isTracksLoaded = false;
        private ObservableCollection<TrackItem> _trackItems;
        private PlayAlbumCommand _playAlbumCommand = new PlayAlbumCommand();
        private FavoriteAlbumCommand _favoriteAlbumCommand = new FavoriteAlbumCommand();
        private AlbumTrackClickedCommand _trackClickedCommand = new AlbumTrackClickedCommand();
        private ArtistClickedCommand _viewArtist = new ArtistClickedCommand();

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
            get
            {
                if (_isTracksLoaded)
                {
                    _isTracksLoaded = true;
                    Task.Run(async () => await this.GetTracks());
                }
                return _trackItems ?? (_trackItems = new ObservableCollection<TrackItem>());
            }
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
            set
            {
                SetProperty(ref _picture, value);
                OnPropertyChanged();
            }
        }

        public uint Year
        {
            get { return _year; }
            set { SetProperty(ref _year, value); }
        }

        private async Task LoadPicture()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()) return;
            try
            {
                await ArtistInformationsHelper.GetAlbumPicture(this);
            }
            catch (Exception)
            {
                // TODO: Tell user we could not get their album art.
                Debug.WriteLine("Error getting album art...");
            }
            _isPictureLoaded = true;
        }

        [Ignore]
        public ArtistClickedCommand ViewArtist
        {
            get { return _viewArtist; }
            set { SetProperty(ref _viewArtist, value); }
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
        public AlbumTrackClickedCommand TrackClicked
        {
            get { return _trackClickedCommand; }
            set { SetProperty(ref _trackClickedCommand, value); }
        }
    }
}
