using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using SQLite;
using VLC_WINRT_APP.Commands.Music;
using VLC_WINRT_APP.Common;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Helpers.MusicLibrary;
using VLC_WINRT_APP.Helpers.MusicLibrary.MusicEntities;
using VLC_WINRT_APP.ViewModels.MusicVM;

namespace VLC_WINRT_APP.Model.Music
{

    public class ArtistItem : BindableBase
    {
        private string _name;
        private string _picture;
        private bool _isPictureLoaded;
        private ObservableCollection<AlbumItem> _albumItems;
        private bool _isAlbumsLoaded = false;

        // more informations
        private bool _isFavorite;
        private bool _isOnlinePopularAlbumItemsLoaded = false;
        private List<Album> _onlinePopularAlbumItems;
        private bool _isOnlineRelatedArtistsLoaded = false;
        private List<Artist> _onlineRelatedArtists;
        private bool _isOnlineMusicVideosLoaded = false;
        private string _biography;
        private List<ShowItem> _upcomingShowItems;
        private bool _isUpcomingShowsLoading = false;
        private bool _isUpcomingShowsItemsLoaded = false;
        private PinArtistCommand pinArtistCommand;
#if WINDOWS_PHONE_APP
        private SeeArtistShowsCommand seeArtistShowsCommand;
#endif

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

        [Ignore]
        public string Picture
        {
            get
            {
                if (!_isPictureLoaded)
                {
                    _isPictureLoaded = true;
                    _picture = "ms-appx:///Assets/NoCover.jpg";
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

        public bool IsPictureLoaded
        {
            get { return _isPictureLoaded; }
            set
            {
                SetProperty(ref _isPictureLoaded, value);
                if (value)
                {
                    Picture = "ms-appdata:///local/artistPic/" + Id + ".jpg";
                }
            }
        }


        private async Task LoadPicture()
        {
            try
            {
                await ArtistInformationsHelper.GetArtistPicture(this);
            }
            catch (Exception)
            {
                LogHelper.Log("Error getting artist picture : " + _name);
            }
        }

        [Ignore]
        public ObservableCollection<AlbumItem> Albums
        {
            get
            {
                if (!_isAlbumsLoaded)
                {
                    _isAlbumsLoaded = true;
                    Task.Run(async () => await this.PopulateAlbums());
                }
                return _albumItems ?? (_albumItems = new ObservableCollection<AlbumItem>());
            }
            set { SetProperty(ref _albumItems, value); }
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
                Task.Run(async () => await ArtistInformationsHelper.GetArtistBiography(this));
                return "Loading";
            }
            set { SetProperty(ref _biography, value); }
        }

        [Ignore]
        public List<Album> OnlinePopularAlbumItems
        {
            get
            {
                if (_onlinePopularAlbumItems != null)
                    return _onlinePopularAlbumItems;
                if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                    Task.Run(() => ArtistInformationsHelper.GetArtistTopAlbums(this));
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
                    Task.Run(() => ArtistInformationsHelper.GetArtistSimilarsArtist(this));
                return null;
            }
            set { SetProperty(ref _onlineRelatedArtists, value); }
        }

        [Ignore]
        public PinArtistCommand PinArtistCommand
        {
            get { return pinArtistCommand ?? (pinArtistCommand = new PinArtistCommand()); }
        }


#if WINDOWS_PHONE_APP
        [Ignore]
        public SeeArtistShowsCommand SeeArtistShowsCommand
        {
            get
            {
                return seeArtistShowsCommand ?? (seeArtistShowsCommand = new SeeArtistShowsCommand());
            }
        }
#endif

        [Ignore]
        public List<ShowItem> UpcomingShows
        {
            get
            {
                if (!_isUpcomingShowsItemsLoaded)
                {
                    IsUpcomingShowsLoading = true;
                    _isUpcomingShowsItemsLoaded = true;
                    Task.Run(() => ArtistInformationsHelper.GetArtistEvents(this));
                }
                return _upcomingShowItems;
            }
            set { SetProperty(ref _upcomingShowItems, value); }
        }

        [Ignore]
        public bool IsUpcomingShowsLoading
        {
            get
            {
                return _isUpcomingShowsLoading;
            }
            set { SetProperty(ref _isUpcomingShowsLoading, value); }
        }

        public bool IsFavorite
        {
            get { return _isFavorite; }
            set { SetProperty(ref _isFavorite, value); }
        }
    }
}
