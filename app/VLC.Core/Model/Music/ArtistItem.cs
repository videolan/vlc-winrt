using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using SQLite;
using VLC.Commands.MusicLibrary;
using VLC.Helpers;
using VLC.MusicMetaFetcher.Models.MusicEntities;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Model.Music
{
    public class ArtistItem : BindableBase
    {
        private string _name;
        private bool _isPictureLoaded;
        private ObservableCollection<AlbumItem> _albumItems;
        private ObservableCollection<GroupItemList<TrackItem>> _albumsGrouped;

        private bool _isAlbumsLoaded = false;

        // more informations
        private int _playCount;
        private bool _isFavorite;
        private bool _isOnlinePopularAlbumItemsLoaded = false;
        private List<Album> _onlinePopularAlbumItems;
        private bool _isOnlineRelatedArtistsLoaded = false;
        private List<Artist> _onlineRelatedArtists;
        private bool _isOnlineMusicVideosLoaded = false;
        private string _biography;
        private bool _isPinned;

        private BitmapImage _artistImage;
        private LoadingState _artistImageLoadingState = LoadingState.NotLoaded;

        private BitmapImage _artistThumbnail;
        private LoadingState _artistThumbnailLoadingState = LoadingState.NotLoaded;

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

        [Indexed]
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        [Ignore]
        public BitmapImage ArtistImage
        {
            get
            {
                if (_artistImage == null && _artistImageLoadingState == LoadingState.NotLoaded)
                {
                    _artistImageLoadingState = LoadingState.Loading;
                    ResetArtistPicture(false);
                }

                return _artistImage;
            }
            set { SetProperty(ref _artistImage, value); }
        }

        [Ignore]
        public BitmapImage ArtistImageThumbnail
        {
            get
            {
                if (_artistThumbnail == null && _artistThumbnailLoadingState == LoadingState.NotLoaded)
                {
                    _artistThumbnailLoadingState = LoadingState.Loading;
                    ResetArtistPicture(true);
                }

                return _artistThumbnail;
            }
            set { SetProperty(ref _artistThumbnail, value); }
        }

        public Task ResetArtistPicture(bool thumbnail)
        {
            return Task.Factory.StartNew(() => LoadImageToMemoryHelper.LoadImageToMemory(this, thumbnail));
        }
        
        [Ignore]
        public string Picture => IsPictureLoaded ? string.Format("ms-appdata:///local/artistPic-fullsize/{0}.jpg", Id) : null;

        [Ignore]
        public string PictureThumbnail => IsPictureLoaded ? string.Format("ms-appdata:///local/artistPic-thumbnail/{0}.jpg", Id) : null;

        public bool IsPictureLoaded
        {
            get { return _isPictureLoaded; }
            set { SetProperty(ref _isPictureLoaded, value); }
        }

        public Task LoadBio()
        {
            if (string.IsNullOrEmpty(_biography))
                return Task.Run(async () => await Locator.MusicMetaService.GetArtistBiography(this));
            return null;
        }

        [Ignore]
        public ObservableCollection<AlbumItem> Albums
        {
            get
            {
                if (!_isAlbumsLoaded)
                {
                    _isAlbumsLoaded = true;
                    Task.Run(async () => await Locator.MediaLibrary.PopulateAlbums(this));
                }
                return _albumItems ?? (_albumItems = new ObservableCollection<AlbumItem>());
            }
            set { SetProperty(ref _albumItems, value); }
        }


        [Ignore]
        public ObservableCollection<GroupItemList<TrackItem>> AlbumsGrouped
        {
            get
            {

                if (!_isAlbumsLoaded)
                {
                    _isAlbumsLoaded = true;
                    Task.Run(async () => await Locator.MediaLibrary.PopulateAlbumsWithTracks(this));
                }
                return _albumsGrouped;
            }
            set { SetProperty(ref _albumsGrouped, value); }
        }

        public string Biography
        {
            get
            {
                if (!string.IsNullOrEmpty(_biography) && _biography != "Loading")
                {
                    return _biography;
                }
                return null;
            }
            set
            {
                SetProperty(ref _biography, value);
                OnPropertyChanged(nameof(BiographyString));
            }
        }

        [Ignore]
        public string BiographyString
        {
            get
            {
                if (string.IsNullOrEmpty(_biography))
                    Task.Run(() => LoadBio());
                return _biography;
            }
        }

        [Ignore]
        public List<Album> OnlinePopularAlbumItems
        {
            get
            {
                if (_onlinePopularAlbumItems != null)
                    return _onlinePopularAlbumItems;
                if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                    Task.Run(() => Locator.MusicMetaService.GetPopularAlbums(this));
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
                    Task.Run(() => Locator.MusicMetaService.GetSimilarArtists(this));
                return null;
            }
            set { SetProperty(ref _onlineRelatedArtists, value); }
        }
        
        public bool IsPinned
        {
            get { return _isPinned; }
            set { SetProperty(ref _isPinned, value); }
        }

        public bool IsFavorite
        {
            get { return _isFavorite; }
            set { SetProperty(ref _isFavorite, value); }
        }

        public int PlayCount
        {
            get { return _playCount; }
            set { SetProperty(ref _playCount, value); }
        }

        #region commands
        [Ignore]
        public PinArtistCommand PinArtistCommand { get; } = new PinArtistCommand();

        [Ignore]
        public SeeArtistShowsCommand SeeArtistShowsCommand { get; } = new SeeArtistShowsCommand();
        #endregion
    }
}
